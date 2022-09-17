using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieInfo
{
    public class CmdMarkSharedFolders : IAsyncCommand
    {
        #region Constructors

        public CmdMarkSharedFolders(CacheData cacheData)
        {
            m_cacheData = cacheData;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            lock (m_cacheData)
            {
                var paths = new HashSet<string>();
                var sharedPaths = new HashSet<string>();
                foreach (var movie in m_cacheData.Movies)
                {
                    if (paths.Contains(movie.Path) == false)
                        paths.Add(movie.Path);
                    else
                        sharedPaths.Add(movie.Path);
                }
                foreach (var movie in m_cacheData.Movies)
                    movie.SharedPath = sharedPaths.Contains(movie.Path);
            }
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;

        #endregion
    }
}
