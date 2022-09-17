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

        public MovieCollection(System.Windows.Threading.Dispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
            CommandQueue.Command().CommandFinished += CommandQueue_CommandFinished;
            var folder = Utilities.GetJavLuvSettingsFolder();
            m_cacheFilename = Path.Combine(folder, "JavLuv.cache");
            m_backupFilename = Path.Combine(folder, "Metadata.backup");
            if (File.Exists(m_cacheFilename))
                CommandQueue.Command().Execute(new CmdLoad(ref m_cacheData, m_cacheFilename, ref m_backupData, m_backupFilename));
            else
                m_loaded = true;
        }

        #endregion

        #region Events

        public event EventHandler MoviesDisplayedChanged;

        private void CommandQueue_CommandFinished(object sender, CommandEventArgs e)
        {
            if (e.CommandName == "MovieInfo.CmdLoad")
            {
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()          
                { 
                    NotifyMoviesDisplayedChanged();
                    m_loaded = true;
                    Search();
                }));     
            }
            else if (e.CommandName == "MovieInfo.CmdSearch")
            {
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    if (m_search != null)
                    {
                        m_moviesDisplayed = m_search.FilteredMovies;
                        NotifyMoviesDisplayedChanged();                     
                    }
                }));
            }
        }

        #endregion

        #region Properties

        public List<MovieData> MoviesDisplayed { get { return m_moviesDisplayed; } }

        public string SearchText
        {
            private get { return m_searchText; }
            set 
            { 
                m_searchText = value;
                Search();
            }
        }

        public SortBy SortBy
        {
            private get { return m_sortBy; }
            set
            {
                if (value != m_sortBy)
                {
                    m_sortBy = value;
                    Search();
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
                    Search();
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
                    Search();
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
                    Search();
                }
            }
        }

        public int NumMovies
        {
            get { return m_cacheData.Movies.Count; }
        }

        #endregion

        #region Public Functions

        public List<MovieData> AddMovies(List<MovieData> movies, out string errorMsg)
        {
            List<MovieData> newMovies = new List<MovieData>();
            var err = new StringBuilder(1024);
            lock (m_cacheData)
            {
                foreach (var movie in movies)
                {
                    // There's already a movie with this ID.  Check if we're updating location
                    // or if it's a duplicate error.
                    MovieData existingMovie;
                    if (m_cacheData.Movies.TryGetValue(movie, out existingMovie))
                    {
                        if (Directory.Exists(existingMovie.Path) && movie.Path != existingMovie.Path)
                        {
                            err.Append("Movie ID: ");
                            err.Append(existingMovie.Metadata.UniqueID.Value);
                            err.Append(" already exists in collection at: ");
                            err.Append(existingMovie.Path);
                            err.Append("\n");
                            err.Append("New movie skipped: ");
                            err.Append(movie.Path);
                            err.Append("\n\n");
                        }
                        else
                        {
                            // Preserve movie resolution as a special case
                            movie.MovieResolution = existingMovie.MovieResolution;
                            m_cacheData.Movies.Remove(movie);
                            m_cacheData.Movies.Add(movie);
                            newMovies.Add(movie);
                        }
                    }
                    else
                    {
                        m_cacheData.Movies.Add(movie);
                        newMovies.Add(movie);
                    }
                }
            }
            Search();
            Save();
            errorMsg = err.ToString();
            return newMovies;
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

        public bool FileInCollection(string uniqueID, string fileName)
        {
            lock (m_cacheData)
            {
                MovieData key = new MovieData();
                key.Metadata.UniqueID.Value = uniqueID;
                MovieData result = new MovieData();
                string dirName = Path.GetDirectoryName(fileName);
                if (m_cacheData.Movies.TryGetValue(key, out result))
                {
                    if (result.Path == dirName)
                        return true;
                }
                foreach (MovieData movieData in m_cacheData.Movies)
                {
                    if (movieData.Path == dirName)
                        return true;
                }
                
                return false;
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
                MovieMetadata check = new MovieMetadata();
                check.UniqueID.Value = uniqueID;
                if (m_backupData.Movies.TryGetValue(check, out value))
                    return value;
            }
            return null;
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
            Search();
            Save();
        }

        public void FilterMetadata(List<MovieData> movies, List<FilterPair> studioFilter, List<FilterPair> labelFilter,
            List<FilterPair> directorFilter, List<FilterPair> categoryFilter, List<FilterPair> actorFilter)
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
            foreach (var pair in actorFilter)
                actors.Add(new FilterPair(pair.Original, pair.Filtered));
            CommandQueue.Command().Execute(new CmdFilter(movies, studios, labels, directors, genres, actors));
            Search();
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
            Search();
            Save();
        }

        public void DeleteCache()
        {
            lock (m_cacheData)
            {
                m_cacheData = new CacheData();
            }
            m_moviesDisplayed.Clear();
            try
            {
                if (File.Exists(m_cacheFilename))
                    File.Delete(m_cacheFilename);
            }
            catch(Exception)
            {
            }
            NotifyMoviesDisplayedChanged();
        }

        public void Search()
        {
            if (m_loaded == false)
                return;
            m_search = new CmdSearch(m_cacheData, m_searchText, m_sortBy, ShowUnratedOnly, ShowSubtitlesOnly);
            CommandQueue.Command().Execute(m_search);            
        }

        public void Save()
        {
            CommandQueue.Command().Execute(new CmdMarkSharedFolders(m_cacheData));
            CommandQueue.Command().Execute(new CmdSave(m_cacheData, m_cacheFilename, m_backupData, m_backupFilename));
        }

        #endregion

        #region Private Functions

        private void NotifyMoviesDisplayedChanged()
        {
            EventHandler handler = MoviesDisplayedChanged;
            handler?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Private Members

        // Displayed results
        private List<MovieData> m_moviesDisplayed = new List<MovieData>();

        // Search parameters
        private string m_searchText = String.Empty;

        // Current search command
        private CmdSearch m_search;

        // Current sort by command
        private SortBy m_sortBy;

        // Show ID with title
        private bool m_showID;

        // Show only unrated movies
        private bool m_showUnratedOnly;

        // Show only subtitled movies
        private bool m_showSubtitlesOnly;

        // Internal data
        private System.Windows.Threading.Dispatcher m_dispatcher;
        private CacheData m_cacheData = new CacheData();
        private BackupData m_backupData = new BackupData();
        private string m_cacheFilename = String.Empty;
        private string m_backupFilename = String.Empty;
        private bool m_loaded = false;

        #endregion
    }
}
