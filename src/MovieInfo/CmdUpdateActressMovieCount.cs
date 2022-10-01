using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieInfo
{
    public class CmdUpdateActressMovieCount : IAsyncCommand
    {
        #region Constructors

        public CmdUpdateActressMovieCount(CacheData cacheData, ActressesDatabase actressesDatabase)
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
            ActressData actress = null;
            if (m_actressesDatabase.Actresses.TryGetValue(new ActressData(name), out actress))
                actress.MovieCount++;
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private ActressesDatabase m_actressesDatabase;

        #endregion
    }
}
