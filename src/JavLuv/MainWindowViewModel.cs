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
                    // from the actress detail page, and preserve it once we're done.
                    if (m_overlayViewModel is ActressDetailViewModel && value is MovieDetailViewModel)
                        m_previousActressDetailViewModel = m_overlayViewModel as ActressDetailViewModel;
                    else if (value == null && m_previousActressDetailViewModel != null)
                    {
                        value = new ActressDetailViewModel(m_previousActressDetailViewModel);
                        m_previousActressDetailViewModel = null;
                    }   
                    
                    m_overlayViewModel = value;
                    NotifyPropertyChanged("Overlay");

                    // Selectively enable or disable various view models depending on combination
                    SidePanel.IsEnabled = (m_overlayViewModel == null) ? true : false;
                    if (m_overlayViewModel == null || m_overlayViewModel is ActressDetailViewModel)
                        MovieBrowser.IsEnabled = true;
                    else
                        MovieBrowser.IsEnabled = false;

                    if (m_overlayViewModel == null)
                    {
                        ActressBrowser.IsEnabled = true;
                        Collection.MovieSearchActress = String.Empty;
                        Collection.SearchMovies();
                    }
                    else
                    {
                        ActressBrowser.IsEnabled = false;
                    }
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
                    SidePanel.OnChangeTabs();
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
                m_sidePanelViewModel.IsEnabled = false;
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
                if (Overlay.GetType() == typeof(MovieDetailViewModel))
                {
                    Collection.SearchMovies();
                    Collection.Save();
                }
                else if (Overlay.GetType() == typeof(ActressDetailViewModel))
                {
                    Collection.SearchActresses();
                    Collection.SearchMovies();
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
        MovieBrowserViewModel m_movieBrowserViewModel;
        ActressBrowserViewModel m_actressBrowserViewModel;
        ObservableObject m_overlayViewModel;
        ActressDetailViewModel m_previousActressDetailViewModel;
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
