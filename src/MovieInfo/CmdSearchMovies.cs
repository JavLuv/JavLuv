using Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieInfo
{
    public enum SortMoviesBy
    {
        Title,
        ID,
        Actress,
        Date_Newest,
        Date_Oldest,
        UserRating,
    }

    #region Comparers

    public class MovieTitleComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            return MovieUtils.MovieTitleCompare(left.Metadata.Title, right.Metadata.Title);
        }
    }

    public class MovieIDComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            return MovieUtils.MovieIDCompare(left.Metadata.UniqueID, right.Metadata.UniqueID);
        }
    }

    public class MovieActressComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            return MovieUtils.MovieActressCompare(left.Metadata.Actors, right.Metadata.Actors);
        }
    }

    public class MovieDateNewestComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            int cmpVal = right.Metadata.Premiered.CompareTo(left.Metadata.Premiered);
            if (cmpVal == 0)
                return left.Metadata.Title.CompareTo(right.Metadata.Title);
            return cmpVal;
        }
    }

    public class MovieDateOldestComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            int cmpVal = left.Metadata.Premiered.CompareTo(right.Metadata.Premiered);
            if (cmpVal == 0)
                return left.Metadata.Title.CompareTo(right.Metadata.Title);
            return cmpVal;
        }
    }

    public class MovieUserRatingComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            int cmpVal = right.Metadata.UserRating.CompareTo(left.Metadata.UserRating);
            if (cmpVal == 0)
                return left.Metadata.Title.CompareTo(right.Metadata.Title);
            return cmpVal;
        }
    }

    #endregion

    public class CmdSearchMovies : IAsyncCommand
    {
        #region Constructors

        public CmdSearchMovies(
            MovieCollection collection,
            CacheData cacheData, 
            string searchText, 
            string actressName,
            SortMoviesBy sortMoviesBy, 
            bool showUnratedOnly, 
            bool showSubtitlesOnly
            )
        {
            m_movieCollection = collection;
            m_cacheData = cacheData;
            m_searchText = searchText;
            m_actressName = actressName;
            m_sortMoviesBy = sortMoviesBy;
            m_showUnratedOnly = showUnratedOnly;
            m_showSubtitlesOnly = showSubtitlesOnly;
        }

        #endregion;

        #region Properties

        public List<MovieData> FilteredMovies { get { return m_filteredMovies; } }

        #endregion

        #region Public Functions

        public void Execute()
        {
            // Perform keyword-based search if required
            if (String.IsNullOrEmpty(m_searchText) == false || m_showSubtitlesOnly || m_showUnratedOnly || String.IsNullOrEmpty(m_actressName) == false)
            {
                Search();
            }
            else
            {
                // Populate filtered movie list with all movies
                lock (m_cacheData)
                {
                    foreach (var movie in m_cacheData.Movies)
                    {
                        if (m_showUnratedOnly && movie.Metadata.UserRating != 0)
                            continue;
                        m_filteredMovies.Add(movie);
                    }
                }
            }

            // Sort the filtered movies
            Sort();

            // Calculate average movie rating
            int totalRating = 0;
            int totalCount = 0;
            foreach (var movie in m_filteredMovies)
            {
                if (movie.Metadata.UserRating != 0)
                {
                    totalCount++;
                    totalRating += movie.Metadata.UserRating;
                }
            }
            if (String.IsNullOrEmpty(m_actressName) == false && totalCount > 0)
                m_movieCollection.AverageMovieRating = (int)Math.Ceiling((double)totalRating / (double)totalCount);
            else
                m_movieCollection.AverageMovieRating = 0;
        }

        #endregion

        #region Private Functions

        private void Search()
        {
            HashSet<MovieData> foundMovies = new HashSet<MovieData>();

            if (String.IsNullOrEmpty(m_searchText) == false)
            {
                var terms = MovieUtils.SearchSplit(m_searchText);   
                foreach (MovieData movie in m_cacheData.Movies)
                {
                    if (m_showUnratedOnly && movie.Metadata.UserRating != 0)
                        continue;
                    if (m_showSubtitlesOnly && movie.SubtitleFileNames.Count() == 0)
                        continue;
                    bool found = true;
                    foreach (string term in terms)
                    {
                        if (!SearchMovieForTerm(movie, term))
                        {
                            found = false;
                            continue;
                        }
                    }
                    if (found)
                        foundMovies.Add(movie);
                }
            }

            if (String.IsNullOrEmpty(m_actressName) == false)
            {
                foreach (MovieData movie in m_cacheData.Movies)
                {
                    if (SearchMovieForActress(movie, m_actressName))
                        m_filteredMovies.Add(movie);
                }
            }

            foreach (MovieData movie in foundMovies)
                m_filteredMovies.Add(movie);
        }

        private bool SearchMovieForTerm(MovieData movie, string term)
        {
            if (movie.Metadata.Title.ContainsCaseless(term))
                return true;
            if (movie.Metadata.UniqueID.Value.ContainsCaseless(term))
                return true;
            if (movie.Metadata.Genres.ContainsCaseless(term))
                return true;
            foreach (var actor in movie.Metadata.Actors)
            {
                if (actor.Name.ContainsCaseless(term))
                    return true;
                if (actor.Aliases.ContainsCaseless(term))
                    return true;
            }
            if (movie.Metadata.Studio.ContainsCaseless(term))
                return true;
            if (movie.Metadata.Label.ContainsCaseless(term))
                return true;
            if (movie.Metadata.Studio.ContainsCaseless(term))
                return true;
            if (movie.Metadata.Director.ContainsCaseless(term))
                return true;
            if (movie.Metadata.Plot.ContainsCaseless(term))
                return true;
            if (movie.Path.ContainsCaseless(term))
                return true;
            return false;
        }

        private bool SearchMovieForActress(MovieData movie, string actress)
        {
            foreach (var actor in movie.Metadata.Actors)
            {
                if (String.Equals(actress, actor.Name, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (Utilities.Equals(actress, actor.Aliases, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void Sort()
        {
            switch (m_sortMoviesBy)
            {
                case SortMoviesBy.Title:
                    m_filteredMovies.Sort(new MovieTitleComparer());
                    break;
                case SortMoviesBy.ID:
                    m_filteredMovies.Sort(new MovieIDComparer());
                    break;
                case SortMoviesBy.Actress:
                    m_filteredMovies.Sort(new MovieActressComparer());
                    break;
                case SortMoviesBy.Date_Newest:
                    m_filteredMovies.Sort(new MovieDateNewestComparer());
                    break;
                case SortMoviesBy.Date_Oldest:
                    m_filteredMovies.Sort(new MovieDateOldestComparer());
                    break;
                case SortMoviesBy.UserRating:
                    m_filteredMovies.Sort(new MovieUserRatingComparer());
                    break;
            };
        }

        #endregion

        #region Private Members

        private MovieCollection m_movieCollection;
        private CacheData m_cacheData;
        private List<MovieData> m_filteredMovies = new List<MovieData>();
        private string m_searchText = String.Empty;
        private SortMoviesBy m_sortMoviesBy = SortMoviesBy.Title;
        private bool m_showUnratedOnly;
        private bool m_showSubtitlesOnly;
        private string m_actressName;

        #endregion
    }
}
