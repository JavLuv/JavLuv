namespace MovieInfo
{
    public class CmdUpdateActressMovieCount : IAsyncCommand
    {
        #region Constructors

        public CmdUpdateActressMovieCount(MovieCollection collection, CacheData cacheData, ActressesDatabase actressesDatabase)
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
                foreach (var actress in m_actressesDatabase.Actresses)
                    actress.MovieCount = 0;

                lock (m_cacheData)
                {
                    foreach (MovieData movie in m_cacheData.Movies)
                        UpdateActressesCount(movie);
                }
            }
        }

        #endregion

        #region Private Functions

        private void UpdateActressesCount(MovieData movie)
        {
            foreach (var actor in movie.Metadata.Actors)
                UpdateActressCount(actor.Name);
        }

        private void UpdateActressCount(string name)
        {
            ActressData actress = m_collection.FindActress(name);
            if (m_actressesDatabase != null)
                actress.MovieCount++;
        }

        #endregion

        #region Private Members

        private MovieCollection m_collection;
        private CacheData m_cacheData;
        private ActressesDatabase m_actressesDatabase;

        #endregion
    }
}
