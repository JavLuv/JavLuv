using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieInfo
{


    public enum SortActressesBy
    {
        Name,
        Age_Youngest,
        Age_Oldest,
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
            return DateTime.Compare(right.DateOfBirth, left.DateOfBirth);
        }
    }

    public class ActressAgeOldestComparer : IComparer<ActressData>
    {
        public int Compare(ActressData left, ActressData right)
        {
            return DateTime.Compare(left.DateOfBirth, right.DateOfBirth);
        }
    }

    #endregion

    public class CmdSearchActresses : IAsyncCommand
    {
        #region Constructors

        public CmdSearchActresses(ActressesDatabase actressesData, string searchText, SortActressesBy sortActressesBy, bool showUnknownActresses)
        {
            m_actressesData = actressesData;
            m_searchText = searchText;
            m_sortActressesBy = sortActressesBy;
            m_showUnknownActresses = showUnknownActresses;
        }

        #endregion;

        #region Properties

        public List<ActressData> FilteredActresses { get { return m_filteredActresses; } }

        #endregion

        #region Public Functions

        public void Execute()
        {
            // Perform keyword-based search if required
            if (String.IsNullOrEmpty(m_searchText) == false || m_showUnknownActresses == false)
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
                if (m_showUnknownActresses == false)
                {
                    if (MovieUtils.IsActressUnknonwn(actress))
                        continue;
                    if (m_sortActressesBy == SortActressesBy.Age_Youngest || m_sortActressesBy == SortActressesBy.Age_Oldest)
                    {
                        if (actress.DateOfBirth == new DateTime())
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
            };
        }

        #endregion

        #region Private Members

        private ActressesDatabase m_actressesData;
        private List<ActressData> m_filteredActresses = new List<ActressData>();
        private string m_searchText = String.Empty;
        private SortActressesBy m_sortActressesBy = SortActressesBy.Name;
        private bool m_showUnknownActresses;
        #endregion
    }
}
