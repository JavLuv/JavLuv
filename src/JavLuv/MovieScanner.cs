using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WebScraper;

namespace JavLuv
{
    public enum ScanPhase
    {
        ScanningFolders,
        LoadingMetadata,
        ImportMovies,
        DownloadMetadata,
        DownloadActressData,
        Finished,
    }

    public class MovieScanner
    {
        #region Constructors

        public MovieScanner(MovieCollection movieCollection)
        {
            m_directoriesToScan = new List<string>();
            m_movieCollection = movieCollection;
            m_dispatcher = Application.Current.Dispatcher;
            Phase = ScanPhase.Finished;
            Movies = new List<MovieData>();
            Actresses = new List<ActressData>();
        }

        #endregion

        #region Events

        public event EventHandler ScanUpdate;
        public event EventHandler ScanComplete;

        #endregion

        #region Properties

        public ScanPhase Phase { get; private set; }
        public int ItemsProcessed { get; private set; }
        public int TotalItems { get; private set; }
        public bool IsCancelled { get; private set; }
        public List<MovieData> Movies { get; private set; }
        public List<ActressData> Actresses { get; private set; }
        public string ErrorLog { get { return m_errorLog; } }

        #endregion

        #region Public Functions

        public void Start(string scanDirectory)
        {
            m_directoriesToScan = new List<string>();
            m_directoriesToScan.Add(scanDirectory);
            Start();
        }

        public void Start(List<string> scanDirectories)
        {
            // Log operation
            Logger.WriteInfo("Start scanner");
            foreach (string scanDirectory in scanDirectories)
                Logger.WriteInfo("Scan directory: " + scanDirectory);

            m_directoriesToScan = scanDirectories;
            Start();
        }

        public void Start(List<ActressData> actresses)
        {
            Actresses = actresses;
            m_directoriesToScan = new List<string>();
            Start();
        }

        public void Cancel()
        {
            Logger.WriteInfo("Cancel scanner");
            IsCancelled = true;
        }

        public void Clear()
        {
            Logger.WriteInfo("Clear scanner");
            Movies.Clear();
            Actresses.Clear();
        }

        #endregion

        #region Private Functions

        public void Start()
        {
            // Log operation
            Logger.WriteInfo("Start scanner");

            foreach (string scanDirectory in m_directoriesToScan)
                Logger.WriteInfo("Scan directory: " + scanDirectory);

            foreach (ActressData actress in Actresses)
                Logger.WriteInfo("Update actress: " + actress.Name);

            // Branching behavior based on input data
            if (m_directoriesToScan.Count > 0)
            {
                Phase = ScanPhase.ScanningFolders;
                Actresses.Clear();
            }
            else
            {
                Phase = ScanPhase.DownloadActressData;
            }

            IsCancelled = false;
            m_hideMetadataAndCovers = Settings.Get().HideMetadataAndCovers;
            m_autoRestoreMetadata = Settings.Get().AutoRestoreMetadata;
            m_scanRecursively = Settings.Get().ScanRecursively;
            m_autoImportImprovedMovies = Settings.Get().AutoImportImprovedMovies;
            m_language = Settings.Get().Language;
            m_subtitleExts = Utilities.ProcessSettingsList(Settings.Get().SubtitleExts);
            m_coverNames = Utilities.ProcessSettingsList(Settings.Get().CoverNames);
            m_thumbnailNames = Utilities.ProcessSettingsList(Settings.Get().ThumbnailNames);
            m_movieExclusions = Utilities.ProcessSettingsList(Settings.Get().MovieExclusions);
            m_studioFilters = Settings.Get().Culture.StudioFilters.ToList();
            m_labelFilters = Settings.Get().Culture.LabelFilters.ToList();
            m_directorFilters = Settings.Get().Culture.DirectorFilters.ToList();
            m_genreFilters = Settings.Get().Culture.GenreFilters.ToList();
            m_generateLocalMetadata = Settings.Get().GenerateLocalMetadata;
            m_useFolderAsTitle = Settings.Get().UseFolderAsTitle;
            m_errorLog = String.Empty;
            m_moviesToProcess.Clear();
            Movies.Clear();
            ItemsProcessed = 0;
            TotalItems = 0;
            m_thread = new Thread(new ThreadStart(ThreadRun));
            m_thread.Start();
        }

        private void ThreadRun()
        {
            try
            {
                if (Phase == ScanPhase.ScanningFolders)
                {
                    m_filesInCache = m_movieCollection.GetAllFileNames();
                    foreach (var dir in m_directoriesToScan)
                        ProcessDirectory(dir);
                    ProcessMetadata();
                    ProcessActors();
                }
                else
                {
                    UpdateActresses();
                }
                Phase = ScanPhase.Finished;
                if (m_dispatcher.HasShutdownStarted == false)
                    m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanComplete?.Invoke(this, new EventArgs()); }));
                m_thread = null;
            }
            catch (Exception ex)
            {
                if (m_dispatcher.HasShutdownStarted == false)
                {
                    m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () 
                    {
                        MessageBox.Show(ex.ToString(), TextManager.GetString("Text.ErrorMovieScanner"));
                    }));
                }
            }
        }

        private void ProcessDirectory(string directoryToScan)
        {
            if (IsCancelled || m_dispatcher.HasShutdownStarted)
                return;

            // Check to see if the directory exists
            if (Directory.Exists(directoryToScan) == false)
                return;

            ItemsProcessed++;
            m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));

            Logger.WriteInfo(String.Format("Processing folder: {0}", directoryToScan));

            // Gather information about this scanned directory
            var directoryInfo = new DirectoryInfo();
            directoryInfo.Path = directoryToScan;
            directoryInfo.Folder = Path.GetFileName(directoryToScan);
            directoryInfo.ID = Utilities.ParseMovieID(directoryInfo.Folder);
            if (String.IsNullOrEmpty(directoryInfo.ID) == false)
                Logger.WriteInfo(String.Format("Detected ID {0} in folder name", directoryInfo.ID));

            // Check if folder has previously been scanned (i.e. is already in collection), and if so, whether it's a shared folder
            directoryInfo.ExistsInCollection = m_movieCollection.FolderInCollection(
                directoryInfo.ID, 
                directoryInfo.Path, 
                out directoryInfo.IsSharedFolder, 
                out directoryInfo.ID
                );

            try
            {
                string[] fileNames = Directory.GetFiles(directoryToScan);
                foreach (string fn in fileNames)
                {
                    // Cather relevant information about all files in this folder
                    var fileInfo = new FileInfo();
                    fileInfo.FileName = Path.GetFileName(fn);
                    fileInfo.FileType = GetFileType(fileInfo.FileName);

                    // Don't process unknown files
                    if (fileInfo.FileType == FileType.Unknown)
                        continue;

                    // Ignore filenames already in the cache
                    if (m_filesInCache.Contains(fn))
                        continue;

                    // Get file sub-types
                    if (fileInfo.FileType == FileType.Movie)
                        fileInfo.MovieType = GetMovieType(fileInfo.FileName);
                    else if (fileInfo.FileType == FileType.Image)
                        fileInfo.ImageType = GetImageType(fileInfo.FileName);

                    // Only extract ID info if the file is not an extra movie type
                    if (fileInfo.FileType != FileType.Movie || fileInfo.MovieType != MovieType.Extra)
                        fileInfo.ID = Utilities.ParseMovieID(fileInfo.FileName);

                    // Check to see if if this movie is already in the cache.  
                    if (String.IsNullOrEmpty(fileInfo.ID) == false && m_movieCollection.MovieExists(fileInfo.ID))
                    {
                        // Is this path the same as the existing movie?  If so, we can just skip it.
                        MovieData movieData = m_movieCollection.GetMovie(fileInfo.ID);
                        string path = Path.GetDirectoryName(fn);
                        if (String.Equals(path, movieData.Path, StringComparison.OrdinalIgnoreCase))
                            continue;
                        
                        // Does the movie actually exist?  The user may have moved it manually and not informed JavLuv.
                        // If it doesn't exist, we can remove it from the collection and allow the scan to proceed.
                        // We'll check for the existance of path and movie on disk. 
                        if (Directory.Exists(movieData.Path) == false || movieData.MovieFileNames.Count < 1 || 
                            File.Exists(Path.Combine(movieData.Path, movieData.MovieFileNames[0])) == false)
                        {
                            m_movieCollection.RemoveMovie(movieData);
                        }
                        else
                        {
                            // Mark this as a shared folder, since we may have extra files in it now.  Otherwise, if
                            // only one successful file remains, the directory could be inadvertently be moved instead
                            // of selected files copied.
                            directoryInfo.IsSharedFolder = true;

                            // Check to see if we want to import better movies, and if so, ignore the duplicate
                            if (m_autoImportImprovedMovies == false)
                            {
                                LogError(String.Format("Error scanning file {0}.  {1} already exists in collection.", fileInfo.FileName, fileInfo.ID), directoryToScan);
                                continue;
                            }
                        }
                    }

                    // Log ID
                    if (String.IsNullOrEmpty(fileInfo.ID) == false)
                        Logger.WriteInfo(String.Format("Detected ID {0} in file name {1}", fileInfo.ID, fileInfo.FileName));
                    else
                        Logger.WriteInfo(String.Format("No ID detected in file name {0}", fileInfo.FileName));

                    // Add this file to the directory info to analyze and scan
                    directoryInfo.Files.Add(fileInfo);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.WriteError("Issue when iterating over filenames during scanning", ex);
            }

            // Only process non-empty folders
            if (directoryInfo.Files.Count > 0)
            {
                // Fix some special-case IDs
                FixMultiPartD(directoryInfo);

                // Check to see if this folder has more than one movie in it
                directoryInfo.IsSharedFolder = IsSharedFolder(directoryInfo);

                // Finish processing directory
                ProcessDirectoryInfo(directoryInfo);
            }

            // Recursively process any subdirectories if required
            if (m_scanRecursively)
            {
                string[] directories = Directory.GetDirectories(directoryToScan);
                foreach (string directory in directories)
                    ProcessDirectory(directory);
            }
        }

        private void FixMultiPartD(DirectoryInfo directoryInfo)
        {
            // A file whose embedded ID ends in 'D' can either be a legit ID (in very rare cases)
            // or it can be the fourth in a series. Unfortunately, it can only be determine by
            // context - that is, the other files nearby with similar filenames and an identical root name.
            foreach (var fileInfo in directoryInfo.Files)
            {
                if (String.IsNullOrEmpty(fileInfo.ID) == true)
                    continue;
                if (fileInfo.ID.EndsWith("D"))
                {
                    int matchingCount = 0;
                    string ID = fileInfo.ID.Substring(0, fileInfo.ID.Length - 1);
                    foreach (var fi in directoryInfo.Files)
                    {
                        if (fi.ID == ID && fileInfo.FileType == fi.FileType)
                            matchingCount++;
                    }
                    if (matchingCount >= 3)
                    {
                        fileInfo.ID = ID;
                    }
                }
            }
        }

        private void ProcessDirectoryInfo(DirectoryInfo directoryInfo)
        {
            if (directoryInfo.IsSharedFolder)
            {
                var movies = new HashSet<MovieData>();

                // Place all files in their appropriate slots
                foreach (var fileInfo in directoryInfo.Files)
                {
                    // In a shared folder, all files must have an associated ID to be processed
                    if (String.IsNullOrEmpty(fileInfo.ID))
                    {
                        Logger.WriteInfo(String.Format("No ID detected, so skipping file {0}", fileInfo.FileName));
                        continue;
                    }

                    MovieData id = new MovieData();
                    id.Metadata.UniqueID.Value = fileInfo.ID;
                    MovieData movieData;
                    if (movies.TryGetValue(id, out movieData) == false)
                    {
                        // Create movie data
                        movieData = CreateMovieData(directoryInfo);
                        movieData.Metadata.UniqueID.Value = fileInfo.ID;
                        movies.Add(movieData);
                    }
                    AddFileToMovieData(movieData, fileInfo);
                }

                // Add all movies to the processing list
                foreach (var movie in movies)
                {
                    m_moviesToProcess.Add(movie);
                }
            }
            else
            {
                // Check to see if we have existing movie data
                MovieData movieData = null;
                if (directoryInfo.ExistsInCollection)
                {
                    MovieData key = new MovieData();
                    key.Metadata.UniqueID.Value = directoryInfo.ID;
                    movieData = m_movieCollection.GetMovie(directoryInfo.ID);
                    if (movieData == null)
                    {
                        Logger.WriteWarning(String.Format("Folder {0} reported it was in collection, but now can't be found.", directoryInfo.Path));
                        directoryInfo.ExistsInCollection = false;
                    }
                }

                // If not, we create new movie data now, and assign an ID
                if (movieData == null)
                {
                    movieData = CreateMovieData(directoryInfo);

                    // Find the most likely ID for an exclusive folder
                    foreach (var fileInfo in directoryInfo.Files)
                    {
                        if (String.IsNullOrEmpty(fileInfo.ID) == false && fileInfo.FileType == FileType.Movie)
                        {
                            if (fileInfo.ID != movieData.Metadata.UniqueID.Value)
                            {
                                movieData.Metadata.UniqueID.Value = fileInfo.ID;
                                break;
                            }
                        }
                    }
                }

                // Place all files in their appropriate slots
                foreach (var fileInfo in directoryInfo.Files)
                {
                    AddFileToMovieData(movieData, fileInfo);
                }

                // Add movie to the processing list if it's new
                if (directoryInfo.ExistsInCollection == false)
                    m_moviesToProcess.Add(movieData);
            }
        }

        private MovieData CreateMovieData(DirectoryInfo directoryInfo)
        {
            MovieData movieData = new MovieData();
            movieData.Metadata.UniqueID.Value = directoryInfo.ID;
            movieData.SharedPath = directoryInfo.IsSharedFolder;
            movieData.Path = directoryInfo.Path;
            movieData.Folder = directoryInfo.Folder;
            return movieData;
        }

        private void AddFileToMovieData(MovieData movieData, FileInfo fileInfo)
        {
            if (fileInfo.FileType == FileType.Movie)
            {
                if (fileInfo.MovieType == MovieType.Feature)
                    movieData.MovieFileNames.Add(fileInfo.FileName);
                else if (fileInfo.MovieType == MovieType.Extra)
                    movieData.ExtraMovieFileNames.Add(fileInfo.FileName);
            }
            else if (fileInfo.FileType == FileType.Image)
            {
                if (fileInfo.ImageType == ImageType.Cover)
                    movieData.CoverFileName = fileInfo.FileName;
                else if (fileInfo.ImageType == ImageType.Thumbnails)
                    movieData.ThumbnailsFileNames.Add(fileInfo.FileName);
                else if (String.IsNullOrEmpty(movieData.CoverFileName))
                    movieData.CoverFileName = fileInfo.FileName;
            }
            else if (fileInfo.FileType == FileType.Metadata)
            {
                movieData.MetadataFileName = fileInfo.FileName;
            }
            else if (fileInfo.FileType == FileType.Subtitle)
            {
                if (String.IsNullOrEmpty(fileInfo.ID) == false)
                    movieData.SubtitleFileNames.Add(fileInfo.FileName);
            }
        }

        private bool IsSharedFolder(DirectoryInfo directoryInfo)
        {
            // Don't bother checking if we've marked this as a shared folder previously.
            if (directoryInfo.IsSharedFolder)
                return true;

            string currentID = String.Empty;

            // Check to see if we have multiple IDs
            foreach (var fileInfo in directoryInfo.Files)
            {
                if (fileInfo.FileType != FileType.Movie)
                    continue;
                if (fileInfo.MovieType == MovieType.Extra)
                    continue;

                // Check to see if the file has an embedded ID in the name and we have a current ID
                if (String.IsNullOrEmpty(fileInfo.ID) == false && String.IsNullOrEmpty(currentID) == false)
                {
                    if (fileInfo.ID != currentID)
                    {
                        return true;
                    }
                }
                currentID = fileInfo.ID;
            }
            return false;
        }

        private void ProcessMetadata()
        {
            if (IsCancelled || m_dispatcher.HasShutdownStarted)
                return;

            List<MovieData> moviesToLoad = new List<MovieData>();
            List<MovieData> moviesToImport = new List<MovieData>();
            List<MovieData> moviesToDownload = new List<MovieData>();

            // Split movies into three lists
            foreach (MovieData movieData in m_moviesToProcess)
            {
                if (m_movieCollection.MovieExists(movieData.Metadata.UniqueID.Value))
                    moviesToImport.Add(movieData);
                else if (String.IsNullOrEmpty(movieData.MetadataFileName))
                    moviesToDownload.Add(movieData);
                else
                    moviesToLoad.Add(movieData);
            }

            // Load all movies we already have on disk
            LoadMovies(moviesToLoad);

            // Import all movies with similar or better quality
            ImportMovies(moviesToImport);

            // Download metadata as necessary
            DownloadMovieMetadata(moviesToDownload);

            // Remove any movies that don't have metadata or a valid UniqueID
            Movies.RemoveAll(m => m.Metadata == null || String.IsNullOrEmpty(m.MetadataFileName) || String.IsNullOrEmpty(m.Metadata.UniqueID.Value));
        }

        private void LoadMovies(List<MovieData> moviesToLoad)
        {
            Phase = ScanPhase.LoadingMetadata;
            ItemsProcessed = 0;
            TotalItems = moviesToLoad.Count;
            if (m_dispatcher.HasShutdownStarted == false)
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));

            foreach (MovieData movieData in moviesToLoad)
            {
                if (IsCancelled || m_dispatcher.HasShutdownStarted)
                    break;

                string fn = Path.Combine(movieData.Path, movieData.MetadataFileName);
                try
                {
                    movieData.Metadata = MovieSerializer<MovieMetadata>.Load(fn);

                    // Javinizer saves DVD-ID to the ID field instead of the UniqueID field, so we check that here.
                    if (String.IsNullOrEmpty(movieData.Metadata.UniqueID.Value) && String.IsNullOrEmpty(movieData.Metadata.ID) == false)
                        movieData.Metadata.UniqueID.Value = movieData.Metadata.ID;
                    Movies.Add(movieData);
                }
                catch (Exception ex)
                {
                    LogError("Unable to load metadata", movieData.Path, ex);
                }

                ItemsProcessed++;
                if (m_dispatcher.HasShutdownStarted == false)
                    m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));
            }
        }

        private void ImportMovies(List<MovieData> moviesToImport)
        {
            Phase = ScanPhase.ImportMovies;
            ItemsProcessed = 0;
            TotalItems = moviesToImport.Count;
            if (m_dispatcher.HasShutdownStarted == false)
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));

            foreach (MovieData movieData in moviesToImport)
            {
                if (IsCancelled || m_dispatcher.HasShutdownStarted)
                    break;

                // Validate that we're copying a movie that's as good or better resolution than destination, else it's an error to report
                string sourceResolution = MovieUtils.GetMovieResolution(Path.Combine(movieData.Path, movieData.MovieFileNames[0]));
                int sourceWidth, sourceHeight;
                MovieUtils.ParseMovieResolution(sourceResolution, out sourceWidth, out sourceHeight);
                if (sourceWidth == 0 || sourceHeight == 0)
                {
                    LogError(TextManager.GetString("Text.ErrorImportingMovie") + " " + movieData.MovieFileNames[0], movieData.Path);
                    continue;
                }
                var dest = m_movieCollection.GetMovie(movieData.Metadata.UniqueID.Value);
                if (dest == null)
                {
                    LogError(TextManager.GetString("Text.ErrorImportingMovie") + " " + movieData.MovieFileNames[0], movieData.Path);
                    continue;
                }
                int destWidth, destHeight;
                MovieUtils.GetMovieResolution(dest.Metadata, out destWidth, out destHeight);
                if (destWidth == 0 || destHeight == 0)
                {
                    LogError(TextManager.GetString("Text.ErrorImportingMovie") + " " + movieData.MovieFileNames[0], movieData.Path);
                    continue;
                }
                if (sourceWidth < destWidth || sourceHeight < destHeight)
                {
                    LogError(TextManager.GetString("Text.ErrorImportingMovie") + " " + movieData.MovieFileNames[0], movieData.Path);
                    continue;
                }

                // Confirm we wish to continue and overwrite the old movies with new ones
                if (m_dispatcher.HasShutdownStarted == true)
                    break;
                bool confirmReplace = false;
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () 
                {
                    var filenames = new List<string>();
                    foreach (var fn in dest.MovieFileNames)
                        filenames.Add(Path.Combine(dest.Path, fn));
                    string destFilenames = Utilities.StringListToString(filenames);
                    filenames.Clear();
                    foreach (var fn in movieData.MovieFileNames)
                        filenames.Add(Path.Combine(movieData.Path, fn));
                    string sourceFilenames = Utilities.StringListToString(filenames);
                    string destResolution = MovieUtils.GetMovieResolution(dest.Metadata);
                    string message = String.Format(TextManager.GetString("Text.OverrideMovie"), destResolution, destFilenames, sourceResolution, sourceFilenames);
                    string caption = TextManager.GetString("Text.OverrideMovieTitle");
                    MessageBoxResult result = MessageBox.Show(message, caption, MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                        confirmReplace = true;
                }));

                // Don't replace unless we've verified with the user we want to replace the source file(s)
                if (confirmReplace == false)
                    continue;

                // Set new movie resolution
                MovieUtils.SetMovieResolution(dest, sourceWidth, sourceHeight);
                dest.MovieResolution = MovieUtils.GetMovieResolution(dest.Metadata);

                // Get root movie name
                string rootMovieName = GetCommonMovieFilename(dest);

                // Delete the destination movies
                foreach (string fn in dest.MovieFileNames)
                {
                    string fnPath = Path.Combine(dest.Path, fn);
                    Utilities.DeleteFile(fnPath);
                }

                // Clear the destination file list
                dest.MovieFileNames.Clear();

                // Move the new movie files to the destination path
                for (int i = 0; i < movieData.MovieFileNames.Count; ++i)
                {
                    string sourceFn = movieData.MovieFileNames[i];
                    string sourcePath = Path.Combine(movieData.Path, sourceFn);
                    string destFileName = Path.GetFileNameWithoutExtension(rootMovieName);
                    if (movieData.MovieFileNames.Count > 1)
                        destFileName = rootMovieName + "-" + ((char)((int)'A' + i)).ToString();
                    destFileName = Path.ChangeExtension(destFileName, Path.GetExtension(movieData.MovieFileNames[i]));
                    string destPath = Path.Combine(dest.Path, destFileName);
                    Utilities.MoveFile(sourcePath, destPath);
                    dest.MovieFileNames.Add(destFileName);
                }
                
                // Add to list of movies processed
                Movies.Add(dest);

                ItemsProcessed++;
                if (m_dispatcher.HasShutdownStarted == false)
                    m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));
            }
        }

        private void DownloadMovieMetadata(List<MovieData> moviesToDownload)
        {
            Phase = ScanPhase.DownloadMetadata;
            ItemsProcessed = 0;
            TotalItems = moviesToDownload.Count;
            if (m_dispatcher.HasShutdownStarted == false)
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));

            foreach (MovieData movieData in moviesToDownload)
            {
                if (IsCancelled || m_dispatcher.HasShutdownStarted)
                    break;

                // If we don't have metadata, generate it now from the movie ID
                GenerateMetaData(movieData);

                // Add to list of movies processed
                Movies.Add(movieData);

                ItemsProcessed++;
                if (m_dispatcher.HasShutdownStarted == false)
                    m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));
            }
        }

        private void ProcessActors()
        {
            var actorsToProcess = new List<ActorData>();
            foreach (MovieData movie in Movies)
            {
                if (IsCancelled)
                    break;
                foreach (ActorData actor in movie.Metadata.Actors)
                {
                    if (IsCancelled)
                        break;
                    if (m_movieCollection.ActressExists(actor.Name))
                        continue;
                    bool foundAlias = false;
                    foreach (var alias in actor.Aliases)
                    {
                        if (m_movieCollection.ActressExists(actor.Name))
                        {
                            foundAlias = true;
                            break;
                        }
                    }
                    if (foundAlias)
                        continue;
                    actorsToProcess.Add(actor);
                }
            }

            TotalItems = actorsToProcess.Count;
            ItemsProcessed = 0;
            Phase = ScanPhase.DownloadActressData;
            if (m_dispatcher.HasShutdownStarted == false)
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));

            // Download all actresses
            foreach (var actor in actorsToProcess)
            {
                if (IsCancelled)
                    break;

                var scraper = new Scraper();
                var actressData = scraper.ScrapeActress(actor, m_language);

                // Check to see if we want to restore from backup instead of using scraped metadata
                if (m_autoRestoreMetadata)
                {
                    var backupActressData = m_movieCollection.GetBackupActress(actressData.Name);
                    if (backupActressData == null)
                    {
                        foreach (var altName in actressData.AltNames)
                        {
                            backupActressData = m_movieCollection.GetBackupActress(altName);
                            if (backupActressData != null)
                                break;
                        }
                    }
                    if (backupActressData == null && String.IsNullOrEmpty(actressData.JapaneseName) == false)
                        backupActressData = m_movieCollection.GetBackupActress(actressData.JapaneseName);
                    
                    if (backupActressData != null)
                    {
                        backupActressData.ImageFileNames.Concat(actressData.ImageFileNames);
                        actressData = backupActressData;
                        if (actressData.ImageFileNames.Count > 0)
                        {
                            Utilities.DeleteDuplicateFiles(Utilities.GetActressImageFolder(), actressData.ImageFileNames);
                            actressData.ImageIndex = Math.Min(actressData.ImageIndex, actressData.ImageFileNames.Count - 1);
                        }            
                    }
                }

                // Add actress data to the processed list
                if (actressData != null)
                    Actresses.Add(actressData);

                ItemsProcessed++;
                if (m_dispatcher.HasShutdownStarted == false)
                    m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));
            }
        }

        private void UpdateActresses()
        {
            // Prepare for update
            TotalItems = Actresses.Count;
            ItemsProcessed = 0;
            Phase = ScanPhase.DownloadActressData;
            if (m_dispatcher.HasShutdownStarted == false)
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));

            // Update each actress in turn
            foreach (var actress in Actresses)
            {
                if (IsCancelled)
                    break;
                var scraper = new Scraper();
                scraper.ScrapeActress(actress, m_language);

                ItemsProcessed++;
                if (m_dispatcher.HasShutdownStarted == false)
                    m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { ScanUpdate?.Invoke(this, new EventArgs()); }));
            }
        }

        private FileType GetFileType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            if (String.IsNullOrEmpty(ext))
                return FileType.Unknown;
            ext = ext.Split('.')[1];
            if (Utilities.GetMovieFileExts().Contains(ext))
                return FileType.Movie;
            if (Utilities.GetImageFileExts().Contains(ext))
                return FileType.Image;
            if (m_subtitleExts.Contains(ext))
                return FileType.Subtitle;
            if (ext == "nfo")
                return FileType.Metadata;
            return FileType.Unknown;
        }

        private ImageType GetImageType(string fileName)
        {
            if (Utilities.ContainsCaseless(fileName, m_coverNames))
                return ImageType.Cover;
            if (Utilities.ContainsCaseless(fileName, m_thumbnailNames))
                return ImageType.Thumbnails;
            return ImageType.Unknown;
        }

        private MovieType GetMovieType(string fileName)
        {
            if (Utilities.ContainsCaseless(fileName, m_movieExclusions))
                return MovieType.Extra;
            return MovieType.Feature;
        }

        private async Task<Tuple<int, int>> GetMovieResolutionAsync(string fileName)
        {
            int width = 0; int height = 0;
            await Task.Run(() => 
            {
                string resStr = MovieUtils.GetMovieResolution(fileName);
                MovieUtils.ParseMovieResolution(resStr, out width, out height);
            });
            return new Tuple<int, int>(width, height);
        }

        private async void GenerateMetaData(MovieData movieData)
        {
            if (IsCancelled || m_dispatcher.HasShutdownStarted)
                return;

            if (movieData.MovieFileNames.Count == 0)
            {
                Logger.WriteWarning("No movies found in " + movieData.Path);
                return;
            }

            string movieID = movieData.Metadata.UniqueID.Value;
            string movieFileName = GetCommonMovieFilename(movieData);

            // We can't generate metadata without a valid movie filename or movieID
            if (String.IsNullOrEmpty(movieFileName) || String.IsNullOrEmpty(movieID))
            {
                LogError("No valid movieID or movie file found", movieData.Path);
                return;
            }

            // Scrape metadata from websites
            Scraper scraper = new Scraper();
            MovieMetadata metadata = null;
            try
            {
                // Begin retrieving the movie resolution asyncronously while the metadata is being scraped.
                var resolutionTask = GetMovieResolutionAsync(Path.Combine(movieData.Path, movieData.MovieFileNames[0]));

                // Check to see if we need to download a cover image
                string coverImagePath = String.Empty;
                if (String.IsNullOrEmpty(movieData.CoverFileName))
                {
                    coverImagePath = Path.Combine(movieData.Path, Path.GetFileNameWithoutExtension(movieFileName));
                }

                // Scrape metadata and optionally download cover image as well
                metadata = scraper.ScrapeMovie(movieID, ref coverImagePath, m_language);

                if (metadata != null)
                {
                    // Clean up metadata
                    MovieUtils.FilterMetadata(metadata, m_studioFilters, m_labelFilters, m_directorFilters, m_genreFilters);

                    // Check to see if we've successfully downloaded a cover file, and if so, set that value
                    if (String.IsNullOrEmpty(movieData.CoverFileName) && String.IsNullOrEmpty(coverImagePath) == false && File.Exists(coverImagePath))
                        movieData.CoverFileName = Path.GetFileName(coverImagePath);
                }
                else
                {
                    // check to see if we want to generate metadata anyway
                    if (m_generateLocalMetadata)
                    {
                        metadata = new MovieMetadata();
                        metadata.UniqueID.Value = movieID;
                        if (m_useFolderAsTitle)
                        {
                            metadata.Title = movieData.Folder;
                        }
                        else
                        {
                            if (movieData.MovieFileNames.Count == 0)
                            {
                                LogError("Could not generate local metadata - no movie found", movieData.Path);
                                return;
                            }
                            metadata.Title = Utilities.GetCommonFileName(movieData.MovieFileNames);
                        }              
                    }
                    else 
                    { 
                        movieData.Metadata = null;
                        LogError(String.Format("Unable to find online metadata for {0}", movieID), movieData.Path);
                        return;
                    }
                }

                if (metadata != null)
                {
                    // Fill in the 'date added' field
                    var dt = DateTime.Now;
                    metadata.DateAdded = Utilities.DateTimeToString(dt.Year, dt.Month, dt.Day);
                }

                // Check to see if we instead want to restore from backup instead of using scraped metadata
                if (m_autoRestoreMetadata)
                {
                    var newMetaData = m_movieCollection.GetBackupMetadata(movieData.Metadata.UniqueID.Value);
                    if (newMetaData != null)
                        metadata = newMetaData;
                }    

                // Save new metadata file
                string filename = Path.Combine(movieData.Path, Path.ChangeExtension(movieFileName, "nfo"));
                MovieSerializer<MovieMetadata>.Save(filename, metadata);

                // Store metadata with movie
                movieData.Metadata = metadata;

                // Wait for this task to be finished, and set resolution data for movie
                Tuple<int, int> resResult = await resolutionTask;
                MovieUtils.SetMovieResolution(movieData, resResult.Item1, resResult.Item2);

                // Save metadata filename
                movieData.MetadataFileName = Path.GetFileName(filename);
            }
            catch (Exception ex)
            {
                LogError("Unexpected error when generating metadata", movieData.Path, ex);
            }
        }

        private string GetCommonMovieFilename(MovieData movieData)
        {
            string movieFileName = movieData.MovieFileNames[0];

            // Get common prefix if there are multiple movies
            if (movieData.MovieFileNames.Count > 1)
            {
                movieFileName = Utilities.GetCommonFileName(movieData.MovieFileNames);

                // If the movies have nothing in common, pick the one that has the embedded ID in it
                if (String.IsNullOrEmpty(movieFileName))
                {
                    foreach (string movie in movieData.MovieFileNames)
                    {
                        if (Utilities.ParseMovieID(movie) == movieData.Metadata.UniqueID.Value)
                        {
                            movieFileName = movie;
                            break;
                        }
                    }
                }

                // If all else fails, we just pick the first movie to use
                if (String.IsNullOrEmpty(movieFileName))
                    movieFileName = movieData.MovieFileNames[0];
            }
            return movieFileName;
        }

        private void LogError(string errorMsg, string directory, Exception ex = null)
        {
            Logger.WriteError(errorMsg + " directory: " + directory, ex);

            var msg = new StringBuilder();
            msg.Capacity = 1024;
            msg.Append(errorMsg);
            msg.Append("\n");
            if (ex != null)
            {
                msg.Append("Exception: ");
                msg.Append(ex.Message);
                msg.Append("\n");
            }
            msg.Append("Folder: ");
            msg.Append(directory);
            msg.Append("\n");

            msg.Append("Files:");
            string[] fileNames = Directory.GetFiles(directory);
            int count = 0;
            foreach (string fn in fileNames)
            {
                count++;
                if (count >= 10)
                {
                    msg.Append(" + " + (fileNames.Length - 10).ToString() + " more...");
                    break;
                }
                msg.Append(" ");
                msg.Append(Path.GetFileName(fn));
            }
            msg.Append("\n\n");
            m_errorLog += msg.ToString();
        }

        #endregion

        #region Private Members

        private enum FileType
        {
            Movie,
            Image,
            Metadata,
            Subtitle,
            Unknown,
        }

        private enum MovieType
        {
            Feature,
            Extra,
            Unknown,
        }

        private enum ImageType
        {
            Cover,
            Thumbnails,
            Unknown,
        }

        private class FileInfo
        {
            public string FileName = String.Empty;
            public string ID = String.Empty;
            public FileType FileType = FileType.Unknown;
            public MovieType MovieType = MovieType.Unknown;
            public ImageType ImageType = ImageType.Unknown;
        }

        private class DirectoryInfo
        {
            public string Path = String.Empty;
            public string Folder = String.Empty;
            public string ID = String.Empty;
            public bool ExistsInCollection = false;
            public bool IsSharedFolder = false;
            public List<FileInfo> Files = new List<FileInfo>();
        }

        private Dispatcher m_dispatcher;
        private MovieCollection m_movieCollection;
        private List<string> m_directoriesToScan;
        private Thread m_thread;
        private string[] m_subtitleExts;
        private string[] m_coverNames;
        private string[] m_thumbnailNames;
        private string[] m_movieExclusions;
        private List<FilterPair> m_studioFilters;
        private List<FilterPair> m_labelFilters;
        private List<FilterPair> m_directorFilters;
        private List<FilterPair> m_genreFilters;
        private bool m_generateLocalMetadata;
        private bool m_useFolderAsTitle;
        private HashSet<string> m_filesInCache;
        private List<MovieData> m_moviesToProcess = new List<MovieData>();
        private HashSet<MovieMetadata> m_backupMetadata = new HashSet<MovieMetadata>();
        private string m_settingsFolder = String.Empty;
        private LanguageType m_language;
        private bool m_scanRecursively = true;
        private bool m_autoImportImprovedMovies = true;
        private bool m_hideMetadataAndCovers = false;
        private bool m_autoRestoreMetadata = false;
        private string m_errorLog = String.Empty;

        #endregion
    }
}
