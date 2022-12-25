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
        Resolution,
        RecentlyAdded,
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
            int cmpVal = Utilities.DataTimeCompare(right.Metadata.Premiered, (left.Metadata.Premiered));
            if (cmpVal == 0)
                return left.Metadata.Title.CompareTo(right.Metadata.Title);
            return cmpVal;
        }
    }

    public class MovieDateOldestComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            int cmpVal = Utilities.DataTimeCompare(left.Metadata.Premiered, (right.Metadata.Premiered));
            if (cmpVal == 0)
                return left.Metadata.Title.CompareTo(right.Metadata.Title);
            return cmpVal;
        }
    }

    public class MovieResolutionComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            int cmpVal = right.Metadata.FileInfo.StreamDetails.Video.Height.CompareTo(left.Metadata.FileInfo.StreamDetails.Video.Height);
            if (cmpVal == 0)
            {
                cmpVal = right.Metadata.FileInfo.StreamDetails.Video.Width.CompareTo(left.Metadata.FileInfo.StreamDetails.Video.Width);
                if (cmpVal == 0)
                    return left.Metadata.Title.CompareTo(right.Metadata.Title);                
            }
            return cmpVal;
        }
    }

    public class MovieRecentlyAddedComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            int cmpVal = Utilities.DataTimeCompare(right.Metadata.DateAdded, (left.Metadata.DateAdded));
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
            ActressData searchActress,
            SortMoviesBy sortMoviesBy, 
            bool showUnratedOnly, 
            bool showSubtitlesOnly
            )
        {
            m_movieCollection = collection;
            m_cacheData = cacheData;
            m_searchText = searchText;
            m_searchActress = searchActress;
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
            // First start with actress name if available, or the full list of movies otherwise
            lock (m_cacheData)
            {
                if (m_searchActress != null)
                {
                    foreach (MovieData movie in m_cacheData.Movies)
                    {
                        if (SearchMovieForActress(movie, m_searchActress))
                            m_availableMovies.Add(movie);
                    }
                }
                else
                {
                    foreach (MovieData movie in m_cacheData.Movies)
                        m_availableMovies.Add(movie);
                }
            }

            // Perform keyword-based search if required
            if (String.IsNullOrEmpty(m_searchText) == false || m_showSubtitlesOnly || m_showUnratedOnly)
            {
                Search();
            }
            else
            {
                // Populate filtered movie list with all movies
                foreach (var movie in m_availableMovies)
                {
                    if (m_showUnratedOnly && movie.Metadata.UserRating != 0)
                        continue;
                    m_filteredMovies.Add(movie);
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
            if (m_searchActress != null && totalCount > 0)
                m_movieCollection.AverageMovieRating = (int)Math.Ceiling((double)totalRating / (double)totalCount);
            else
                m_movieCollection.AverageMovieRating = 0;
        }

        #endregion

        #region Private Functions

        private void Search()
        {
            HashSet<MovieData> foundMovies = new HashSet<MovieData>();

            var termsList = MovieUtils.SearchSplit(m_searchText);   
            foreach (MovieData movie in m_availableMovies)
            {
                if (m_showUnratedOnly && movie.Metadata.UserRating != 0)
                    continue;
                if (m_showSubtitlesOnly && movie.SubtitleFileNames.Count() == 0 && MovieUtils.IsHardSubtitled(movie) == false)
                    continue;

                foreach (var terms in termsList)
                {
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

            foreach (MovieData movie in foundMovies)
                m_filteredMovies.Add(movie);
        }

        private bool SearchMovieForTerm(MovieData movie, string term)
        {
            if (term == "-")
                return true;
            bool retVal = term.StartsWith("-") == false;
            if (retVal == false)
                term = term.Substring(1);
            if (movie.Metadata.Title.ContainsCaseless(term))
                return retVal;
            if (movie.Metadata.OriginalTitle.ContainsCaseless(term))
                return retVal;
            if (movie.Metadata.UniqueID.Value.ContainsCaseless(term))
                return retVal;
            if (movie.Metadata.Genres.ContainsCaseless(term))
                return retVal;
            foreach (var actor in movie.Metadata.Actors)
            {
                if (actor.Name.ContainsCaseless(term))
                    return retVal;
                if (actor.Aliases.ContainsCaseless(term))
                    return retVal;
            }
            if (movie.Metadata.Studio.ContainsCaseless(term))
                return retVal;
            if (movie.Metadata.Label.ContainsCaseless(term))
                return retVal;
            if (movie.Metadata.Studio.ContainsCaseless(term))
                return retVal;
            if (movie.Metadata.Director.ContainsCaseless(term))
                return retVal;
            if (movie.Metadata.Series.ContainsCaseless(term))
                return retVal;
            if (movie.Metadata.Plot.ContainsCaseless(term))
                return retVal;
            if (movie.Path.ContainsCaseless(term))
                return retVal;
            return !retVal;
        }

        private bool SearchMovieForActress(MovieData movie, ActressData actress)
        {
            foreach (var actor in movie.Metadata.Actors)
            {
                if (MovieUtils.ActressMatchesActor(actress, actor))
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
                case SortMoviesBy.Resolution:
                    m_filteredMovies.Sort(new MovieResolutionComparer());
                    break;
                case SortMoviesBy.RecentlyAdded:
                    m_filteredMovies.Sort(new MovieRecentlyAddedComparer());
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
        private List<MovieData> m_availableMovies = new List<MovieData>();
        private List<MovieData> m_filteredMovies = new List<MovieData>();
        private string m_searchText = String.Empty;
        private SortMoviesBy m_sortMoviesBy = SortMoviesBy.Title;
        private bool m_showUnratedOnly;
        private bool m_showSubtitlesOnly;
        private ActressData m_searchActress;

        #endregion
    }
}
