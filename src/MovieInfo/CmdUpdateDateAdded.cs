using Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace MovieInfo
{
    class CmdUpdateDateAdded : IAsyncCommand
    {
        #region Constructor

        public CmdUpdateDateAdded(CacheData cacheData) 
        { 
            m_cacheData = cacheData;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            List<MovieData> movies = new List<MovieData>();
            lock (m_cacheData)
            {
                foreach (var movie in m_cacheData.Movies)
                {
                    if (String.IsNullOrEmpty(movie.Metadata.DateAdded))
                    {
                        movies.Add(movie);
                    }
                }
            }

            // This may be a long-running operation, so only take the cache lock
            // individually, rather than for the entire operation.
            foreach (var movie in movies)
            {
                lock (m_cacheData)
                {
                    string metadataFullPath = Path.Combine(movie.Path, movie.MetadataFileName);
                    DateTime dt = File.GetCreationTime(metadataFullPath);
                    movie.Metadata.DateAdded = Utilities.DateTimeToString(dt.Year, dt.Month, dt.Day);
                    movie.MetadataChanged = true;
                }
            }
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;

        #endregion
    }
}
