using Common;
using MovieInfo;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JavLuv
{
    public class SortByPair : ObservableObject
    {
        #region Constructor

        public SortByPair(SortBy sortBy, string key)
        {
            Type = sortBy;
            m_key = key;
        }

        #endregion

        #region Properties

        public SortBy Type { get; private set; }
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

            m_sortByList.Add(new SortByPair(SortBy.Title, "Text.SortByTitle"));
            m_sortByList.Add(new SortByPair(SortBy.ID, "Text.SortByID"));
            m_sortByList.Add(new SortByPair(SortBy.Actress, "Text.SortByActress"));
            m_sortByList.Add(new SortByPair(SortBy.Date_Newest, "Text.SortByDateNewest"));
            m_sortByList.Add(new SortByPair(SortBy.Date_Oldest, "Text.SortByDateOldest"));
            m_sortByList.Add(new SortByPair(SortBy.UserRating, "Text.SortByUserRating"));

            foreach (var sortBy in m_sortByList)
            {
                if (sortBy.Type == Settings.Get().SortBy)
                {
                    CurrentSortBy = sortBy;
                    Parent.Collection.SortBy = sortBy.Type;
                    break;
                }
            }
            NotifyPropertyChanged("SortByList");          
        }

        #endregion

        #region Public Functions

        public void NotifyAllProperty()
        {
            NotifyAllPropertiesChanged();
            foreach (var sortByPair in Parent.SidePanel.SortByList)
                sortByPair.Refresh();
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

        public ObservableCollection<SortByPair> SortByList
        {
            get { return m_sortByList; }
        }

        public SortByPair CurrentSortBy
        {
            get { return m_currentSortBy; }
            set
            {
                if (value != m_currentSortBy)
                {
                    m_currentSortBy = value;
                    Settings.Get().SortBy = m_currentSortBy.Type;
                    NotifyPropertyChanged("CurrentSortBy");
                    Parent.Collection.SortBy = m_currentSortBy.Type;
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
            if (Parent.Scanner.IsScanning)
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
        private ObservableCollection<SortByPair> m_sortByList = new ObservableCollection<SortByPair>();
        private SortByPair m_currentSortBy;

        #endregion
    }
}
