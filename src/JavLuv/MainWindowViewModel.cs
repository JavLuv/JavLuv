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
            m_browserViewModel = new BrowserViewModel(this);
            m_sidePanelViewModel = new SidePanelViewModel(this);
            Overlay = null;

            // Get events for property changes
            m_movieScanner.ScanUpdate += OnMovieScannerUpdate;
            m_movieScanner.ScanComplete += OnMovieScannerComplete;
            m_movieCollection.MoviesDisplayedChanged += MovieCollection_MoviesDisplayedChanged;

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
                return m_mainPanelViewModel;
            }
            set
            {
                if (value != m_mainPanelViewModel)
                {
                    m_mainPanelViewModel = value;
                    NotifyPropertyChanged("Overlay");
                    SidePanel.IsEnabled = (m_mainPanelViewModel == null) ? true : false;
                    Browser.IsEnabled = (m_mainPanelViewModel == null) ? true : false;
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

        public SidePanelViewModel SidePanel { get { return m_sidePanelViewModel; } }

        public BrowserViewModel Browser { get { return m_browserViewModel; } }

        public SettingsViewModel Settings { get { return m_settingsViewModel; } }

        //public MovieScanner Scanner { get { return m_movieScanner; } }

        public MovieCollection Collection { get { return m_movieCollection; } }

        public bool IsScanning { get { return m_movieScanner.Phase != ScanPhase.Finished && m_movieScanner.Phase != ScanPhase.Cancelled; } }

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

            string errorMsg = String.Empty;
            var moviesAdded = m_movieCollection.AddMovies(m_movieScanner.Movies, out errorMsg);
            if (m_movieScanner.ErrorLog != String.Empty || errorMsg != String.Empty)
            {
                m_reportViewModel.ErrorLog = String.Empty;
                if (m_movieScanner.ErrorLog != String.Empty)
                    m_reportViewModel.ErrorLog += m_movieScanner.ErrorLog;
                if (errorMsg != String.Empty)
                    m_reportViewModel.ErrorLog += errorMsg;
                Overlay = m_reportViewModel;
                m_sidePanelViewModel.IsEnabled = false;
                ProgressState = TaskbarItemProgressState.Error;
            }

            // Optionally move/rename post-scan
            if (JavLuv.Settings.Get().EnableMoveRename && JavLuv.Settings.Get().MoveRenameAfterScan && m_movieScanner.Phase != ScanPhase.Cancelled)
            {
                m_browserViewModel.MoveRenameMovies(moviesAdded);
            }

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
                m_checkVersion.LatestRelease.tag_name != JavLuv.Settings.Get().LastVersionChecked)
            {

                // Create and prepare version dialog
                var versionCheck = new VersionCheckView();
                versionCheck.Owner = Application.Current.MainWindow;
                versionCheck.CurrentVersion.Text = String.Format(TextManager.GetString("Text.CurrentVersion"), m_checkVersion.CurrentVersion);
                versionCheck.NewVersion.Text = String.Format(TextManager.GetString("Text.NewVersion"), m_checkVersion.LatestRelease.tag_name);
                versionCheck.Details.Text = m_checkVersion.LatestRelease.body;

                // Store new version so we don't bother notifying again
                JavLuv.Settings.Get().LastVersionChecked = m_checkVersion.LatestRelease.tag_name;
                
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
            CloseOverlay();
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

        public void CloseOverlay()
        {
            if (Overlay != null)
            {
                if (Overlay.GetType() == typeof(DetailViewModel))
                {
                    Collection.Search();
                    Collection.Save();
                }
            }
            ProgressState = TaskbarItemProgressState.None;
            Overlay = null;
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

        public void CancelScan()
        {
            if (m_movieScanner != null)
                m_movieScanner.Cancel();
            ProgressState = TaskbarItemProgressState.None;
            NotifyPropertyChanged("IsScanning");
            NotifyPropertyChanged("ScanVisibility");
        }

        #endregion

        #region Private Members

        SidePanelViewModel m_sidePanelViewModel;
        SettingsViewModel m_settingsViewModel;
        ReportViewModel m_reportViewModel;
        BrowserViewModel m_browserViewModel;
        ObservableObject m_mainPanelViewModel;
        MovieScanner m_movieScanner;
        MovieCollection m_movieCollection;
        TaskbarItemProgressState m_progressState;
        CmdCheckVersion m_checkVersion;
        int m_percentComplete = 0;
        string m_scanStatus = String.Empty;
        string m_displayCountText = String.Empty;

        #endregion
    }
}
