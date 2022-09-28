using System.IO;

namespace MovieInfo
{
    public class CmdSave : IAsyncCommand
    {
        #region Constructors

        public CmdSave(CacheData cacheData, string cacheFilename, ActressesDatabase actresses, string actressesFilename, BackupData backupData, string backupFilename)
        {
            m_cacheData = cacheData;
            m_cacheFilename = cacheFilename;
            m_actresses = actresses;
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

                    // Save cache data
                    MovieSerializer<CacheData>.Save(m_cacheFilename, m_cacheData);

                    // Save backup data
                    MovieSerializer<BackupData>.Save(m_backupFilename, m_backupData);
                }
            }

            lock(m_actresses)
            {
                // Save actresses data
                MovieSerializer<ActressesDatabase>.Save(m_actressesFilename, m_actresses);
            }
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private string m_cacheFilename;
        private ActressesDatabase m_actresses;
        private string m_actressesFilename;
        private BackupData m_backupData;
        private string m_backupFilename;

        #endregion
    }
}
