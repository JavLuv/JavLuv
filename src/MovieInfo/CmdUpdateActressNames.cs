using System.Collections.Generic;
using System.Xml.Linq;

namespace MovieInfo
{
    public class CmdUpdateActressNames : IAsyncCommand
    {
        #region Constructors

        public CmdUpdateActressNames(CacheData cacheData, ActressesDatabase actressesDatabase)
        {
            m_cacheData = cacheData;
            m_actressesDatabase = actressesDatabase;
        }
            
        #endregion

        #region Public Functions

        public void Execute()
        {
            lock (m_actressesDatabase)
            {
                // First regenerate alt names
                m_actressesDatabase.AltNames.Clear();
                foreach (var actress in m_actressesDatabase.Actresses)
                {
                    foreach (var alias in actress.AltNames)
                        m_actressesDatabase.AltNames.Add(new AltNameData(alias, actress.Name));
                }

                // Now update all movie metadata with updated names.  The primary goal
                // is to eliminate duplicates and standardize use of the primary name key
                // instead of listing alternate names or spellings.
                lock (m_cacheData)
                {
                     foreach (MovieData movie in m_cacheData.Movies)
                        UpdateActors(movie);
               }
            }
        }

        #endregion

        #region Private Functions

        private void UpdateActors(MovieData movie)
        {
            foreach (var actor in movie.Metadata.Actors)
            {
                ActressData actress = null;
                if (FoundActress(actor, out actress))
                {
                    // If we're synced, just continue
                    if (IsSynchronized(actor, actress))
                        continue;

                    // Match actor to actress data
                    Syncronize(actor, actress);
                    movie.MetadataChanged = true;
                }
            }

            // If anything has changed, check actor names for duplicates
            if (movie.MetadataChanged)
            {
                bool foundDuplicate = false;
                HashSet<string> actorNames = new HashSet<string>();
                foreach (var actor in movie.Metadata.Actors)
                {
                    if (actorNames.Contains(actor.Name))
                    {
                        foundDuplicate = true;
                        break;
                    }
                    actorNames.Add(actor.Name);
                }
                
                // If we found duplicates, clear actors and re-add them from authoritative actress data
                if (foundDuplicate)
                {
                    movie.Metadata.Actors.Clear();
                    foreach (var name in actorNames)
                    {
                        ActressData actress = null;                    
                        if (m_actressesDatabase.Actresses.TryGetValue(new ActressData(name), out actress))
                        {
                            var newActor = new ActorData(actress.Name);
                            foreach (string altName in actress.AltNames)
                                newActor.Aliases.Add(altName);
                            movie.Metadata.Actors.Add(newActor);
                        }
                    }
                }
            }
        }

        private bool FoundActress(ActorData actor, out ActressData actress)
        {
            actress = null;
            if (FoundActress(actor.Name, out actress))
                return true;

            foreach (string altName in actor.Aliases)
            {
                if (FoundActress(altName, out actress))
                    return true;
            }

            return false;
        }

        private bool FoundActress(string name, out ActressData actress)
        {
            actress = null;

            // First check main actress names
            if (m_actressesDatabase.Actresses.TryGetValue(new ActressData(name), out actress))
                return true;

            // Otherwise, check alt names
            AltNameData altNameData = null;
            if (m_actressesDatabase.AltNames.TryGetValue(new AltNameData(name), out altNameData))
            {
                // If we found an alt name, check the actress names and return the match
                if (m_actressesDatabase.Actresses.TryGetValue(new ActressData(altNameData.Name), out actress))
                    return true;
            }

            return false;
        }

        private bool IsSynchronized(ActorData actorData, ActressData actressData)
        {
            if (actorData.Name != actressData.Name)
                return false;

            if (actorData.Aliases.Count != actressData.AltNames.Count)
                return false;

            foreach (var altName in actressData.AltNames)
            {
                bool foundName = false;
                foreach (var alias in actorData.Aliases)
                {
                    if (altName == alias)
                    {
                        foundName = true;
                        break;
                    }
                }
                if (foundName == false)
                    return false;
            }

            return true;
        }

        private void Syncronize(ActorData actorData, ActressData actressData)
        {
            actorData.Name = actressData.Name;
            actorData.Aliases.Clear();
            foreach (var altName in actressData.AltNames)
                actorData.Aliases.Add(altName);
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private ActressesDatabase m_actressesDatabase;

        #endregion
    }
}
