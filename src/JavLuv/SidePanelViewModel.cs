using MovieInfo;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SortMoviesByPair = JavLuv.ObservableStringValuePair<MovieInfo.SortMoviesBy>;
using SortMoviesByPairList = System.Collections.ObjectModel.ObservableCollection<JavLuv.ObservableStringValuePair<MovieInfo.SortMoviesBy>>;
using SortActressesByPair = JavLuv.ObservableStringValuePair<MovieInfo.SortActressesBy>;
using SortActressesByPairList = System.Collections.ObjectModel.ObservableCollection<JavLuv.ObservableStringValuePair<MovieInfo.SortActressesBy>>;
using Common;
using WebScraper;
using System;

namespace JavLuv
{
    public class SidePanelViewModel : ObservableObject
    {
        #region Constructors

        public SidePanelViewModel(MainWindowViewModel parent)
        {
            m_parent = parent;

            ShowSubtitlesOnly = Settings.Get().ShowSubtitlesOnly;

            TestScraper.m_testsFinished += OnScraperTestsFinished;

            Parent.Collection.SearchText = SearchText;
            Parent.Collection.ShowUnratedOnly = ShowUnratedOnly;
            Parent.Collection.ShowSubtitlesOnly = ShowSubtitlesOnly;
            Parent.Collection.ShowAllActresses = ShowAllActresses;

            m_sortMovieByList.Add(new SortMoviesByPair("Text.SortByTitle", SortMoviesBy.Title));
            m_sortMovieByList.Add(new SortMoviesByPair("Text.SortByID", SortMoviesBy.ID));
            m_sortMovieByList.Add(new SortMoviesByPair("Text.SortByActress", SortMoviesBy.Actress));
            m_sortMovieByList.Add(new SortMoviesByPair("Text.SortByDateNewest", SortMoviesBy.Date_Newest));
            m_sortMovieByList.Add(new SortMoviesByPair("Text.SortByDateOldest", SortMoviesBy.Date_Oldest));
            m_sortMovieByList.Add(new SortMoviesByPair("Text.SortByRandom", SortMoviesBy.Random));
            m_sortMovieByList.Add(new SortMoviesByPair("Text.SortByResolution", SortMoviesBy.Resolution));
            m_sortMovieByList.Add(new SortMoviesByPair("Text.SortByRecentlyAdded", SortMoviesBy.RecentlyAdded));
            m_sortMovieByList.Add(new SortMoviesByPair("Text.SortByUserRating", SortMoviesBy.UserRating));

            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByName", SortActressesBy.Name));
            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByAgeYoungest", SortActressesBy.Age_Youngest));
            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByAgeOldest", SortActressesBy.Age_Oldest));
            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByHeightShortest", SortActressesBy.Height_Shortest));
            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByHeightTallest", SortActressesBy.Height_Tallest));
            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByCupSmallest", SortActressesBy.Cup_Smallest));
            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByCupBiggest", SortActressesBy.Cup_Biggest));
            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByBirthday", SortActressesBy.Birthday));
            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByMovieCount", SortActressesBy.MovieCount));
            m_sortActressesByList.Add(new SortActressesByPair("Text.SortByUserRating", SortActressesBy.UserRating));

            foreach (var sortBy in m_sortMovieByList)
            {
                if (sortBy.Value == Settings.Get().SortMoviesBy)
                {
                    CurrentSortMovieBy = sortBy;
                    Parent.Collection.SortMoviesBy = sortBy.Value;
                    break;
                }
            }
            NotifyPropertyChanged("SortMovieByList");

            foreach (var sortBy in m_sortActressesByList)
            {
                if (sortBy.Value == Settings.Get().SortActressesBy)
                {
                    CurrentSortActressesBy = sortBy;
                    Parent.Collection.SortActressesBy = sortBy.Value;
                    break;
                }
            }
            NotifyPropertyChanged("SortActressesByList");

        }

        #endregion

        #region Public Functions

        public void NotifyAllProperty()
        {
            NotifyAllPropertiesChanged();
            foreach (var sortByPair in Parent.SidePanel.SortMovieByList)
                sortByPair.Notify();
        }

        #endregion

        #region Properties

        public MainWindowViewModel Parent { get { return m_parent; } }

        public bool IsCommandViewEnabled 
        { 
            get
            {
                return m_isCommandViewEnabled;
            }
            set
            {
                if (value != m_isCommandViewEnabled)
                {
                    m_isCommandViewEnabled = value;
                    NotifyPropertyChanged("IsCommandViewEnabled");
                }
            }
        }

        public bool IsSearchViewEnabled
        {
            get
            {
                return m_isSearchViewEnabled;
            }
            set
            {
                if (value != m_isSearchViewEnabled)
                {
                    m_isSearchViewEnabled = value;
                    NotifyPropertyChanged("IsSearchViewEnabled");
                }
            }
        }

        public bool SettingsIsEnabled
        {
            get
            {
                return m_settingsIsEnabled;
            }
            set
            {
                if (value != m_settingsIsEnabled)
                {
                    m_settingsIsEnabled = value;
                    NotifyPropertyChanged("SettingsIsEnabled");
                }
            }
        }

        public System.Windows.Visibility AdvancedVisibility
        {
            get
            {
                if (Settings.Get().ShowAdvancedOptions)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public string SearchText
        {
            get { return Settings.Get().SearchText; }
            set
            {
                if (value != Settings.Get().SearchText)
                {
                    Parent.Collection.SearchText = value;
                    Settings.Get().SearchText = value.Sanitize();
                    NotifyPropertyChanged("SearchText");
                }
            }
        }

        public bool ShowID
        {
            get { return Settings.Get().ShowID; }
            set
            {
                if (value != Settings.Get().ShowID)
                {
                    Settings.Get().ShowID = value;
                    Parent.Collection.ShowID = value;
                    NotifyPropertyChanged("ShowID");
                }
            }
        }

        public bool ShowUserRating
        {
            get { return Settings.Get().ShowUserRating; }
            set
            {
                if (value != Settings.Get().ShowUserRating)
                {
                    Settings.Get().ShowUserRating = value;
                    Parent.Collection.ShowUserRating = value;
                    NotifyPropertyChanged("ShowUserRating");
                }
            }
        }

        public bool ShowUnratedOnly
        {
            get { return Settings.Get().ShowUnratedOnly; }
            set
            {
                if (value != Settings.Get().ShowUnratedOnly)
                {
                    Settings.Get().ShowUnratedOnly = value;
                    Parent.Collection.ShowUnratedOnly = value;
                    NotifyPropertyChanged("ShowUnratedOnly");
                }
            }
        }

        public bool ShowSubtitlesOnly
        {
            get { return Settings.Get().ShowSubtitlesOnly; }
            set
            {
                if (value != Settings.Get().ShowSubtitlesOnly)
                {
                    Settings.Get().ShowSubtitlesOnly = value;
                    Parent.Collection.ShowSubtitlesOnly = value;
                    NotifyPropertyChanged("ShowSubtitlesOnly");
                }
            }
        }

        public bool ShowAllActresses
        {
            get { return Settings.Get().ShowAllActresses; }
            set
            {
                if (value != Settings.Get().ShowAllActresses)
                {
                    Settings.Get().ShowAllActresses = value;
                    Parent.Collection.ShowAllActresses = value;
                    NotifyPropertyChanged("ShowAllActresses");
                }
            }
        }

        public ObservableCollection<SortMoviesByPair> SortMovieByList
        {
            get { return m_sortMovieByList; }
        }

        public SortMoviesByPair CurrentSortMovieBy
        {
            get { return m_currentSortMovieBy; }
            set
            {
                if (value != m_currentSortMovieBy)
                {
                    m_currentSortMovieBy = value;
                    Settings.Get().SortMoviesBy = m_currentSortMovieBy.Value;
                    NotifyPropertyChanged("CurrentSortMovieBy");
                    Parent.Collection.SortMoviesBy = m_currentSortMovieBy.Value;
                }
            }
        }
        public ObservableCollection<SortActressesByPair> SortActressByList
        {
            get { return m_sortActressesByList; }
        }

        public SortActressesByPair CurrentSortActressesBy
        {
            get { return m_currentSortActressesBy; }
            set
            {
                if (value != m_currentSortActressesBy)
                {
                    m_currentSortActressesBy = value;
                    Settings.Get().SortActressesBy = m_currentSortActressesBy.Value;
                    NotifyPropertyChanged("CurrentSortActressesBy");
                    Parent.Collection.SortActressesBy = m_currentSortActressesBy.Value;
                }
            }
        }

        public Visibility MovieControlsVisibility
        {
            get { return m_movieControlsVisible; }
            set
            {
                if (value != m_movieControlsVisible)
                {
                    m_movieControlsVisible = value;
                    NotifyPropertyChanged("MovieControlsVisibility");
                }
            }
        }

        public Visibility ActressControlsVisibility
        {
            get { return m_actressControlsVisible; }
            set
            {
                if (value != m_actressControlsVisible)
                {
                    m_actressControlsVisible = value;
                    NotifyPropertyChanged("ActressControlsVisibility");
                }
            }
        }

        public Visibility DebugVisible
        {
            get
            {
                #if DEBUG
                return Visibility.Visible;
                #else
                return Visibility.Collapsed;
                #endif
            }
        }

        #endregion

        #region Commands

        #region Settings Command

        private void SettingsExecute()
        {
            Parent.OpenSettings();
        }

        private bool CanSettingsExecuteExecute()
        {
            return Parent.IsReadOnlyMode == false;
        }

        public ICommand SettingsCommand { get { return new RelayCommand(SettingsExecute, CanSettingsExecuteExecute); } }

        #endregion

        #region Scan Movies Command

        private void ScanMoviesExecute()
        {
            if (Parent.IsScanning)
            {
                var msgRes = System.Windows.Forms.MessageBox.Show(
                    TextManager.GetString("Text.JavLuvScanningFiles"),
                    TextManager.GetString("Text.CancelScan"), 
                    System.Windows.Forms.MessageBoxButtons.YesNo);
                if (msgRes == System.Windows.Forms.DialogResult.Yes)
                {
                    Parent.CancelScan();
                }
                else
                {
                    return;
                }
            }

            var scanWindow = new ScanView(this);
            scanWindow.Owner = App.Current.MainWindow;
            scanWindow.ShowDialog();
        }

        private bool CanScanMoviesExecute()
        {
            return Parent.IsReadOnlyMode == false;
        }

        public ICommand ScanMoviesCommand { get { return new RelayCommand(ScanMoviesExecute, CanScanMoviesExecute); } }

        #endregion

        #region Concatenate Movies Command

        private void ConcatenateMoviesExecute()
        {
            ConcatView concatView = new ConcatView();
            concatView.Owner = App.Current.MainWindow;           
            concatView.ShowDialog();
        }

        private bool CanConcatenateMoviesExecute()
        {
            return true;
        }

        public ICommand ConcatenateMoviesCommand { get { return new RelayCommand(ConcatenateMoviesExecute, CanConcatenateMoviesExecute); } }

        #endregion

        #region Organize Subtitles Command

        private void OrganizeSubtitlesExecute()
        {
            SubtitleOrganizeView subtitleOrganizeView = new SubtitleOrganizeView(this);
            subtitleOrganizeView.Owner = App.Current.MainWindow;
            subtitleOrganizeView.ShowDialog();
        }

        private bool CanOrganizeSubtitlesExecute()
        {
            return true;
        }

        public ICommand OrganizeSubtitlesCommand { get { return new RelayCommand(OrganizeSubtitlesExecute, CanOrganizeSubtitlesExecute); } }

        #endregion

        #region Test Scrapers Command

        private void TestScrapersExecute()
        {
            if (Parent.IsScanning)
                return;
            var mainWindow = Application.Current.MainWindow as MainWindow;
            //mainWindow.webViewControl.Visibility = Visibility.Visible;
            TestScraper.RunTests(Application.Current.Dispatcher, mainWindow.webView);
        }

        private bool CanTestScrapersEExecute()
        {
            return Parent.IsScanning == false;
        }

        public ICommand TestScrapersECommand { get { return new RelayCommand(TestScrapersExecute, CanTestScrapersEExecute); } }

        #endregion

        #endregion

        #region Event Handlers

        private void OnScraperTestsFinished(object sender, EventArgs e)
        {
            string message = "Finished running all scraper tests";
            if (TestScraper.m_exception != null)
                message = TestScraper.m_exception.ToString();
            System.Windows.Forms.MessageBox.Show(
                message,
                "Test Scrapers",
                System.Windows.Forms.MessageBoxButtons.OK);
        }

        #endregion

        #region Private Members


        private MainWindowViewModel m_parent;
        private bool m_isCommandViewEnabled = true;
        private bool m_isSearchViewEnabled = true;
        private bool m_settingsIsEnabled = true;
        private SortMoviesByPairList m_sortMovieByList = new SortMoviesByPairList();
        private SortMoviesByPair m_currentSortMovieBy;
        private SortActressesByPairList m_sortActressesByList = new SortActressesByPairList();
        private SortActressesByPair m_currentSortActressesBy;
        private Visibility m_movieControlsVisible = Visibility.Visible;
        private Visibility m_actressControlsVisible = Visibility.Visible;

        #endregion
    }
}
