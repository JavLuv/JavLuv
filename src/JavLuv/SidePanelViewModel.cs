using Common;
using MovieInfo;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JavLuv
{
    public class SortMovieByPair : ObservableObject
    {
        #region Constructor

        public SortMovieByPair(SortMoviesBy sortBy, string key)
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

            m_sortMovieByList.Add(new SortMovieByPair(SortMoviesBy.Title, "Text.SortByTitle"));
            m_sortMovieByList.Add(new SortMovieByPair(SortMoviesBy.ID, "Text.SortByID"));
            m_sortMovieByList.Add(new SortMovieByPair(SortMoviesBy.Actress, "Text.SortByActress"));
            m_sortMovieByList.Add(new SortMovieByPair(SortMoviesBy.Date_Newest, "Text.SortByDateNewest"));
            m_sortMovieByList.Add(new SortMovieByPair(SortMoviesBy.Date_Oldest, "Text.SortByDateOldest"));
            m_sortMovieByList.Add(new SortMovieByPair(SortMoviesBy.UserRating, "Text.SortByUserRating"));

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

            }
            else
            {
                // Actress browser is shown

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

        public ObservableCollection<SortMovieByPair> SortMovieByList
        {
            get { return m_sortMovieByList; }
        }

        public SortMovieByPair CurrentSortMovieBy
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
        private ObservableCollection<SortMovieByPair> m_sortMovieByList = new ObservableCollection<SortMovieByPair>();
        private SortMovieByPair m_currentSortMovieBy;

        #endregion
    }
}
