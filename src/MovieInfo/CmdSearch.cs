using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MovieInfo
{
    public enum SortBy
    {
        Title,
        ID,
        Actress,
        Date_Newest,
        Date_Oldest,
        UserRating,
    }

    #region Comparers

    public class TitleComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            return MovieUtils.TitleCompare(left.Metadata.Title, right.Metadata.Title);
        }
    }

    public class IDComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            return MovieUtils.MovieIDCompare(left.Metadata.UniqueID, right.Metadata.UniqueID);
        }
    }

    public class ActressComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            return MovieUtils.ActressCompare(left.Metadata.Actors, right.Metadata.Actors);
        }
    }

    public class DateNewestComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            int cmpVal = right.Metadata.Premiered.CompareTo(left.Metadata.Premiered);
            if (cmpVal == 0)
                return left.Metadata.Title.CompareTo(right.Metadata.Title);
            return cmpVal;
        }
    }

    public class DateOldestComparer : IComparer<MovieData>
    {
        public int Compare(MovieData left, MovieData right)
        {
            int cmpVal = left.Metadata.Premiered.CompareTo(right.Metadata.Premiered);
            if (cmpVal == 0)
                return left.Metadata.Title.CompareTo(right.Metadata.Title);
            return cmpVal;
        }
    }

    public class UserRatingComparer : IComparer<MovieData>
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

    public class CmdSearch : IAsyncCommand
    {
        #region Constructors

        public CmdSearch(CacheData cacheData, string searchText, SortBy sortBy, bool showUnratedOnly, bool showSubtitlesOnly)
        {
            m_cacheData = cacheData;
            m_searchText = searchText;
            m_sortBy = sortBy;
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
            if (String.IsNullOrEmpty(m_searchText) == false || m_showSubtitlesOnly || m_showUnratedOnly)
            {
                Search();
            }
            else
            {
                // Populate filtered movie list with all movies
                lock(m_cacheData)
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
        }

        #endregion

        #region Private Functions

        private List<string> Split(string stringToSplit, params char[] delimiters)
        {
            List<string> results = new List<string>();

            bool inQuote = false;
            StringBuilder currentToken = new StringBuilder();
            for (int index = 0; index < stringToSplit.Length; ++index)
            {
                char currentCharacter = stringToSplit[index];
                if (currentCharacter == '"')
                {
                    // When we see a ", we need to decide whether we are
                    // at the start or send of a quoted section...
                    inQuote = !inQuote;
                }
                else if (delimiters.Contains(currentCharacter) && inQuote == false)
                {
                    // We've come to the end of a token, so we find the token,
                    // trim it and add it to the collection of results...
                    string result = currentToken.ToString().Trim();
                    if (result != "") results.Add(result);

                    // We start a new token...
                    currentToken = new StringBuilder();
                }
                else
                {
                    // We've got a 'normal' character, so we add it to
                    // the curent token...
                    currentToken.Append(currentCharacter);
                }
            }

            // We've come to the end of the string, so we add the last token...
            string lastResult = currentToken.ToString().Trim();
            if (lastResult != "") 
                results.Add(lastResult);

            return results;
        }

        private void Search()
        {
            var terms = Split(m_searchText, ' ');   
            HashSet<MovieData> foundMovies = new HashSet<MovieData>();
            foreach (MovieData movie in m_cacheData.Movies)
            {
                if (m_showUnratedOnly && movie.Metadata.UserRating != 0)
                    continue;
                if (m_showSubtitlesOnly && movie.SubtitleFileNames.Count() == 0)
                    continue;
                bool found = true;
                foreach (string term in terms)
                {
                    if (!SearchMovie(movie, term))
                    {
                        found = false;
                        continue;
                    }
                }
                if (found)
                    foundMovies.Add(movie);
            }

            foreach (MovieData movie in foundMovies)
                m_filteredMovies.Add(movie);
        }

        private bool SearchMovie(MovieData movie, string term)
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

        private void Sort()
        {
            switch (m_sortBy)
            {
                case SortBy.Title:
                    m_filteredMovies.Sort(new TitleComparer());
                    break;
                case SortBy.ID:
                    m_filteredMovies.Sort(new IDComparer());
                    break;
                case SortBy.Actress:
                    m_filteredMovies.Sort(new ActressComparer());
                    break;
                case SortBy.Date_Newest:
                    m_filteredMovies.Sort(new DateNewestComparer());
                    break;
                case SortBy.Date_Oldest:
                    m_filteredMovies.Sort(new DateOldestComparer());
                    break;
                case SortBy.UserRating:
                    m_filteredMovies.Sort(new UserRatingComparer());
                    break;
            };
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private List<MovieData> m_filteredMovies = new List<MovieData>();
        private string m_searchText = String.Empty;
        private SortBy m_sortBy = SortBy.Title;
        private bool m_showUnratedOnly;
        private bool m_showSubtitlesOnly;

        #endregion
    }
}
