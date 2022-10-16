using Common;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;

namespace JavLuv
{
    public class ScanViewModel : ObservableObject
    {
        #region Constructors

        public ScanViewModel(SidePanelViewModel parent)
        {
            m_parent = parent;
            ScanFolder = Settings.Get().ScanFolder;
            ScanRecursively = Settings.Get().ScanRecursively;
            MoveRenameAfterScan = Settings.Get().MoveRenameAfterScan;
        }

        #endregion

        #region Properties

        public SidePanelViewModel Parent { get { return m_parent; } }

        public bool ScanRecursively
        {
            get
            {
                return m_scanRecursively;
            }
            set
            {
                if (value != m_scanRecursively)
                {
                    m_scanRecursively = value;
                    NotifyPropertyChanged("ScanRecursively");
                }
            }
        }

        public bool MoveRenameAfterScan
        {
            get
            {
                return m_moveRenameAfterScan;
            }
            set
            {
                if (value != m_moveRenameAfterScan)
                {
                    m_moveRenameAfterScan = value;
                    NotifyPropertyChanged("MoveRenameAfterScan");
                }
            }
        }

        public System.Windows.Visibility MoveRenameAfterScanVisibility
        {
            get
            {
                if (Settings.Get().EnableMoveRename)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public string ScanFolder
        {
            get
            {
                return m_scanFolder;
            }
            set
            {
                if (value != m_scanFolder)
                {
                    m_scanFolder = value;
                    NotifyPropertyChanged("ScanFolder");
                }
            }
        }


        #endregion

        #region Commands

        #region Browse Folder Command

        private void BrowseFolderExecute()
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.EnsurePathExists = true;
            dlg.InitialDirectory = Utilities.GetValidSubFolder(Settings.Get().ScanFolder);
            var result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;
            ScanFolder = dlg.FileName;
        }

        private bool CanBrowseFolderExecute()
        {
            return true;
        }

        public ICommand BrowseFolderCommand { get { return new RelayCommand(BrowseFolderExecute, CanBrowseFolderExecute); } }

        #endregion

        #region Scan Movies Command

        private void ScanMoviesExecute()
        {
            Settings.Get().ScanFolder = ScanFolder;
            Settings.Get().ScanRecursively = ScanRecursively;
            Settings.Get().MoveRenameAfterScan = MoveRenameAfterScan;
            Parent.Parent.StartScan(ScanFolder);
        }

        private bool CanScanMoviesExecute()
        {
            return true;
        }

        public ICommand ScanMoviesCommand { get { return new RelayCommand(ScanMoviesExecute, CanScanMoviesExecute); } }

        #endregion

        #endregion

        #region Private Members

        private SidePanelViewModel m_parent;
        private bool m_scanRecursively;
        private bool m_moveRenameAfterScan;
        private string m_scanFolder;

        #endregion
    }
}
