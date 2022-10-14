using Common;
using System.Collections.Generic;

namespace MovieInfo
{
    public class CmdFilter : IAsyncCommand
    {
        #region Constructors

        public CmdFilter(List<MovieData> movies, List<FilterPair> studioFilter, List<FilterPair> labelFilter, List<FilterPair> directorFilter, List<FilterPair> genreFilter, List<FilterPair> actorFilter)
        {
            m_movies = movies;
            m_studioFilter = studioFilter;
            m_labelFilter = labelFilter;
            m_directorFilter = directorFilter;
            m_genreFilter = genreFilter;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            foreach (var movie in m_movies)
            {
                bool changed = MovieUtils.FilterMetadata(movie.Metadata, m_studioFilter, m_labelFilter, m_directorFilter, m_genreFilter);
                if (changed == true)
                    movie.MetadataChanged = true;
            }
        }

        #endregion

        #region Private Members

        private List<MovieData> m_movies;
        private List<FilterPair> m_studioFilter;
        private List<FilterPair> m_labelFilter;
        private List<FilterPair> m_directorFilter;
        private List<FilterPair> m_genreFilter;

        #endregion
    }
}
