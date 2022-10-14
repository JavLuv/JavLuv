using System.Collections.Generic;

namespace MovieInfo
{
    public class CmdUpdateActressNames : IAsyncCommand
    {
        #region Constructors

        public CmdUpdateActressNames(MovieCollection collection, CacheData cacheData, ActressesDatabase actressesDatabase)
        {
            m_collection = collection;
            m_cacheData = cacheData;
            m_actressesDatabase = actressesDatabase;
        }
            
        #endregion

        #region Public Functions

        public void Execute()
        {
            lock (m_actressesDatabase)
            {
                // First regenerate japanese and alt names
                m_actressesDatabase.JapaneseNames.Clear();
                m_actressesDatabase.AltNames.Clear();
                foreach (var actress in m_actressesDatabase.Actresses)
                {
                    if (string.IsNullOrEmpty(actress.JapaneseName) == false)
                        m_actressesDatabase.JapaneseNames.Add(new NamePair(actress.JapaneseName, actress.Name));
                    foreach (var alias in actress.AltNames)
                        m_actressesDatabase.AltNames.Add(new NamePair(alias, actress.Name));
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
            actress = m_collection.FindActress(actor.Name);
            if (actress != null)
                return true;

            foreach (string altName in actor.Aliases)
            {
                actress = m_collection.FindActress(altName);
                if (actress != null)
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

        private MovieCollection m_collection;
        private CacheData m_cacheData;
        private ActressesDatabase m_actressesDatabase;

        #endregion
    }
}
