using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;

namespace JavLuv
{

    public enum AppState
    {
        MovieBrowser,
        MovieDetail,
        ActressBrowser,
        ActressDetail,
        Settings,
        Report,
    }

    public class MainWindowViewModel : ObservableObject
    {
        #region Constructors

        public MainWindowViewModel()
        {
            Logger.WriteInfo("Main window view model initialized");

            // Catch any unhandled exceptions and display them
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = args.ExceptionObject as Exception;
                App.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    if (ex == null)
                        MessageBox.Show("Unknown (null) exception.", "Fatal error");
                    else
                        MessageBox.Show("Unhandled exception: " + ex.ToString(), "Fatal error");
                }));

                Logger.WriteError("Unhandled exception", ex);
            };

            m_movieCollection = new MovieCollection(Application.Current.Dispatcher);
            m_movieScanner = new MovieScanner(m_movieCollection);
            m_settingsViewModel = new SettingsViewModel(this);
            m_reportViewModel = new ReportViewModel(this);
            m_movieBrowserViewModel = new MovieBrowserViewModel(this);
            m_actressBrowserViewModel = new ActressBrowserViewModel(this);
            m_sidePanelViewModel = new SidePanelViewModel(this);
            Overlay = null;

            // Get events for property changes
            m_movieScanner.ScanUpdate += OnMovieScannerUpdate;
            m_movieScanner.ScanComplete += OnMovieScannerComplete;
            m_movieCollection.MoviesDisplayedChanged += MovieCollection_MoviesDisplayedChanged;

            // Set loaded UI elemenets
            SelectedTabIndex = JavLuv.Settings.Get().SelectedTabIndex;
            State = SelectedTabIndex == 0 ? AppState.MovieBrowser : AppState.ActressBrowser;

            // Set state initially
            ChangeState(null);

            // Check version
            var timeToCheck = new TimeSpan(1, 0, 0, 0); // 1 day interval
            var interval = new TimeSpan(0, 0, 0, 0);
            if (JavLuv.Settings.Get().LastVersionCheckTime != null)
                interval = DateTime.Now - JavLuv.Settings.Get().LastVersionCheckTime;
            if (JavLuv.Settings.Get().CheckForUpdates && interval > timeToCheck)
            {
                JavLuv.Settings.Get().LastVersionCheckTime = DateTime.Now;
                m_checkVersion = new CmdCheckVersion();
                m_checkVersion.FinishedVersionCheck += CheckVersion_FinishedVersionCheck;
                CommandQueue.LongTask().Execute(m_checkVersion);
            }
        }

        #endregion

        #region Properties

        public ObservableObject Overlay
        {
            get
            {
                return m_overlayViewModel;
            }
            set
            {
                if (value != m_overlayViewModel)
                {
                    // This is a special case.  Preserve the old overlay view model if viewing a movie
                    // from the actress detail page, and restore it once we're done.
                    if (m_overlayViewModel is ActressDetailViewModel && value is MovieDetailViewModel)
                        m_previousActressDetailViewModel = m_overlayViewModel as ActressDetailViewModel;
                    else if (value == null && m_previousActressDetailViewModel != null)
                    {
                        value = new ActressDetailViewModel(m_previousActressDetailViewModel);
                        m_previousActressDetailViewModel = null;
                    }   
                    
                    // Handle various conditions when changing state
                    ChangeState(value);

                    m_overlayViewModel = value;
                    NotifyPropertyChanged("Overlay");
                }
            }
        }

        public GridLength SearchViewWidth
        {
            get
            {
                return JavLuv.Settings.Get().SearchViewWidth;
            }
            set
            {
                if (value != JavLuv.Settings.Get().SearchViewWidth)
                {
                    double d = Math.Max(Math.Min(value.Value, 350), 170);
                    JavLuv.Settings.Get().SearchViewWidth = new GridLength(d);
                }
                NotifyPropertyChanged("SearchViewWidth");
            }
        }

        public AppState State { get; private set; }

        public SidePanelViewModel SidePanel { get { return m_sidePanelViewModel; } }

        public MovieBrowserViewModel MovieBrowser { get { return m_movieBrowserViewModel; } }

        public ActressBrowserViewModel ActressBrowser { get { return m_actressBrowserViewModel; } }

        public SettingsViewModel Settings { get { return m_settingsViewModel; } }

        public MovieCollection Collection { get { return m_movieCollection; } }

        public int SelectedTabIndex
        {
            get
            {
                return JavLuv.Settings.Get().SelectedTabIndex;
            }
            set
            {
                if (value != JavLuv.Settings.Get().SelectedTabIndex)
                {
                    JavLuv.Settings.Get().SelectedTabIndex = value;
                    NotifyPropertyChanged("SelectedTabIndex");
                    ChangeState(null);
                }
            }
        }

        public bool IsScanning { get { return m_movieScanner.Phase != ScanPhase.Finished; } }

        public Visibility ScanVisibility
        {
            get
            {
                if (IsScanning)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Hidden;
            }
        }

        public TaskbarItemProgressState ProgressState
        {
            get
            {
                return m_progressState;
            }
            set
            {
                if (value != m_progressState)
                {
                    m_progressState = value;
                    NotifyPropertyChanged("ProgressState");
                }
            }
        }

        public double ProgressValue
        {
            get
            {
                return (double)m_percentComplete / 100.0;
            }
        }

        public int PercentComplete
        {
            get
            {
                return m_percentComplete;
            }
            set
            {
                if (value != m_percentComplete)
                {
                    m_percentComplete = value;
                    NotifyPropertyChanged("PercentComplete");
                    NotifyPropertyChanged("ProgressValue");
                }
            }
        }

        public string ScanStatus
        {
            get
            {
                return m_scanStatus;
            }
            set
            {
                if (value != m_scanStatus)
                {
                    m_scanStatus = value;
                    NotifyPropertyChanged("ScanStatus");
                }
            }
        }

        public string DisplayCountText
        {
            get
            {
                return m_displayCountText;
            }
            set
            {
                if (value != m_displayCountText)
                {
                    m_displayCountText = value;
                    NotifyPropertyChanged("DisplayCountText");
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnMovieScannerComplete(object sender, EventArgs e)
        {
            SidePanel.SettingsIsEnabled = true;
            ProgressState = TaskbarItemProgressState.None;
            ScanStatus = String.Empty;
            NotifyPropertyChanged("IsScanning");
            NotifyPropertyChanged("ScanVisibility");

            // Add new actresses to actress database
            m_movieCollection.AddActresses(m_movieScanner.Actresses);

            string errorMsg = String.Empty;
            if (m_movieScanner.ErrorLog != String.Empty)
            {
                m_reportViewModel.ErrorLog = String.Empty;
                if (m_movieScanner.ErrorLog != String.Empty)
                    m_reportViewModel.ErrorLog += m_movieScanner.ErrorLog;
                if (errorMsg != String.Empty)
                    m_reportViewModel.ErrorLog += errorMsg;
                Overlay = m_reportViewModel;
                ProgressState = TaskbarItemProgressState.Error;
            }

            // Optionally move/rename post-scan
            if (JavLuv.Settings.Get().EnableMoveRename && JavLuv.Settings.Get().MoveRenameAfterScan && m_movieScanner.IsCancelled == false)
                m_movieBrowserViewModel.MoveRenameMovies(m_movieScanner.Movies);

            m_movieCollection.AddMovies(m_movieScanner.Movies);
            m_movieScanner.Clear();
        }

        private void OnMovieScannerUpdate(object sender, EventArgs e)
        {
            if (m_movieScanner.Phase == ScanPhase.ScanningFolders)
            {
                ProgressState = TaskbarItemProgressState.Indeterminate;
                PercentComplete = 0;
                ScanStatus = string.Format(TextManager.GetString("Text.ScanningFolders"), m_movieScanner.ItemsProcessed);
            }
            else if (m_movieScanner.Phase == ScanPhase.LoadingMetadata)
            {
                ProgressState = TaskbarItemProgressState.Normal;
                PercentComplete = (int)(((float)m_movieScanner.ItemsProcessed / (float)m_movieScanner.TotalItems) * 100.0);
                ScanStatus = string.Format(TextManager.GetString("Text.LoadingMetadata"), m_movieScanner.ItemsProcessed, m_movieScanner.TotalItems);
            }
            else if (m_movieScanner.Phase == ScanPhase.DownloadMetadata)
            {
                ProgressState = TaskbarItemProgressState.Normal;
                PercentComplete = (int)(((float)m_movieScanner.ItemsProcessed / (float)m_movieScanner.TotalItems) * 100.0);
                ScanStatus = string.Format(TextManager.GetString("Text.DownloadingMetadata"), m_movieScanner.ItemsProcessed, m_movieScanner.TotalItems);
            }
            else if (m_movieScanner.Phase == ScanPhase.DownloadActressData)
            {
                ProgressState = TaskbarItemProgressState.Normal;
                PercentComplete = (int)(((float)m_movieScanner.ItemsProcessed / (float)m_movieScanner.TotalItems) * 100.0);
                ScanStatus = string.Format(TextManager.GetString("Text.DownloadingActressData"), m_movieScanner.ItemsProcessed, m_movieScanner.TotalItems);
            }
        }

        private void MovieCollection_MoviesDisplayedChanged(object sender, EventArgs e)
        {
            DisplayCountText = String.Format(
                TextManager.GetString("Text.DisplayingMovies"),
                m_movieCollection.MoviesDisplayed.Count,
                m_movieCollection.NumMovies
                );        
        }

        private void CheckVersion_FinishedVersionCheck(object sender, EventArgs e)
        {
            if (m_checkVersion.IsNewVersionAvailable && 
                m_checkVersion.LatestVersion.CompareTo(JavLuv.Settings.Get().LastVersionChecked) != 0)
            {
                // Create and prepare version dialog
                var versionCheck = new VersionCheckView();
                versionCheck.Owner = Application.Current.MainWindow;
                versionCheck.CurrentVersion.Text = String.Format(TextManager.GetString("Text.CurrentVersion"), SemanticVersion.Current);
                versionCheck.NewVersion.Text = String.Format(TextManager.GetString("Text.NewVersion"), m_checkVersion.LatestVersion.ToString());
                versionCheck.Details.Text = m_checkVersion.LatestRelease.body;

                // Store new version so we don't bother notifying again
                JavLuv.Settings.Get().LastVersionChecked = m_checkVersion.LatestVersion;
                
                // Show the dialog box - hey, we have a new version!
                versionCheck.ShowDialog();
            }
            m_checkVersion = null;
        }

        #endregion

        #region Commands

        #region Close Overlay Command

        private void CloseOverlayExecute()
        {
            Overlay = null;
        }

        private bool CanCloseOverlayExecute()
        {
            return true;
        }

        public ICommand CloseOverlayCommand { get { return new RelayCommand(CloseOverlayExecute, CanCloseOverlayExecute); } }

        #endregion

        #region Cancel Scan Command

        private void CancelScanExecute()
        {
            CancelScan();
        }

        private bool CanCancelScanExecute()
        {
            return true;
        }

        public ICommand CancelScanCommand { get { return new RelayCommand(CancelScanExecute, CanCancelScanExecute); } }

        #endregion

        #endregion

        #region Public Functions

        public void OpenSettings()
        {
            Overlay = m_settingsViewModel;
        }

        public void StartScan(string scanDirectory)
        {
            m_movieScanner.Start(scanDirectory);
            SidePanel.SettingsIsEnabled = false;
            NotifyPropertyChanged("IsScanning");
            NotifyPropertyChanged("ScanVisibility");
        }

        public void StartScan(List<string> scanDirectories)
        {
            m_movieScanner.Start(scanDirectories);
            SidePanel.SettingsIsEnabled = false;
            NotifyPropertyChanged("IsScanning");
            NotifyPropertyChanged("ScanVisibility");
        }

        public void StartScan(List<ActressData> actresses)
        {
            m_movieScanner.Start(actresses);
            SidePanel.SettingsIsEnabled = false;
            NotifyPropertyChanged("IsScanning");
            NotifyPropertyChanged("ScanVisibility");
        }

        public void CancelScan()
        {
            if (m_movieScanner != null)
                m_movieScanner.Cancel();
            ProgressState = TaskbarItemProgressState.None;
            NotifyPropertyChanged("IsScanning");
            NotifyPropertyChanged("ScanVisibility");
        }

        #endregion

        #region Private Functions

        private void ChangeState(ObservableObject newOverlay)
        {
            // Determine new state by overlay and existing tab position
            AppState newState = State;
            if (newOverlay is SettingsViewModel)
                newState = AppState.Settings;
            else if (newOverlay is MovieDetailViewModel)
                newState = AppState.MovieDetail;
            else if (newOverlay is ActressDetailViewModel)
                newState = AppState.ActressDetail;
            else // (newOverlay == null)
            {
                if (SelectedTabIndex == 0)
                    newState = AppState.MovieBrowser;
                else
                    newState = AppState.ActressBrowser;
            }

            Logger.WriteInfo("Switching state: " + newState.ToString());

            switch (newState)
            {
                case AppState.MovieBrowser:
                    MovieBrowser.IsEnabled = true;
                    ActressBrowser.IsEnabled = false;
                    SidePanel.IsCommandViewEnabled = true;
                    SidePanel.IsSearchViewEnabled = true;
                    SidePanel.MovieControlsVisibility = Visibility.Visible;
                    SidePanel.ActressControlsVisibility = Visibility.Collapsed;
                    Collection.MovieSearchActress = String.Empty;
                    Collection.SearchMovies();
                    break;
                case AppState.MovieDetail:
                    MovieBrowser.IsEnabled = false;
                    ActressBrowser.IsEnabled = false;
                    SidePanel.IsCommandViewEnabled = false;
                    SidePanel.IsSearchViewEnabled = false;
                    break;
                case AppState.ActressBrowser:
                    MovieBrowser.IsEnabled = false;
                    ActressBrowser.IsEnabled = true;
                    SidePanel.IsCommandViewEnabled = true;
                    SidePanel.IsSearchViewEnabled = true;
                    SidePanel.MovieControlsVisibility = Visibility.Collapsed;
                    SidePanel.ActressControlsVisibility = Visibility.Visible;
                    Collection.SearchActresses();
                    break;
                case AppState.ActressDetail:
                    MovieBrowser.IsEnabled = true;
                    ActressBrowser.IsEnabled = false;
                    SidePanel.IsCommandViewEnabled = false;
                    SidePanel.IsSearchViewEnabled = true;
                    SidePanel.MovieControlsVisibility = Visibility.Visible;
                    SidePanel.ActressControlsVisibility = Visibility.Collapsed;
                    break;
                case AppState.Settings:
                    MovieBrowser.IsEnabled = false;
                    ActressBrowser.IsEnabled = false;
                    SidePanel.IsCommandViewEnabled = false;
                    SidePanel.IsSearchViewEnabled = false;
                    break;
                case AppState.Report:
                    MovieBrowser.IsEnabled = false;
                    ActressBrowser.IsEnabled = false;
                    SidePanel.IsCommandViewEnabled = false;
                    SidePanel.IsSearchViewEnabled = false;
                    break;
            };

            State = newState;
        }

        #endregion

        #region Private Members

        private SidePanelViewModel m_sidePanelViewModel;
        private SettingsViewModel m_settingsViewModel;
        private ReportViewModel m_reportViewModel;
        private MovieBrowserViewModel m_movieBrowserViewModel;
        private ActressBrowserViewModel m_actressBrowserViewModel;
        private ObservableObject m_overlayViewModel;
        private ActressDetailViewModel m_previousActressDetailViewModel;
        private MovieScanner m_movieScanner;
        private MovieCollection m_movieCollection;
        private TaskbarItemProgressState m_progressState;
        private CmdCheckVersion m_checkVersion;
        private int m_percentComplete = 0;
        private string m_scanStatus = String.Empty;
        private string m_displayCountText = String.Empty;

        #endregion
    }
}
