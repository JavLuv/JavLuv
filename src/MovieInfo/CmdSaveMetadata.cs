using System.IO;

namespace MovieInfo
{
    public class CmdSaveMetadata : IAsyncCommand
    {
        #region Constructors

        public CmdSaveMetadata(CacheData cacheData)
        {
            m_cacheData = cacheData;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            lock (m_cacheData)
            {
                // Save any metadata whose information has changed.  We check every minute for metadata
                // to save, and only save a few at a time, since there many be thousands of individual 
                // files to save.
                int saveCount = 0;
                foreach (var movie in m_cacheData.Movies)
                {
                    if (movie.MetadataChanged)
                    {
                        string path = Path.Combine(movie.Path, movie.MetadataFileName);
                        MovieSerializer<MovieMetadata>.Save(path, movie.Metadata);
                        movie.MetadataChanged = false;
                        saveCount++;
                        if (saveCount > 15)
                            break;
                    }
                }                   
            }
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;

        #endregion
    }
}
