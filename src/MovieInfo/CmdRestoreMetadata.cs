using System.IO;

namespace MovieInfo
{
    public class CmdRestoreMetadata : IAsyncCommand
    {
        #region Constructors

        public CmdRestoreMetadata(ref CacheData cacheData, string fileName)
        {
            m_cacheData = cacheData;
            m_fileName = fileName;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            lock (m_cacheData)
            {
                // Check to see if user ratings backup file exists
                string userRatingFilename = Path.Combine(Path.GetDirectoryName(m_fileName), "Metadata.backup");
                if (File.Exists(userRatingFilename))
                {
                    // Load user ratings backup file
                    var userRatings = MovieSerializer<BackupData>.Load(userRatingFilename);

                    // Check all user ratings in backup file.  If any of them are different from cached data,
                    // then restore values from the backup set.
/*
                    MovieData movie = new MovieData();
                    movie.Metadata = new MovieMetadata();
                    foreach (var idRating in userRatings.Ratings)
                    {
                        movie.Metadata.UniqueID.Value = idRating.ID;
                        MovieData cachedMovie = null;
                        if (m_cacheData.Movies.TryGetValue(movie, out cachedMovie))
                        {
                            if (idRating.UserRating != cachedMovie.Metadata.UserRating)
                            {
                                cachedMovie.Metadata.UserRating = idRating.UserRating;
                                cachedMovie.MetadataChanged = true;
                            }
                        }
                    }
*/
                }      
            }
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private string m_fileName;

        #endregion
    }
}
