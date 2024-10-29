using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace MovieInfo
{
    public class MovieCollection
    {
        #region Constructors

        public MovieCollection(Dispatcher dispatcher, bool readOnlyMode, bool useJapaneseNameOrder)
        {
            m_dispatcher = dispatcher;
            m_readOnlyMode = readOnlyMode;
            m_useJapaneseNameOrder = useJapaneseNameOrder;
            CommandQueue.Command().CommandFinished += OnCommandFinished;
            if (m_readOnlyMode == false)
            {
                // Timer fires off every sixty seconds
                m_timer = new System.Timers.Timer(60000);
                m_timer.Start();
                m_timer.Elapsed += OnTimerElapsed;
            }
            var folder = Utilities.GetJavLuvSettingsFolder();
            m_cacheFilename = Path.Combine(folder, "JavLuv.cache");
            m_actressesFilename = Path.Combine(folder, "Actresses.xml");
            string oldBackupFileName = Path.Combine(folder, "Metadata.backup");
            m_backupFilename = Path.Combine(folder, "JavLuv.backup");
            if (File.Exists(oldBackupFileName))
            {
                if (File.Exists(m_backupFilename) == false)
                    File.Move(oldBackupFileName, m_backupFilename);
                else
                    File.Delete(oldBackupFileName);
            }
            if (File.Exists(m_cacheFilename))
                CommandQueue.Command().Execute(new CmdLoad(ref m_cacheData, m_cacheFilename, ref m_actressesDatabase, m_actressesFilename, ref m_backupData, m_backupFilename));
            else
                m_loaded = true;
        }

        #endregion

        #region Events

        public event EventHandler MoviesDisplayedChanged;
        public event EventHandler ActressesDisplayedChanged;

        #endregion

        #region Event Handlers

        private void OnCommandFinished(object sender, CommandEventArgs e)
        {
            if (e.CommandName == "MovieInfo.CmdLoad")
            {
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()          
                { 
                    NotifyMoviesDisplayedChanged();
                    NotifyActressesDisplayedChanged();
                    m_loaded = true;
                    SearchMovies();
                    SearchActresses();
                }));     
            }
            else if (e.CommandName == "MovieInfo.CmdSearchMovies")
            {
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    if (m_searchMovies != null)
                    {
                        m_moviesDisplayed = m_searchMovies.FilteredMovies;
                        NotifyMoviesDisplayedChanged();
                    }
                }));
            }
            else if (e.CommandName == "MovieInfo.CmdSearchActresses")
            {
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    if (m_searchActresses != null)
                    {
                        m_actressesDisplayed = m_searchActresses.FilteredActresses;
                        NotifyActressesDisplayedChanged();
                    }
                }));
            }
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CommandQueue.Command().Execute(new CmdUpdateResolution(m_cacheData));
            CommandQueue.Command().Execute(new CmdSaveMetadata(m_cacheData));
        }

        #endregion

        #region Properties

        public List<MovieData> MoviesDisplayed { get { return m_moviesDisplayed; } }
        public List<ActressData> ActressesDisplayed { get { return m_actressesDisplayed; } }

        public int AverageMovieRating { get; set; }

        public bool AutoSyncActresses { private get; set; }

        public string SearchText
        {
            private get { return m_searchText; }
            set 
            { 
                m_searchText = value;
                SearchMovies();
                SearchActresses();
            }
        }

        public SortMoviesBy SortMoviesBy
        {
            private get { return m_sortMoviesBy; }
            set
            {
                if (value != m_sortMoviesBy)
                {
                    RandomSeed = m_rng.Next();
                    m_sortMoviesBy = value;          
                    SearchMovies();
                }
            }
        }

        public SortActressesBy SortActressesBy
        {
            private get { return m_sortActressesBy; }
            set
            {
                if (value != m_sortActressesBy)
                {
                    m_sortActressesBy = value;
                    SearchActresses();
                }
            }
        }

        public bool UseJapaneseNameOrder
        {
            private get { return m_useJapaneseNameOrder; }
            set
            {
                if (value != m_useJapaneseNameOrder)
                {
                    m_useJapaneseNameOrder = value;
                    SearchActresses();
                }
            }
        }


        public bool ShowID
        {
            private get
            {
                return m_showID;
            }
            set
            {
                if (value != m_showID)
                {
                    m_showID = value;
                    SearchMovies();
                }
            }
        }

        public bool ShowUserRating
        {
            private get
            {
                return m_showID;
            }
            set
            {
                if (value != m_showID)
                {
                    m_showID = value;
                    SearchMovies();
                }
            }
        }

        public bool ShowUnratedOnly 
        {
            private get
            {
                return m_showUnratedOnly;
            }
            set
            {
                if (value != m_showUnratedOnly)
                {
                    m_showUnratedOnly = value;
                    SearchMovies();
                }
            }
        }

        public bool ShowSubtitlesOnly
        {
            private get
            {
                return m_showSubtitlesOnly;
            }
            set
            {
                if (value != m_showSubtitlesOnly)
                {
                    m_showSubtitlesOnly = value;
                    SearchMovies();
                }
            }
        }

        public bool ShowAllActresses
        {
            private get
            {
                return m_showUnknownActresses;
            }
            set
            {
                if (value != m_showUnknownActresses)
                {
                    m_showUnknownActresses = value;
                    SearchActresses();
                }
            }
        }

        public int NumMovies
        {
            get { return m_cacheData.Movies.Count; }
        }

        public int NumActresses
        {
            get { return m_actressesDatabase.Actresses.Count; }
        }

        public ActressData MovieSearchActress { get; set; }

        public int RandomSeed { get; private set; }

        #endregion

        #region Public Functions

        public void AddMovie(MovieData movie)
        {
            var err = new StringBuilder(1024);
            lock (m_cacheData)
            {
                m_cacheData.Movies.Add(movie);
            }
            SearchMovies();
            SearchActresses();
            Save();
        }

        public void AddMovies(List<MovieData> movies)
        {
            var err = new StringBuilder(1024);
            lock (m_cacheData)
            {
                foreach (var movie in movies)
                {
                    m_cacheData.Movies.Add(movie);
                }
            }
            SearchMovies();
            SearchActresses();
            Save();
        }

        public void AddActress(ActressData actress)
        {
            lock (m_actressesDatabase)
            {
                AddActressNoLock(actress);
            }
            UpdateActressNames();
            SearchActresses();
            Save();
        }

        public void AddActresses(List<ActressData> actresses)
        {
            lock (m_actressesDatabase)
            {
                foreach (var actress in actresses)
                    AddActressNoLock(actress);
            }
            UpdateActressNames();
            SearchActresses();
            Save();
        }

        public MovieData GetMovie(string uniqueID)
        {
            lock (m_cacheData)
            {
                MovieData key = new MovieData();
                key.Metadata.UniqueID.Value = uniqueID;
                MovieData result = null;
                m_cacheData.Movies.TryGetValue(key, out result);
                return result;
            }
        }

        public bool MovieExists(string uniqueID)
        {
            lock (m_cacheData)
            {
                MovieData key = new MovieData();
                key.Metadata.UniqueID.Value = uniqueID;
                return m_cacheData.Movies.Contains(key);
            }
        }

        public bool ActressExists(string name)
        {
            lock (m_actressesDatabase)
            {
                if (m_actressesDatabase.Actresses.Contains(new ActressData(name)))
                    return true;
                if (m_actressesDatabase.JapaneseNames.Contains(new NamePair(name)))
                    return true;
                if (m_actressesDatabase.AltNames.Contains(new NamePair(name)))
                    return true;
                return false;
            }
        }

        public ActressData FindActress(string name)
        {
            lock (m_actressesDatabase)
            {
                ActressData actress = null;
                if (m_actressesDatabase.Actresses.TryGetValue(new ActressData(name), out actress))
                    return actress;
                NamePair altName = null;
                if (m_actressesDatabase.JapaneseNames.TryGetValue(new NamePair(name), out altName))
                {
                    if (m_actressesDatabase.Actresses.TryGetValue(new ActressData(altName.Name), out actress))
                        return actress;
                }
                if (m_actressesDatabase.AltNames.TryGetValue(new NamePair(name), out altName))
                {
                    if (m_actressesDatabase.Actresses.TryGetValue(new ActressData(altName.Name), out actress))
                        return actress;
                }
                return actress;
            }
        }

        public HashSet<string> GetAllFileNames()
        {
            lock (m_cacheData)
            {
                HashSet<string> fileNames = new HashSet<string>();
                foreach (MovieData movieData in m_cacheData.Movies)
                {
                    foreach (string fn in movieData.MovieFileNames)
                        fileNames.Add(Path.Combine(movieData.Path, fn));
                    foreach (string fn in movieData.ThumbnailsFileNames)
                        fileNames.Add(Path.Combine(movieData.Path, fn));
                    foreach (string fn in movieData.SubtitleFileNames)
                        fileNames.Add(Path.Combine(movieData.Path, fn));
                    foreach (string fn in movieData.ExtraMovieFileNames)
                        fileNames.Add(Path.Combine(movieData.Path, fn));
                    if (String.IsNullOrEmpty(movieData.CoverFileName) == false)
                        fileNames.Add(Path.Combine(movieData.Path, movieData.CoverFileName));
                    if (String.IsNullOrEmpty(movieData.MetadataFileName) == false)
                        fileNames.Add(Path.Combine(movieData.Path, movieData.MetadataFileName));
                }

                return fileNames;
            }
        }

        public bool FolderInCollection(string uniqueID, string folderName, out bool sharedPath, out string foundID)
        {
            sharedPath = false;
            foundID = uniqueID;
            lock (m_cacheData)
            {
                // First check specific ID if applicable
                if (String.IsNullOrEmpty(uniqueID) == false)
                {
                    MovieData key = new MovieData();
                    key.Metadata.UniqueID.Value = uniqueID;
                    MovieData result = new MovieData();
                    if (m_cacheData.Movies.TryGetValue(key, out result))
                    {
                        if (result.Path == folderName)
                        {
                            sharedPath = result.SharedPath;
                            if (sharedPath == false)
                                foundID = result.Metadata.UniqueID.Value;
                            return true;
                        }
                    }
                }

                // Fall back to checking all folders
                foreach (MovieData movieData in m_cacheData.Movies)
                {
                    if (movieData.Path == folderName)
                    {
                        sharedPath = movieData.SharedPath;
                        if (sharedPath == false)
                            foundID = movieData.Metadata.UniqueID.Value;
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsFolderSharedPath(string folderName)
        {
            lock (m_cacheData)
            {
                foreach (MovieData movieData in m_cacheData.Movies)
                {
                    if (movieData.Path == folderName && movieData.SharedPath)
                        return true;
                }
                return false;
            }
        }

        public MovieMetadata GetBackupMetadata(string uniqueID)
        {
            lock (m_backupData)
            {
                MovieMetadata value;
                if (m_backupData.Movies.TryGetValue(new MovieMetadata(uniqueID), out value))
                    return value;
            }
            return null;
        }

        public ActressData GetBackupActress(string actressName)
        {
            lock (m_backupData)
            {
                ActressData value;
                if (m_backupData.Actresses.TryGetValue(new ActressData(actressName), out value))
                    return value;
                NamePair altNameData;
                if (m_backupData.JapaneseNames.TryGetValue(new NamePair(actressName), out altNameData))
                {
                    if (m_backupData.Actresses.TryGetValue(new ActressData(altNameData.Name), out value))
                        return value;
                }
                if (m_backupData.AltNames.TryGetValue(new NamePair(actressName), out altNameData))
                {
                    if (m_backupData.Actresses.TryGetValue(new ActressData(altNameData.Name), out value))
                        return value;
                }
            }
            return null;
        }

        public void RemoveMovie(MovieData movie)
        {
            lock (m_cacheData)
            {
                if (m_cacheData.Movies.Contains(movie))
                    m_cacheData.Movies.Remove(movie);           
            }
            SearchMovies();
            Save();
        }

        public void RemoveMovies(List<MovieData> movies)
        {
            lock (m_cacheData)
            {
                foreach (var movie in movies)
                {
                    if (m_cacheData.Movies.Contains(movie))
                        m_cacheData.Movies.Remove(movie);
                }
            }
            SearchMovies();
            Save();
        }

        public void RemoveActress(ActressData actress, bool deleteImages)
        {
            RemoveActressNoLock(actress, deleteImages);
            SearchActresses();
            Save();
        }

        public void RemoveActresses(List<ActressData> actresses)
        {
            lock (m_actressesDatabase)
            {
                foreach (var actress in actresses)
                    RemoveActressNoLock(actress, true);
            }
            SearchActresses();
            Save();
        }

        public void FilterMetadata(List<MovieData> movies, List<FilterPair> studioFilter, List<FilterPair> labelFilter,
            List<FilterPair> directorFilter, List<FilterPair> categoryFilter)
        {
            var studios = new List<FilterPair>();
            foreach (var pair in studioFilter)
                studios.Add(new FilterPair(pair.Original, pair.Filtered));
            var labels = new List<FilterPair>();
            foreach (var pair in labelFilter)
                labels.Add(new FilterPair(pair.Original, pair.Filtered));
            var directors = new List<FilterPair>();
            foreach (var pair in directorFilter)
                directors.Add(new FilterPair(pair.Original, pair.Filtered));
            var genres = new List<FilterPair>();
            foreach (var pair in categoryFilter)
                genres.Add(new FilterPair(pair.Original, pair.Filtered));         
            var actors = new List<FilterPair>();
            CommandQueue.Command().Execute(new CmdFilter(movies, studios, labels, directors, genres, actors));
            SearchMovies();
            SearchActresses();
            Save();
        }

        public void DeleteMetadata(List<MovieData> movies)
        {
            lock (m_cacheData)
            {
                foreach (var movie in movies)
                {
                    if (m_cacheData.Movies.Contains(movie))
                    {
                        if (String.IsNullOrEmpty(movie.Path) == false && String.IsNullOrEmpty(movie.MetadataFileName) == false)
                        {
                            string metaDataPath = Path.Combine(movie.Path, movie.MetadataFileName);
                            try
                            {
                                if (File.Exists(metaDataPath))
                                    File.Delete(metaDataPath);
                            }
                            catch(Exception)
                            {
                            }
                        }
                        m_cacheData.Movies.Remove(movie);
                    }
                }
                m_moviesDisplayed.Clear();
            }
            SearchMovies();
            Save();
        }

        public void SearchMovies()
        {
            if (m_loaded == false)
                return;
            m_searchMovies = new CmdSearchMovies(this, m_cacheData, m_searchText, MovieSearchActress, m_sortMoviesBy, ShowUnratedOnly, ShowSubtitlesOnly, UseJapaneseNameOrder);
            CommandQueue.Command().Execute(m_searchMovies);            
        }

        public void SearchActresses()
        {
            if (m_loaded == false)
                return;
            if (SortActressesBy == SortActressesBy.MovieCount)
                CommandQueue.Command().Execute(new CmdUpdateActressMovieCount(this, m_cacheData, m_actressesDatabase));
            m_searchActresses = new CmdSearchActresses(m_actressesDatabase, m_searchText, m_sortActressesBy, m_showUnknownActresses, m_useJapaneseNameOrder);
            CommandQueue.Command().Execute(m_searchActresses);
        }

        public void Save()
        {
            if (m_readOnlyMode == true)
                return;
            CommandQueue.Command().Execute(new CmdMarkSharedFolders(m_cacheData));
            CommandQueue.Command().Execute(new CmdSave(m_cacheData, m_cacheFilename, m_actressesDatabase, m_actressesFilename, m_backupData, m_backupFilename));
        }

        public void UpdateActressNames()
        {
            if (AutoSyncActresses)
                CommandQueue.Command().Execute(new CmdUpdateActressNames(this, m_cacheData, m_actressesDatabase));
        }

        public void CleanActressImages()
        {
            CommandQueue.Command().Execute(new CmdCleanActressImages(m_actressesDatabase));
        }

        public void UpdateDateAdded()
        {
            CommandQueue.LongTask().Execute(new CmdUpdateDateAdded(m_cacheData));
        }

        #endregion

        #region Private Functions

        private void AddActressNoLock(ActressData actress)
        {
            if (m_actressesDatabase.Actresses.Contains(new ActressData(actress.Name)))
            {
                Logger.WriteError("Attempting to add " + actress.Name + ", but this name is already used");
                return;
            }
            if (m_actressesDatabase.AltNames.Contains(new NamePair(actress.Name)))
            {
                Logger.WriteError("Attempting to add " + actress.Name + ", but this already exists as an alternate name");
                return;
            }
            m_actressesDatabase.Actresses.Add(actress);
            if (String.IsNullOrEmpty(actress.JapaneseName) == false)
                m_actressesDatabase.JapaneseNames.Add(new NamePair(actress.JapaneseName));
            foreach (string altName in actress.AltNames)
                m_actressesDatabase.AltNames.Add(new NamePair(altName, actress.Name));
        }

        private void RemoveActressNoLock(ActressData actress, bool deleteImages)
        {
            m_actressesDatabase.Actresses.Remove(actress);
            if (deleteImages)
            {
                try
                {
                    var folder = Utilities.GetActressImageFolder();
                    foreach (var fn in actress.ImageFileNames)
                    {
                        var fullPath = Path.Combine(folder, fn);
                        File.Delete(fullPath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError("Could not delete actress image", ex);
                }
            }
            if (String.IsNullOrEmpty(actress.JapaneseName) == false)
                m_actressesDatabase.JapaneseNames.Remove(new NamePair(actress.JapaneseName));
            foreach (string alias in actress.AltNames)
                m_actressesDatabase.AltNames.Remove(new NamePair(alias));
        }

        private void NotifyMoviesDisplayedChanged()
        {
            EventHandler handler = MoviesDisplayedChanged;
            handler?.Invoke(this, new EventArgs());
        }

        private void NotifyActressesDisplayedChanged()
        {
            EventHandler handler = ActressesDisplayedChanged;
            handler?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Private Members

        // Displayed results
        private List<MovieData> m_moviesDisplayed = new List<MovieData>();
        private List<ActressData> m_actressesDisplayed = new List<ActressData>();

        // Search parameters
        private string m_searchText = String.Empty;

        // Movie search
        private CmdSearchMovies m_searchMovies;
        private SortMoviesBy m_sortMoviesBy;

        // Actress search
        private CmdSearchActresses m_searchActresses;
        private SortActressesBy m_sortActressesBy;
        private bool m_useJapaneseNameOrder;

        // Show ID with title
        private bool m_showID;

        // Show user rating with title
        private bool m_showUserRating;

        // Show only unrated movies
        private bool m_showUnratedOnly;

        // Show only subtitled movies
        private bool m_showSubtitlesOnly;

        // Show unknown actresses
        private bool m_showUnknownActresses;

        // Internal data
        private System.Windows.Threading.Dispatcher m_dispatcher;
        private System.Timers.Timer m_timer;
        private CacheData m_cacheData = new CacheData();
        private BackupData m_backupData = new BackupData();
        private ActressesDatabase m_actressesDatabase = new ActressesDatabase();
        private string m_cacheFilename = String.Empty;
        private string m_actressesFilename = String.Empty;
        private string m_backupFilename = String.Empty;
        private bool m_loaded = false;
        private bool m_readOnlyMode = false;
        private Random m_rng = new Random();

        #endregion
    }
}
