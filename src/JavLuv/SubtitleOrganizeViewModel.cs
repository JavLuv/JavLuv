using Common;
using Subtitles;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace JavLuv
{
    public class OrganizerModeItem : ObservableObject
    {
        #region Constructor

        public OrganizerModeItem(Organizer.Mode mode, string name)
        {
            Mode = mode;
            Name = name;
        }

        #endregion

        #region Properties

        public Organizer.Mode Mode { get; private set; }
        public string Name { get; private set; }

        #endregion
    }

    public class SubtitleOrganizeViewModel : ObservableObject
    {
        #region Constructor

        public SubtitleOrganizeViewModel(SidePanelViewModel parent)
        {
            m_parent = parent;
            
            // Build modes list and set selected
            Modes = new ObservableCollection<OrganizerModeItem>();
            Modes.Add(new OrganizerModeItem(Organizer.Mode.Copy, TextManager.GetString("Text.OrganizeModeCopy")));
            Modes.Add(new OrganizerModeItem(Organizer.Mode.Move, TextManager.GetString("Text.OrganizeModeMove")));
            SelectedModeItem = Modes[(int)Settings.Get().OrganizerMode];

            // Get initial folders from settings
            ImportFolder = Settings.Get().SubtitleImportFolder;
            ExportFolder = Settings.Get().SubtitleExportFolder;

            m_organizerNotRunning = true;
        }

        #endregion

        #region Events

        public EventHandler Start;
        public EventHandler Finished;

        #endregion

        #region Properties

        public ObservableCollection<OrganizerModeItem> Modes 
        { 
            get { return m_modes; }
            private set
            {
                if (value != m_modes)
                {
                    m_modes = value;
                    NotifyPropertyChanged("Modes");
                }
            }
        }

        public OrganizerModeItem SelectedModeItem { get; set; }

        public string ImportFolder
        { 
            get { return m_importFolder;}
            set
            {
                if (value != m_importFolder)
                {
                    m_importFolder = value;
                    NotifyPropertyChanged("ImportFolder");
                }
            }
        }

        public string ExportFolder
        {
            get { return m_exportFolder; }
            set
            {
                if (value != m_exportFolder)
                {
                    m_exportFolder = value;
                    NotifyPropertyChanged("ExportFolder");
                }
            }
        }

        public string SubtitleFileStats
        {
            get { return m_subtitleFileStats; }
            private set
            {
                if (value != m_subtitleFileStats)
                {
                    m_subtitleFileStats = value;
                    NotifyPropertyChanged("SubtitleFileStats");
                }
            }
        }    

        public bool OrganizerNotRunning
        {
            get { return m_organizerNotRunning; }
            private set
            {
                if (value != m_organizerNotRunning)
                {
                    m_organizerNotRunning = value;
                    NotifyPropertyChanged("OrganizerNotRunning");
                }
            }

        }

        #endregion

        #region Commands

        #region Browse Import Folder Command

        private void BrowseImportFolderExecute()
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = Utilities.GetValidSubFolder(ImportFolder);
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;
            ImportFolder = dlg.SelectedPath;
        }

        private bool CanBrowseImportFolderExecute()
        {
            return true;
        }

        public ICommand BrowseImportFolderCommand { get { return new RelayCommand(BrowseImportFolderExecute, CanBrowseImportFolderExecute); } }

        #endregion

        #region Browse Export Folder Command

        private void BrowseExportFolderExecute()
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = Utilities.GetValidSubFolder(Utilities.GetValidSubFolder(ExportFolder));
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;
            ExportFolder = dlg.SelectedPath;
        }

        private bool CanBrowseExportFolderExecute()
        {
            return true;
        }

        public ICommand BrowseExportFolderCommand { get { return new RelayCommand(BrowseExportFolderExecute, CanBrowseExportFolderExecute); } }

        #endregion

        #region Sort Command

        private void SortExecute()
        {
            // Save settings only when we begin sorting
            Settings.Get().SubtitleImportFolder = ImportFolder;
            Settings.Get().SubtitleExportFolder = ExportFolder;
            Settings.Get().OrganizerMode = SelectedModeItem.Mode;

            // Begin sorting
            m_organizer = new Organizer(App.Current.Dispatcher);
            m_organizer.ImportFolder = ImportFolder;
            m_organizer.ExportFolder = ExportFolder;
            m_organizer.ProcessingMode = SelectedModeItem.Mode;

            m_organizer.SubtitleExts = Utilities.ProcessSettingsList(Settings.Get().SubtitleExts);
            m_organizer.Finished += OnFinished;
            m_organizer.FileProcessed += OnFileProcessed;
            SubtitleFileStats = "";
            m_organizer.Start();
            OrganizerNotRunning = false;

            Start?.Invoke(this, new EventArgs());
            var mainWindowViewModel = App.Current.MainWindow.DataContext as MainWindowViewModel;
            mainWindowViewModel.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
        }

        private bool CanSortExecute()
        {
            return true;
        }

        public ICommand SortCommand { get { return new RelayCommand(SortExecute, CanSortExecute); } }

        #endregion

        #region Close Command

        private void CloseExecute()
        {
            if (m_organizer != null)
            {
                m_organizer.Cancel();
                m_organizer.Finished -= OnFinished;
                m_organizer.FileProcessed -= OnFileProcessed;
                m_organizer = null;
            }
            OrganizerNotRunning = true;
            m_organizer = null;
            Finished?.Invoke(this, new EventArgs());
            var mainWindowViewModel = App.Current.MainWindow.DataContext as MainWindowViewModel;
            mainWindowViewModel.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
        }

        private bool CanCloseExecute()
        {
            return true;
        }

        public ICommand CloseCommand { get { return new RelayCommand(CloseExecute, CanCloseExecute); } }

        #endregion

        #endregion

        #region Event Handlers

        private void OnFileProcessed(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(1000);
            sb.AppendLine(String.Format(TextManager.GetString("Text.SubtitleFilesProcessed"), m_organizer.FilesProcessed));
            sb.AppendLine(String.Format(TextManager.GetString("Text.SubtitleFilesDuplicate"), m_organizer.FilesDuplicate));
            sb.AppendLine(String.Format(TextManager.GetString("Text.SubtitleFilesNoID"), m_organizer.FilesNoID));
            sb.AppendLine(String.Format(TextManager.GetString("Text.SubtitleFilesEncodingFixed"), m_organizer.FilesEncodingFixed));
            sb.AppendLine(String.Format(TextManager.GetString("Text.SubtitleFilesExtensionFixed"), m_organizer.FilesExtensionFixed));
            sb.AppendLine(String.Format(TextManager.GetString("Text.SubtitleFilesImported"), m_organizer.FilesImported));
            SubtitleFileStats = sb.ToString();
        }

        private void OnFinished(object sender, EventArgs e)
        {
            CloseCommand?.Execute(null);
        }

        #endregion 

        #region Private Members

        private SidePanelViewModel m_parent;
        private ObservableCollection<OrganizerModeItem> m_modes;
        private string m_importFolder;
        private string m_exportFolder;
        private string m_subtitleFileStats;
        private Organizer m_organizer;
        private bool m_organizerNotRunning;

        #endregion
    }
}
