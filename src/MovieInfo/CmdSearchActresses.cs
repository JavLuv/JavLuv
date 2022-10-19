using Common;
using System;
using System.Collections.Generic;

namespace MovieInfo
{

    public enum SortActressesBy
    {
        Name,
        Age_Youngest,
        Age_Oldest,
        Birthday,
        MovieCount,
        UserRating,
    }

    #region Comparers

    public class ActressNameComparer : IComparer<ActressData>
    {
        public int Compare(ActressData left, ActressData right)
        {
            return String.Compare(left.Name, right.Name);
        }
    }

    public class ActressAgeYoungestComparer : IComparer<ActressData>
    {
        public int Compare(ActressData left, ActressData right)
        {
            if (left.DobYear == 0 && right.DobYear == 0)
                return String.Compare(right.Name, left.Name);
            if (left.DobYear == 0)
                return 1;
            if (right.DobYear == 0)
                return -1;
            int l = MovieUtils.GetAgeFromDateOfBirth(left.DobYear, left.DobMonth, left.DobDay);
            int r = MovieUtils.GetAgeFromDateOfBirth(right.DobYear, right.DobMonth, right.DobDay);
            if (l != r)
                return (l < r) ? -1 : 1;
            return String.Compare(left.Name, right.Name);
        }
    }

    public class ActressAgeOldestComparer : IComparer<ActressData>
    {
        public int Compare(ActressData left, ActressData right)
        {
            if (left.DobYear == 0 && right.DobYear == 0)
                return String.Compare(right.Name, left.Name);
            if (left.DobYear == 0)
                return 1;
            if (right.DobYear == 0)
                return -1;
            int l = MovieUtils.GetAgeFromDateOfBirth(left.DobYear, left.DobMonth, left.DobDay);
            int r = MovieUtils.GetAgeFromDateOfBirth(right.DobYear, right.DobMonth, right.DobDay);
            if (l != r)
                return (l < r) ? 1 : -1;
            return String.Compare(left.Name, right.Name);
        }
    }

    public class ActressBirthdayComparer : IComparer<ActressData>
    {
        public int Compare(ActressData left, ActressData right)
        {
            if (left.DobDay == 0 && right.DobDay == 0)
                return String.Compare(left.Name, right.Name);
            if (left.DobDay == 0 || left.DobMonth == 0)
                return 1;
            if (right.DobDay == 0 || right.DobMonth == 0)
                return -1;
            if (left.DobMonth != right.DobMonth)
                return (left.DobMonth < right.DobMonth) ? -1 : 1;
            if (left.DobDay != right.DobDay)
                return (left.DobDay < right.DobDay) ? -1 : 1;
            return String.Compare(left.Name, right.Name);
        }
    }

    public class ActressMovieCountComparer : IComparer<ActressData>
    {
        public int Compare(ActressData left, ActressData right)
        {
            int l = left.MovieCount;
            int r = right.MovieCount;
            if (l != r)
                return (l < r) ? 1 : -1;
            return String.Compare(left.Name, right.Name);
        }
    }

    public class ActressUserRatingComparer : IComparer<ActressData>
    {
        public int Compare(ActressData left, ActressData right)
        {
            int l = left.UserRating;
            int r = right.UserRating;
            if (l != r)
                return (l < r) ? 1 : -1;
            return String.Compare(left.Name, right.Name);
        }
    }

    #endregion

    public class CmdSearchActresses : IAsyncCommand
    {
        #region Constructors

        public CmdSearchActresses(ActressesDatabase actressesData, string searchText, SortActressesBy sortActressesBy, bool showAllActresses)
        {
            m_actressesData = actressesData;
            m_searchText = searchText;
            m_sortActressesBy = sortActressesBy;
            m_showAllActresses = showAllActresses;
        }

        #endregion;

        #region Properties

        public List<ActressData> FilteredActresses { get { return m_filteredActresses; } }

        #endregion

        #region Public Functions

        public void Execute()
        {
            // Perform keyword-based search if required
            if (String.IsNullOrEmpty(m_searchText) == false || m_showAllActresses == false)
            {
                Search();
            }
            else
            {
                // Populate filtered movie list with all movies
                lock (m_actressesData)
                {
                    foreach (var actress in m_actressesData.Actresses)
                        m_filteredActresses.Add(actress);
                }
            }

            // Sort the filtered movies
            Sort();
        }

        #endregion

        #region Private Functions

        private void Search()
        {
            var terms = MovieUtils.SearchSplit(m_searchText);
            HashSet<ActressData> foundActresses = new HashSet<ActressData>();
            foreach (ActressData actress in m_actressesData.Actresses)
            {
                if (m_showAllActresses == false)
                {
                    if (MovieUtils.IsActressWorthShowing(actress) == false)
                        continue;
                    if (m_sortActressesBy == SortActressesBy.Age_Youngest || 
                        m_sortActressesBy == SortActressesBy.Age_Oldest)
                    {
                        if (actress.DobYear == 0)
                            continue;
                    }
                    else if (m_sortActressesBy == SortActressesBy.Age_Youngest ||
                        m_sortActressesBy == SortActressesBy.Age_Oldest ||
                        m_sortActressesBy == SortActressesBy.Birthday)
                    {
                        if (actress.DobYear == 0 || actress.DobMonth == 0 || actress.DobDay == 0)
                            continue;
                    }
                }
                bool found = true;
                foreach (string term in terms)
                {

                    if (!SearchActresses(actress, term))
                    {
                        found = false;
                        continue;
                    }
                }
                if (found)
                    foundActresses.Add(actress);
            }

            foreach (ActressData actress in foundActresses)
                m_filteredActresses.Add(actress);
        }

        private bool SearchActresses(ActressData actress, string term)
        {
            if (actress.Name.ContainsCaseless(term))
                return true;
            if (actress.JapaneseName.ContainsCaseless(term))
                return true;
            foreach (var altName in actress.AltNames)
            {
                if (altName.ContainsCaseless(term))
                    return true;
            }
            if (actress.Notes.ContainsCaseless(term))
                return true;
            return false;
        }

        private void Sort()
        {
            switch (m_sortActressesBy)
            {
                case SortActressesBy.Name:
                    m_filteredActresses.Sort(new ActressNameComparer());
                    break;
                case SortActressesBy.Age_Youngest:
                    m_filteredActresses.Sort(new ActressAgeYoungestComparer());
                    break;
                case SortActressesBy.Age_Oldest:
                    m_filteredActresses.Sort(new ActressAgeOldestComparer());
                    break;
                case SortActressesBy.Birthday:
                    m_filteredActresses.Sort(new ActressBirthdayComparer());
                    break;
                case SortActressesBy.MovieCount:
                    m_filteredActresses.Sort(new ActressMovieCountComparer());
                    break;
                case SortActressesBy.UserRating:
                    m_filteredActresses.Sort(new ActressUserRatingComparer());
                    break;
            };
        }

        #endregion

        #region Private Members

        private ActressesDatabase m_actressesData;
        private List<ActressData> m_filteredActresses = new List<ActressData>();
        private string m_searchText = String.Empty;
        private SortActressesBy m_sortActressesBy = SortActressesBy.Name;
        private bool m_showAllActresses;

        #endregion
    }
}
