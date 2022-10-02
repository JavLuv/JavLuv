using System.IO;

namespace MovieInfo
{
    public class CmdSave : IAsyncCommand
    {
        #region Constructors

        public CmdSave(CacheData cacheData, string cacheFilename, ActressesDatabase actressesDatabase, string actressesFilename, BackupData backupData, string backupFilename)
        {
            m_cacheData = cacheData;
            m_cacheFilename = cacheFilename;
            m_actressesDatabase = actressesDatabase;
            m_actressesFilename = actressesFilename;
            m_backupData = backupData;
            m_backupFilename = backupFilename;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            lock (m_cacheData)
            {
                lock (m_actressesDatabase)
                {
                    lock (m_backupData)
                    {                  
                        // Save any metadata whose information has changed
                        foreach (var movie in m_cacheData.Movies)
                        {
                            if (movie.MetadataChanged)
                            {
                                string path = Path.Combine(movie.Path, movie.MetadataFileName);
                                MovieSerializer<MovieMetadata>.Save(path, movie.Metadata);
                                movie.MetadataChanged = false;
                            }

                            // Add this metadata to backup data
                            if (m_backupData.Movies.Contains(movie.Metadata))
                                m_backupData.Movies.Remove(movie.Metadata);
                            m_backupData.Movies.Add(movie.Metadata);
                        }

                        // Save existing actresses
                        foreach (var actress in m_actressesDatabase.Actresses)
                        {
                            if (m_backupData.Actresses.Contains(actress))
                                m_backupData.Actresses.Remove(actress);
                            m_backupData.Actresses.Add(actress);
                            foreach (var altName in actress.AltNames)
                            {
                                var key = new NamePair(altName);
                                if (m_backupData.AltNames.Contains(key))
                                    m_backupData.AltNames.Remove(key);
                                m_backupData.AltNames.Add(new NamePair(altName, actress.Name));
                            }
                        }

                        // Save cache data
                        MovieSerializer<CacheData>.Save(m_cacheFilename, m_cacheData);

                        // Save actresses data
                        MovieSerializer<ActressesDatabase>.Save(m_actressesFilename, m_actressesDatabase);

                        // Save backup data
                        MovieSerializer<BackupData>.Save(m_backupFilename, m_backupData);
                    }
                }
            }
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private string m_cacheFilename;
        private ActressesDatabase m_actressesDatabase;
        private string m_actressesFilename;
        private BackupData m_backupData;
        private string m_backupFilename;

        #endregion
    }
}
