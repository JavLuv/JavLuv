using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MovieInfo
{
    public class CmdSave : IAsyncCommand
    {
        #region Constructors

        public CmdSave(CacheData cacheData, string cacheFilename, BackupData backupData, string backupFilename)
        {
            m_cacheData = cacheData;
            m_cacheFilename = cacheFilename;
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
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private string m_cacheFilename;
        private BackupData m_backupData;
        private string m_backupFilename;

        #endregion
    }
}
