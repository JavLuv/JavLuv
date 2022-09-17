using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace JavLuv
{
    public class BrowserViewModel : ObservableObject
    {
        #region Constructors

        public BrowserViewModel(MainWindowViewModel parent)
        {
            m_parent = parent;
            m_parent.Collection.MoviesDisplayedChanged += Collection_MoviesDisplayedChanged;
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        #endregion

        #region Public Functions

        public void NotifyAllProperties()
        {
            NotifyAllPropertiesChanged();
        }

        #endregion

        #region Properties

        public MainWindowViewModel Parent { get { return m_parent; } }

        public ObservableCollection<BrowserItemViewModel> Movies
        {
            get { return m_movies; }
        }

        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set
            {
                if (value != m_isEnabled)
                {
                    m_isEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
                }
            }
        }

        public ObservableCollection<BrowserItemViewModel> SelectedItems
        {
            get { return m_selectedItems; }
            set
            {
                if (value != m_selectedItems)
                {
                    m_selectedItems = value;
                    NotifyPropertyChanged("SelectedItems");
                }
            }
        }

        public string SelectedDescription
        {
            get { return m_selectedDescription; }
            set
            {
                if (value != m_selectedDescription)
                {
                    m_selectedDescription = value;
                    NotifyPropertyChanged("SelectedDescription");
                }
            }
        }

        public System.Windows.Visibility EnableMoveRenameVisibility
        {
            get
            {
                if (Settings.Get().EnableMoveRename)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility ShowAdvancedOptionsVisibility
        {
            get
            {
                if (Settings.Get().ShowAdvancedOptions)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility FindSubtitlesVisibility
        {
            get
            {
                if (Settings.Get().ShowAdvancedOptions && String.IsNullOrEmpty(Settings.Get().Subtitles) == false)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility PlayMovieVisibility { get; set; }

        public System.Windows.Visibility PlayRandomMovieVisibility { get; set; }

        #endregion

        #region Events

        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (SelectedItems.Count == 0)
            {
                SelectedDescription = "";
                PlayMovieVisibility = Visibility.Collapsed;
                PlayRandomMovieVisibility = Visibility.Collapsed;
            }
            else if (SelectedItems.Count == 1)
            {
                SelectedDescription = SelectedItems[0].DisplayTitle;
                PlayMovieVisibility = Visibility.Visible;
                PlayRandomMovieVisibility = Visibility.Collapsed;
            }
            else if (SelectedItems.Count > 1)
            {
                var str = new StringBuilder();
                foreach (var item in SelectedItems)
                {
                    str.Append(item.ID);
                    if (SelectedItems.IndexOf(item) != SelectedItems.Count - 1)
                        str.Append(", ");
                }
                SelectedDescription = str.ToString();
                PlayMovieVisibility = Visibility.Collapsed;
                PlayRandomMovieVisibility = Visibility.Visible;
            }
            NotifyPropertyChanged("PlayMovieVisibility");
            NotifyPropertyChanged("PlayRandomMovieVisibility");
        }

        private void Collection_MoviesDisplayedChanged(object sender, EventArgs e)
        {
            Movies.Clear();
            foreach (var movie in Parent.Collection.MoviesDisplayed)
                Movies.Add(new BrowserItemViewModel(this, movie));
        }

        #endregion

        #region Commands

        #region Play Movie Command

        private void PlayMovieExecute()
        {
            try
            {
                if (SelectedItems.Count == 0)
                    return;
                BrowserItemViewModel movieItem = null;
                if (SelectedItems.Count == 1)
                    movieItem = SelectedItems[0];
                else
                    movieItem = SelectedItems[m_random.Next(SelectedItems.Count)];
                string movieFileName = Path.Combine(movieItem.MovieData.Path, movieItem.MovieData.MovieFileNames[0]);
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.FileName = movieFileName;
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, TextManager.GetString("Text.ErrorPlayingMovie"));
            }
        }

        private bool CanPlayMovieExecute()
        {
            return true;
        }

        public ICommand PlayMovieCommand { get { return new RelayCommand(PlayMovieExecute, CanPlayMovieExecute); } }

        #endregion

        #region Navigate Left Command

        private void NavigateLeftExecute()
        {
            DetailViewModel current = Parent.Overlay as DetailViewModel;
            if (current == null)
                return;
            Parent.Overlay = new DetailViewModel(this, Movies[Movies.IndexOf(current.BrowserItem) - 1]);
        }

        private bool CanNavigateLeftExecute()
        {
            DetailViewModel current = Parent.Overlay as DetailViewModel;
            if (current == null)
                return false;
            if (Movies.IndexOf(current.BrowserItem) == 0)
                return false;
            return true;
        }

        public ICommand NavigateLeftCommand { get { return new RelayCommand(NavigateLeftExecute, CanNavigateLeftExecute); } }

        #endregion

        #region Navigate Right Command

        private void NavigateRightExecute()
        {
            DetailViewModel current = Parent.Overlay as DetailViewModel;
            if (current == null)
                return;
            Parent.Overlay = new DetailViewModel(this, Movies[Movies.IndexOf(current.BrowserItem) + 1]);
        }

        private bool CanNavigateRightExecute()
        {
            DetailViewModel current = Parent.Overlay as DetailViewModel;
            if (current == null)
                return false;
            if (Movies.IndexOf(current.BrowserItem) >= Movies.Count - 1)
                return false;
            return true;
        }

        public ICommand NavigateRightCommand { get { return new RelayCommand(NavigateRightExecute, CanNavigateRightExecute); } }

        #endregion

        #region Move/Rename Command

        private void MoveRenameExecute()
        {
            // Gather selected movies to move
            List<MovieData> moviesToMoveRename = new List<MovieData>();
            foreach (var item in SelectedItems)
                moviesToMoveRename.Add(item.MovieData);
            if (moviesToMoveRename.Count == 0)
                return;

            MoveRenameMovies(moviesToMoveRename);
        }

        private bool CanMoveRenameExecute()
        {
            return Parent.Scanner.IsScanning == false;
        }

        public ICommand MoveRenameCommand { get { return new RelayCommand(MoveRenameExecute, CanMoveRenameExecute); } }

        #endregion

        #region Move To Folders Command

        private void MoveToFoldersExecute()
        {
            // Select destination folder
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = Utilities.GetValidSubFolder(Settings.Get().LastFolder);
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            // Gather selected movies to move
            Settings.Get().LastFolder = dlg.SelectedPath;
            List<MovieData> moviesToMove = new List<MovieData>();
            foreach (var item in SelectedItems)
            {
                if (dlg.SelectedPath != item.MovieData.Path)
                    moviesToMove.Add(item.MovieData);
            }
            if (moviesToMove.Count == 0)
                return;

            // Create progress window to display modally as folders are moved
            var progress = new ProgressWindow();
            progress.Owner = App.Current.MainWindow;
            progress.Title = TextManager.GetString("Text.MovingMoviesTitle");
            progress.Message = TextManager.GetString("Text.MovingMoviesMessage");
            progress.TotalActions = moviesToMove.Count;
            progress.UpdateProgress();

            // Move folders as a background task
            CommandQueue.Command().Execute(new CmdMoveToFolder(progress, dlg.SelectedPath, moviesToMove, Parent.Collection));

            // Show progress dialog
            progress.ShowDialog();

            // Save collection, which will update folder status
            m_parent.Collection.Save();
        }

        private bool CanMoveToFoldersExecute()
        {
            return Parent.Scanner.IsScanning == false;
        }

        public ICommand MoveToFolderCommand { get { return new RelayCommand(MoveToFoldersExecute, CanMoveToFoldersExecute); } }

        #endregion

        #region Rescan Files Command

        private void RescanFilesExecute()
        {
            List<MovieData> movies = new List<MovieData>();
            foreach (var item in SelectedItems)
                movies.Add(item.MovieData);
            Parent.Collection.RemoveMovies(movies);
            HashSet<string> foldersToScan = new HashSet<string>();
            foreach (var item in SelectedItems)
                foldersToScan.Add(item.MovieData.Path);
            Parent.Scanner.Start(foldersToScan.ToList());
        }

        private bool CanRescanFilesExecute()
        {
            return Parent.Scanner.IsScanning == false;
        }

        public ICommand RescanFilesCommand { get { return new RelayCommand(RescanFilesExecute, CanRescanFilesExecute); } }

        #endregion

        #region Remove From Library Command

        private void RemoveFromLibraryExecute()
        {
            List<MovieData> movies = new List<MovieData>();
            foreach (var item in SelectedItems)
                movies.Add(item.MovieData);
            Parent.Collection.RemoveMovies(movies);
        }

        private bool CanRemoveFromLibraryExecute()
        {
            return Parent.Scanner.IsScanning == false;
        }

        public ICommand RemoveFromLibraryCommand { get { return new RelayCommand(RemoveFromLibraryExecute, CanRemoveFromLibraryExecute); } }

        #endregion

        #region Import Cover Image Command

        private void ImportCoverImageExecute()
        {
            var selectedItem = m_selectedItems[0];
            selectedItem.ImportCoverImage();
        }

        private bool CanImportCoverImageExecute()
        {
            return m_selectedItems.Count == 1;
        }

        public ICommand ImportCoverImageCommand { get { return new RelayCommand(ImportCoverImageExecute, CanImportCoverImageExecute); } }

        #endregion

        #region Regenerate Metadata Command

        private void RegenerateMetadataExecute()
        {
            List<MovieData> movies = new List<MovieData>();
            HashSet<string> foldersToScan = new HashSet<string>();
            foreach (var item in SelectedItems)
            {
                movies.Add(item.MovieData);
                foldersToScan.Add(item.MovieData.Path);
            }
            Parent.Collection.DeleteMetadata(movies);
            Parent.Scanner.Start(foldersToScan.ToList());
        }

        private bool CanRegenerateMetadataExecute()
        {
            return Parent.Scanner.IsScanning == false;
        }

        public ICommand RegenerateMetadataCommand { get { return new RelayCommand(RegenerateMetadataExecute, CanRegenerateMetadataExecute); } }

        #endregion

        #region Filter Metadata Command

        private void FilterMetadataExecute()
        {
            List<MovieData> movies = new List<MovieData>();
            foreach (var item in SelectedItems)
                movies.Add(item.MovieData);
            Parent.Collection.FilterMetadata(movies, Settings.Get().Culture.StudioFilters, Settings.Get().Culture.LabelFilters, 
                Settings.Get().Culture.DirectorFilters, Settings.Get().Culture.GenreFilters, Settings.Get().Culture.ActorFilters);
        }

        private bool CanFilterMetadataExecute()
        {
            return Parent.Scanner.IsScanning == false;
        }

        public ICommand FilterMetadataCommand { get { return new RelayCommand(FilterMetadataExecute, CanFilterMetadataExecute); } }

        #endregion

        #region Delete Metadata Command

        private void DeleteMetadataExecute()
        {
            List<MovieData> movies = new List<MovieData>();
            foreach (var item in SelectedItems)
                movies.Add(item.MovieData);
            Parent.Collection.DeleteMetadata(movies);
        }

        private bool CanDeleteMetadataExecute()
        {
            return Parent.Scanner.IsScanning == false;
        }

        public ICommand DeleteMetadataCommand { get { return new RelayCommand(DeleteMetadataExecute, CanDeleteMetadataExecute); } }

        #endregion

        #region Find Subtitles Command

        private void FindSubtitlesExecute()
        {
            // Select destination folder
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = Utilities.GetValidSubFolder(Settings.Get().LastFolder);
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            // Gather selected movies to move
            Settings.Get().LastFolder = dlg.SelectedPath;
            List<string> movieIDs = new List<string>();
            foreach (var item in SelectedItems)
            {
                if (item.MovieData.SubtitleFileNames.Count == 0)
                    movieIDs.Add(item.MovieData.Metadata.UniqueID.Value);
            }
            if (movieIDs.Count == 0)
                return;

            // Move folders as a background task
            CommandQueue.Command().Execute(new CmdFindSubtitles(movieIDs, dlg.SelectedPath));
        }

        private bool CanFindSubtitlesExecute()
        {
            return true;
        }

        public ICommand FindSubtitlesCommand { get { return new RelayCommand(FindSubtitlesExecute, CanFindSubtitlesExecute); } }

        #endregion

        #endregion

        #region Public Functions

        public void OpenDetailView(BrowserItemViewModel browserItem)
        {
            Parent.Overlay = new DetailViewModel(this, browserItem);
        }

        public void MoveRenameMovies(List<MovieData> movies)
        {
            if (movies.Count == 0)
                return;

            // Create progress window to display modally as folders are moved
            var progress = new ProgressWindow();
            progress.Owner = App.Current.MainWindow;
            progress.Title = TextManager.GetString("Text.MovingRenamingMoviesTitle");
            progress.Message = TextManager.GetString("Text.MovingRenamingMoviesMessage");
            progress.TotalActions = movies.Count;
            progress.UpdateProgress();

            // Move/rename movies as a background task
            CommandQueue.Command().Execute(new CmdMoveRename(Parent.Collection, progress, movies));

            // Show progress dialog
            progress.ShowDialog();

            // Save collection, which will update folder status
            m_parent.Collection.Save();
        }

        public bool ImportCoverImage(MovieData movieData)
        {
            try
            {
                var openFileDlg = new System.Windows.Forms.OpenFileDialog();
                openFileDlg.Filter = "Image files (*.jpg;*.jpeg;*.png;*.webp;*.gif)|*.jpg;*.jpeg;*.png;*.webp;*.gif|All files(*.*)|*.*";
                openFileDlg.InitialDirectory = movieData.Path;
                openFileDlg.CheckFileExists = true;
                openFileDlg.CheckPathExists = true;
                openFileDlg.Multiselect = false;
                var results = openFileDlg.ShowDialog();
                if (results == System.Windows.Forms.DialogResult.OK)
                {
                    string destFileName;
                    if (String.IsNullOrEmpty(movieData.CoverFileName) == false)
                    {
                        destFileName = Path.Combine(movieData.Path, movieData.CoverFileName);
                        if (String.Compare(openFileDlg.FileName, destFileName, true) == 0)
                            return false;
                        File.Delete(destFileName);
                        ImageCache.Get().Delete(destFileName);
                    }
                    else
                    {
                        destFileName = Path.Combine(movieData.Path, movieData.MovieFileNames[0]);
                    }
                    destFileName = Path.ChangeExtension(destFileName, Path.GetExtension(openFileDlg.FileName));
                    Utilities.MoveFile(openFileDlg.FileName, destFileName);
                    movieData.CoverFileName = Path.GetFileName(destFileName);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("Issue importing cover image", ex);
                return false;
            }


            return true;
        }

        #endregion

        #region Private Members

        private MainWindowViewModel m_parent;
        private ObservableCollection<BrowserItemViewModel> m_movies = new ObservableCollection<BrowserItemViewModel>();
        private ObservableCollection<BrowserItemViewModel> m_selectedItems = new ObservableCollection<BrowserItemViewModel>();
        private string m_selectedDescription = String.Empty;
        private bool m_isEnabled = true;
        private Random m_random = new Random();

        #endregion
    }
}
