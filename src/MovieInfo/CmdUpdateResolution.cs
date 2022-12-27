using System;
using System.Collections.Generic;
using System.IO;

namespace MovieInfo
{
    public class CmdUpdateResolution : IAsyncCommand
    {
        #region Constructor

        public CmdUpdateResolution(CacheData cacheData)
        {
            m_cacheData = cacheData;
            m_startTime = DateTime.Now;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            // Ensure we don't queue these tasks in case of heavy disk usage
            if (DateTime.Now - m_startTime > new TimeSpan(0, 0, 2))
                return;

            List<MovieData> movies = new List<MovieData>();
            lock (m_cacheData)
            {
                foreach (var movie in m_cacheData.Movies)
                {
                    if (MovieUtils.HasMovieRevolution(movie.Metadata) == false && movie.MovieFileNames.Count > 0)
                    {
                        movies.Add(movie);
                    }
                }
            }

            // This may be a long-running operation, so only take the cache lock
            // individually, rather than for the entire operation.
            int count = 0;
            foreach (var movie in movies)
            {
                lock (m_cacheData)
                {
                    int width, height;
                    if (String.IsNullOrEmpty(movie.MovieResolution))
                    {
                        // If we must extract the resolution from the filename, break from the loop and continue later.
                        string resolution = MovieUtils.GetMovieResolution(Path.Combine(movie.Path, movie.MovieFileNames[0]));
                        MovieUtils.ParseMovieResolution(resolution, out width, out height);
                        MovieUtils.SetMovieResolution(movie, width, height);
                        count++;
                        if (count == 5)
                            break;
                    }
                    else
                    {
                        MovieUtils.ParseMovieResolution(movie.MovieResolution, out width, out height);
                        MovieUtils.SetMovieResolution(movie, width, height);
                    }               
                }
            }
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private DateTime m_startTime;

        #endregion
    }
}
