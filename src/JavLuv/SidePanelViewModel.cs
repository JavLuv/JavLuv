using MovieInfo;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace JavLuv
{
    // TODO: Make a generic ObservablePair class or something,
    // instead of making all those one-off classes
    public class SortMoviesByPair : ObservableObject
    {
        #region Constructor

        public SortMoviesByPair(SortMoviesBy sortBy, string key)
        {
            Value = sortBy;
            m_key = key;
        }

        #endregion

        #region Properties

        public SortMoviesBy Value { get; private set; }
        public string Name 
        { 
            get
            {
                return TextManager.GetString(m_key);
            }
        }

        #endregion

        #region Public Functions

        public void Refresh()
        {
            NotifyAllPropertiesChanged();
        }

        #endregion

        #region Private Members

        private string m_key;

        #endregion
    }

    public class SortActressesByPair : ObservableObject
    {
        #region Constructor

        public SortActressesByPair(SortActressesBy sortBy, string key)
        {
            Value = sortBy;
            m_key = key;
        }

        #endregion

        #region Properties

        public SortActressesBy Value { get; private set; }
        public string Name
        {
            get
            {
                return TextManager.GetString(m_key);
            }
        }

        #endregion

        #region Public Functions

        public void Refresh()
        {
            NotifyAllPropertiesChanged();
        }

        #endregion

        #region Private Members

        private string m_key;

        #endregion
    }

    public class SidePanelViewModel : ObservableObject
    {
        #region Constructors

        public SidePanelViewModel(MainWindowViewModel parent)
        {
            m_parent = parent;

            ShowSubtitlesOnly = Settings.Get().ShowSubtitlesOnly;

            Parent.Collection.SearchText = SearchText;
            Parent.Collection.ShowUnratedOnly = ShowUnratedOnly;
            Parent.Collection.ShowSubtitlesOnly = ShowSubtitlesOnly;
            Parent.Collection.ShowUnknownActresses = ShowUnknownActresses;

            m_sortMovieByList.Add(new SortMoviesByPair(SortMoviesBy.Title, "Text.SortByTitle"));
            m_sortMovieByList.Add(new SortMoviesByPair(SortMoviesBy.ID, "Text.SortByID"));
            m_sortMovieByList.Add(new SortMoviesByPair(SortMoviesBy.Actress, "Text.SortByActress"));
            m_sortMovieByList.Add(new SortMoviesByPair(SortMoviesBy.Date_Newest, "Text.SortByDateNewest"));
            m_sortMovieByList.Add(new SortMoviesByPair(SortMoviesBy.Date_Oldest, "Text.SortByDateOldest"));
            m_sortMovieByList.Add(new SortMoviesByPair(SortMoviesBy.UserRating, "Text.SortByUserRating"));

            m_sortActressesByList.Add(new SortActressesByPair(SortActressesBy.Name, "Text.SortByName"));
            m_sortActressesByList.Add(new SortActressesByPair(SortActressesBy.Age_Youngest, "Text.SortByAgeYoungest"));
            m_sortActressesByList.Add(new SortActressesByPair(SortActressesBy.Age_Oldest, "Text.SortByAgeOldest"));

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

            OnChangeTabs();
        }

        #endregion

        #region Public Functions

        public void NotifyAllProperty()
        {
            NotifyAllPropertiesChanged();
            foreach (var sortByPair in Parent.SidePanel.SortMovieByList)
                sortByPair.Refresh();
        }

        public void OnChangeTabs()
        {
            if (Settings.Get().SelectedTabIndex == 0)
            {
                // Movie browser is shown
                MovieControlsVisibility = Visibility.Visible;
                ActressControlsVisibility = Visibility.Collapsed;
            }
            else
            {
                // Actress browser is shown
                MovieControlsVisibility = Visibility.Collapsed;
                ActressControlsVisibility = Visibility.Visible;
            }
        }

        #endregion

        #region Properties

        public MainWindowViewModel Parent { get { return m_parent; } }

        public bool IsEnabled 
        { 
            get
            {
                return m_isEnabled;
            }
            set
            {
                if (value != m_isEnabled)
                {
                    m_isEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
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
                    Settings.Get().SearchText = value;
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

        public bool ShowUnknownActresses
        {
            get { return Settings.Get().ShowUnknownActresses; }
            set
            {
                if (value != Settings.Get().ShowUnknownActresses)
                {
                    Settings.Get().ShowUnknownActresses = value;
                    Parent.Collection.ShowUnknownActresses = value;
                    NotifyPropertyChanged("ShowUnknownActresses");
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

        #endregion

        #region Commands

        #region Settings Command

        private void SettingsExecute()
        {
            Parent.OpenSettings();
        }

        private bool CanSettingsExecuteExecute()
        {
            return true;
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
            return true;
        }

        public ICommand ScanMoviesCommand { get { return new RelayCommand(ScanMoviesExecute, CanScanMoviesExecute); } }

        #endregion

        #region Delete Cache Command

        private void DeleteCacheExecute()
        {
            var msgRes = System.Windows.Forms.MessageBox.Show(
                TextManager.GetString("Text.JavLuvDeleteCache"),
                TextManager.GetString("Text.DeleteLocalCache"),
                System.Windows.Forms.MessageBoxButtons.YesNo);
            if (msgRes == System.Windows.Forms.DialogResult.Yes)
            {
                Parent.Collection.DeleteCache();
                ImageCache.Get().DeleteAll();
            }    
        }

        private bool CanDeleteCacheExecute()
        {
            return true;
        }

        public ICommand DeleteCacheCommand { get { return new RelayCommand(DeleteCacheExecute, CanDeleteCacheExecute); } }

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

        #endregion

        #region Private Members

        private MainWindowViewModel m_parent;
        private bool m_isEnabled = true;
        private bool m_settingsIsEnabled = true;
        private ObservableCollection<SortMoviesByPair> m_sortMovieByList = new ObservableCollection<SortMoviesByPair>();
        private SortMoviesByPair m_currentSortMovieBy;
        private ObservableCollection<SortActressesByPair> m_sortActressesByList = new ObservableCollection<SortActressesByPair>();
        private SortActressesByPair m_currentSortActressesBy;
        private Visibility m_movieControlsVisible = Visibility.Visible;
        private Visibility m_actressControlsVisible = Visibility.Visible;

        #endregion
    }
}
