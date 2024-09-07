using Common;
using MovieInfo;
using Subtitles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace JavLuv
{

    [Serializable]
    public class CultureSettings
    {
        #region Constructors

        public CultureSettings()
        {

        }

        public CultureSettings(LanguageType language)
        {
            Language = language;
        }

        #endregion

        #region Properties

        public LanguageType Language { get; set; }
        public List<FilterPair> StudioFilters { get; set; }
        public List<FilterPair> DirectorFilters { get; set; }
        public List<FilterPair> LabelFilters { get; set; }
        public List<FilterPair> GenreFilters { get; set; }

        #endregion
    }


    [Serializable]
    public class Settings
    {
        #region Constructors

        static Settings()
        {
            s_filePath = Path.Combine(Utilities.GetJavLuvSettingsFolder(), "JavLuv.settings");
        }

        public Settings()
        {
            MainWindowState = System.Windows.WindowState.Normal;
            MainWindowHeight = 600;
            MainWindowWidth = 800;
            MainWindowLeft = 100;
            MainWindowTop = 100;

            ScanFolder = String.Empty;
            ImageImportFolder = String.Empty;
            ConcatFolder = String.Empty;
            MoveToFolder = String.Empty;
            FindSubtitlesFolder = String.Empty;
            SubtitleImportFolder = String.Empty;
            SubtitleExportFolder = String.Empty;

            AddToCollection = true;
            ScanRecursively = true;
            AutoImportImprovedMovies = true;
            SearchViewWidth = new GridLength(300);
            SearchText = String.Empty;
            LastVersionRun = SemanticVersion.Current;
            LastVersionChecked = SemanticVersion.Current;
            OrganizerMode = Organizer.Mode.Copy;
            IsDefault = true;

            // Set initial language based on current culture
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            if (currentCulture.Name == "jp-JP")
                Language = LanguageType.Japanese;
            else
                Language = LanguageType.English; ;
        }

        #endregion

        #region Properties

        // Indicates these are default values
        public bool IsDefault { get; set; }


        // Window properties
        public System.Windows.WindowState MainWindowState { get; set; }
        public double MainWindowWidth { get; set; }
        public double MainWindowHeight { get; set; }
        public double MainWindowLeft { get; set; }
        public double MainWindowTop { get; set; }

        [XmlIgnore]
        public GridLength SearchViewWidth { get; set; }

        public double SearchViewWidthValue
        {
            get { return SearchViewWidth.Value; }
            set
            {
                if (value != SearchViewWidth.Value)
                {
                    SearchViewWidth = new GridLength(value);
                }
            }
        }

        // Preserved UI elements
        public string ScanFolder { get; set; }
        public string ImageImportFolder { get; set; }
        public string ConcatFolder { get; set; }
        public string MoveToFolder { get; set; }
        public string FindSubtitlesFolder { get; set; }
        public string SubtitleImportFolder { get; set; }
        public string SubtitleExportFolder { get; set; }
        public int SelectedTabIndex { get; set; }
        public Organizer.Mode OrganizerMode { get; set; }
        public bool AddToCollection { get; set; }
        public bool ScanRecursively { get; set; }
        public bool AutoImportImprovedMovies { get; set; }
        public bool MoveRenameAfterScan { get; set; }
        public string SearchText { get; set; }
        public MovieInfo.SortMoviesBy SortMoviesBy { get; set; }
        public MovieInfo.SortActressesBy SortActressesBy { get; set; }
        public bool ShowID { get; set; }
        public bool ShowUserRating { get; set; }
        public bool ShowUnratedOnly { get; set; }
        public bool ShowSubtitlesOnly { get; set; }
        public bool ShowAllActresses { get; set; }
        public bool ShowOriginalTitle { get; set; }

        // Misc persistent data
        public SemanticVersion LastVersionRun { get; set; }
        public SemanticVersion LastVersionChecked { get; set; }
        public DateTime LastVersionCheckTime { get; set; }

        // Config settings
        public bool CheckForUpdates { get; set; }
        public bool AutoSyncActresses { get; set; }
        public bool AllowMultipleInstances { get; set; }
        public LanguageType Language { get; set; }
        public ThemeType Theme { get; set; }
        public bool ShowAdvancedOptions { get; set; }
        public bool ShowActressAgeAtPremier { get; set; }
        public bool UseJapaneseNameOrder { get; set; }
        public string Subtitles { get; set; }
        public bool GenerateLocalMetadata { get; set; }
        public bool UseFolderAsTitle { get; set; }
        public bool UseMovieFilenameAsTitle { get; set; }
        public bool HideMetadataAndCovers { get; set; }
        public bool AutoRestoreMetadata { get; set; }
        public string SubtitleExts { get; set; }
        public string CoverNames { get; set; }
        public string ThumbnailNames { get; set; }
        public string MovieExclusions { get; set; }
        public bool EnableMoveRename { get; set; }
        public string Library { get; set; }
        public string Folder { get; set; }
        public string Movie { get; set; }
        public string Cover { get; set; }
        public string Preview { get; set; }
        public string Metadata { get; set; }

        // Culture-specific settings
        public List<CultureSettings> Cultures
        {
            get { return m_cultures; }
            set { m_cultures = value; }
        }

        [XmlIgnore] 
        public CultureSettings Culture 
        { 
            get { return m_cultures[(int)Language]; } 
        }

        #endregion

        #region Public Functions

        public static void Load()
        {
            Logger.WriteInfo("Loading Settings");
            lock (s_settings)
            {
                try
                {
                    if (File.Exists(s_filePath))
                        s_settings = MovieSerializer<Settings>.Load(s_filePath, Filter);
                }
                catch(Exception ex)
                {
                    Logger.WriteError("Error loading settings", ex);
                }

                // Defaults are applied the first time the app is run
                if (s_settings.IsDefault)
                {
                    s_settings.LoadDefaultValues();
                    s_settings.IsDefault = false;
                }

                // We're upgrading versions!  If we need to set a new default value, here is the place to do it.
                if (s_settings.LastVersionRun != SemanticVersion.Current)
                {
                    if (SemanticVersion.Current == new SemanticVersion(1, 1, 16))
                        s_settings.AutoImportImprovedMovies = true;
                    if (SemanticVersion.Current == new SemanticVersion(1, 1, 31))
                        s_settings.AddToCollection = true;
                }
            }
        }

        public static void Save()
        {
            Logger.WriteInfo("Saving Settings");
            lock (s_settings)
            {
                try
                {
                    MovieSerializer<Settings>.Save(s_filePath, s_settings);
                }
                catch (Exception ex)
                {
                    Logger.WriteError("Error saving settings", ex);
                }
            }
        }

        public static void Reset()
        {
            lock (s_settings)
            {
                s_settings.LoadDefaultValues();
            }
        }

        public static void ResetFilters()
        {
            lock (s_settings)
            {
                s_settings.Cultures = LoadDefaultFilters();
            }
        }

        public static void MergeFilters()
        {
            lock (s_settings)
            {
                var defaultFilters = LoadDefaultFilters();
                for (int i = 0; i < s_settings.Cultures.Count; ++i)
                {
                    s_settings.Cultures[i].StudioFilters = MergeFilterLists(s_settings.Cultures[i].StudioFilters, defaultFilters[i].StudioFilters);
                    s_settings.Cultures[i].LabelFilters = MergeFilterLists(s_settings.Cultures[i].LabelFilters, defaultFilters[i].LabelFilters);
                    s_settings.Cultures[i].DirectorFilters = MergeFilterLists(s_settings.Cultures[i].DirectorFilters, defaultFilters[i].DirectorFilters);
                    s_settings.Cultures[i].GenreFilters = MergeFilterLists(s_settings.Cultures[i].GenreFilters, defaultFilters[i].GenreFilters);
                }
            }
        }

        public static Settings Get()
        {
            lock(s_settings)
            {
                return s_settings;
            }
        }

        #endregion

        #region Private Functions

        private static void Filter(XDocument doc)
        {
            XElement root = doc.Root;

            // Check for culture-specific settings version.  If not found, create them now.
            XElement cultures = root.Element("Cultures");
            if (cultures == null)
            {
                cultures = new XElement("Cultures");
                root.LastNode.AddAfterSelf(cultures);
                foreach (LanguageType lang in Enum.GetValues(typeof(LanguageType)))
                {
                    var cultureSettings = new XElement("CultureSettings");
                    cultureSettings.Add(new XElement("Language", lang.ToString()));
                    cultures.Add(cultureSettings);
                }
                
                // We'll copy the elements that used to belong to root to the English-specific settings
                XElement englishSettings = cultures.FirstNode as XElement;
                XElement filters = root.Element("StudioFilters");
                englishSettings.Add(filters);
                filters = root.Element("LabelFilters");
                englishSettings.Add(filters);
                filters = root.Element("DirectorFilters");
                englishSettings.Add(filters);
                filters = root.Element("GenreFilters");
                englishSettings.Add(filters);
                filters = root.Element("ActorFilters");
                englishSettings.Add(filters);
            }

            // We're changing the name of the SortBy field
            XElement sortBy = root.Element("SortBy");
            if (sortBy != null)
            {
                sortBy.Name = "SortMoviesBy";
            }

            // LastFolder is becoming a number of specialized folders.  We'll 
            // initialize all of them with the old LastFolder value to start with.
            XElement lastFolder = root.Element("LastFolder");
            if (lastFolder != null)
            {
                lastFolder.AddAfterSelf(new XElement("ScanFolder", lastFolder.Value));
                lastFolder.AddAfterSelf(new XElement("ImageImportFolder", lastFolder.Value));
                lastFolder.AddAfterSelf(new XElement("ConcatFolder", lastFolder.Value));
                lastFolder.AddAfterSelf(new XElement("MoveToFolder", lastFolder.Value));
                lastFolder.AddAfterSelf(new XElement("FindSubtitlesFolder", lastFolder.Value));
                lastFolder.AddAfterSelf(new XElement("SubtitleImportFolder", lastFolder.Value));
                lastFolder.AddAfterSelf(new XElement("SubtitleExportFolder", lastFolder.Value));
            }
        }

        private void LoadDefaultValues()
        {
            ShowAdvancedOptions = false;
            CheckForUpdates = true;
            Subtitles = "";
            GenerateLocalMetadata = false;
            UseFolderAsTitle = true;
            UseMovieFilenameAsTitle = false;
            HideMetadataAndCovers = false;
            AutoRestoreMetadata = true;
            LastVersionRun = SemanticVersion.Current;
            LastVersionCheckTime = new DateTime(2022, 1, 1);
            SubtitleExts = "srt; vtt; ssa; ass; smi";

            // Some settings should be common, but defaults may differ by language
            if (Language == LanguageType.Japanese)
            {
                CoverNames = "cover; title; poster";
                ThumbnailNames = "thumb; thumbnail; preview; screenshot";
                MovieExclusions = "excerpt; preview; trailer; behind the scenes; bonus video";
            }
            else
            {
                CoverNames = "cover; title; poster";
                ThumbnailNames = "thumb; thumbnail; preview; screenshot";
                MovieExclusions = "excerpt; preview; trailer; behind the scenes; bonus video";
            }

            EnableMoveRename = false;
            Library = "";
            Folder = "[{DVD-ID}] {TITLE 80}";
            Movie = "{DVD-ID}{SEQUENCE \"-\" ALPHA}";
            if (Language == LanguageType.Japanese)
            {
                 Cover = "{DVD-ID} cover";
                Preview = "{DVD-ID} preview{SEQUENCE \" \" ALPHA_LOWER}";
            }
            else
            {
                Cover = "{DVD-ID} cover";
                Preview = "{DVD-ID} preview{SEQUENCE \" \" ALPHA_LOWER}";
            }
            Metadata = "{DVD-ID}";

            // Replace existing culture-specific filters
            Cultures = LoadDefaultFilters();
        }

        private static List<CultureSettings> LoadDefaultFilters()
        {
            var cultures = new List<CultureSettings>();
            foreach (LanguageType lang in Enum.GetValues(typeof(LanguageType)))
                cultures.Add(new CultureSettings(lang));

            // Set English defaults
            CultureSettings cs = cultures[(int)LanguageType.English];
            cs.StudioFilters = new List<FilterPair>();
            cs.StudioFilters.Add(new FilterPair("Anna to Hanako", "Anna and Hanako"));

            cs.LabelFilters = new List<FilterPair>();
            cs.LabelFilters.Add(new FilterPair("Anna to Hanako", "Anna and Hanako"));

            cs.DirectorFilters = new List<FilterPair>();
            cs.DirectorFilters.Add(new FilterPair("Roshilvia Takiguchi", "Silvia Takiguchi"));
            cs.DirectorFilters.Add(new FilterPair("Takiguchi Shiruvia", "Silvia Takiguchi"));
            cs.DirectorFilters.Add(new FilterPair("Aoi Rena", "Rena Aoi"));
            cs.DirectorFilters.Add(new FilterPair("Masaki Nao", "Nao Masaki"));

            cs.GenreFilters = new List<FilterPair>();
            cs.GenreFilters.Add(new FilterPair("Facesitting", "Face Sitting"));
            cs.GenreFilters.Add(new FilterPair("Lesbian Kiss", "Lesbian Kissing"));
            cs.GenreFilters.Add(new FilterPair("Outdoor", "Outdoors"));
            cs.GenreFilters.Add(new FilterPair("School Girls", "Schoolgirl"));
            cs.GenreFilters.Add(new FilterPair("OL", "Office Lady"));
            cs.GenreFilters.Add(new FilterPair("Foreign Objects", "Object Insertion"));
            cs.GenreFilters.Add(new FilterPair("4HR+", "Over 4 Hours"));
            cs.GenreFilters.Add(new FilterPair("Pantyhose Tights", "Pantyhose"));
            cs.GenreFilters.Add(new FilterPair("High Vision", "High-Def"));
            cs.GenreFilters.Add(new FilterPair("Hi-Def", "High-Def"));
            cs.GenreFilters.Add(new FilterPair("Six Nine", "Sixty-Nine"));
            cs.GenreFilters.Add(new FilterPair("69", "Sixty-Nine"));
            cs.GenreFilters.Add(new FilterPair("School Swimsuits", "School Swimsuit"));
            cs.GenreFilters.Add(new FilterPair("School Stuff", "School"));
            cs.GenreFilters.Add(new FilterPair("Dirty Words", "Dirty Talk"));
            cs.GenreFilters.Add(new FilterPair("Shaved", "Shaved Pussy"));
            cs.GenreFilters.Add(new FilterPair("Sale (limited time)", ""));
            cs.GenreFilters.Add(new FilterPair("Toy", "Sex Toy"));
            cs.GenreFilters.Add(new FilterPair("Sex Toys", "Sex Toy"));
            cs.GenreFilters.Add(new FilterPair("3P, 4P", "Threesome / Foursome"));
            cs.GenreFilters.Add(new FilterPair("3P / 4P", "Threesome / Foursome"));
            cs.GenreFilters.Add(new FilterPair("Daydreamers", ""));
            cs.GenreFilters.Add(new FilterPair("Dead Drunk", "Drunk"));
            cs.GenreFilters.Add(new FilterPair("Other Fetish", "Other Fetishes"));
            cs.GenreFilters.Add(new FilterPair("Nampa", "Picking Up Girls"));
            cs.GenreFilters.Add(new FilterPair("Digital Mosaic", ""));
            cs.GenreFilters.Add(new FilterPair("SM", "BDSM"));
            cs.GenreFilters.Add(new FilterPair("Anal Play", "Anal"));
            cs.GenreFilters.Add(new FilterPair("Variety", "Omnibus"));
            cs.GenreFilters.Add(new FilterPair("Best, Omnibus", "Omnibus"));
            cs.GenreFilters.Add(new FilterPair("Compilation", "Omnibus"));
            cs.GenreFilters.Add(new FilterPair("Sun tan", "Suntan"));
            cs.GenreFilters.Add(new FilterPair("Swimsuits", "Swimsuit"));
            cs.GenreFilters.Add(new FilterPair("Substance Use", "Drugs"));
            cs.GenreFilters.Add(new FilterPair("Drug", "Drugs"));
            cs.GenreFilters.Add(new FilterPair("Ropes & Ties", "Bondage"));
            cs.GenreFilters.Add(new FilterPair("Restraint", "Bondage"));
            cs.GenreFilters.Add(new FilterPair("Nasty, Hardcore", "Hardcore"));
            cs.GenreFilters.Add(new FilterPair("Female Detective", "Female Investigator"));
            cs.GenreFilters.Add(new FilterPair("Breasts", "Beautiful Breasts"));
            cs.GenreFilters.Add(new FilterPair("Beautiful Tits", "Beautiful Breasts"));
            cs.GenreFilters.Add(new FilterPair("Tits", "Beautiful Breasts"));
            cs.GenreFilters.Add(new FilterPair("Big Tits", "Big Breasts"));
            cs.GenreFilters.Add(new FilterPair("Big Asses", "Big Ass"));
            cs.GenreFilters.Add(new FilterPair("Huge Butt", "Big Ass"));
            cs.GenreFilters.Add(new FilterPair("Butt", "Ass Lover"));
            cs.GenreFilters.Add(new FilterPair("Tall", "Tall Girl"));
            cs.GenreFilters.Add(new FilterPair("KIMONO", "Kimono"));
            cs.GenreFilters.Add(new FilterPair("Bride, Young Wife", "Young Wife"));
            cs.GenreFilters.Add(new FilterPair("Kimono, Mourning", "Kimono"));
            cs.GenreFilters.Add(new FilterPair("Busty Fetish", "Big Breasts Lover"));
            cs.GenreFilters.Add(new FilterPair("Big Tits Lover", "Big Breasts Lover"));
            cs.GenreFilters.Add(new FilterPair("Vibe", "Vibrator"));
            cs.GenreFilters.Add(new FilterPair("Dildo", "Sex Toy"));
            cs.GenreFilters.Add(new FilterPair("Electric Massager", "Vibrator"));
            cs.GenreFilters.Add(new FilterPair("White Actress", "Caucasian Actress"));
            cs.GenreFilters.Add(new FilterPair("Various Worker", "Various Professions"));
            cs.GenreFilters.Add(new FilterPair("Mini Skirt", "Miniskirt"));
            cs.GenreFilters.Add(new FilterPair("Finger Fuck", "Fingering"));
            cs.GenreFilters.Add(new FilterPair("Girl", "Beautiful Girl"));
            cs.GenreFilters.Add(new FilterPair("Shame", "Humiliation"));
            cs.GenreFilters.Add(new FilterPair("Embarassment", "Humiliation"));
            cs.GenreFilters.Add(new FilterPair("Sweat", "Sweating"));
            cs.GenreFilters.Add(new FilterPair("Kiss Kiss", "Kiss"));
            cs.GenreFilters.Add(new FilterPair("Sailor Suit", "Sailor Uniform"));
            cs.GenreFilters.Add(new FilterPair("Female College Student", "College Girl"));
            cs.GenreFilters.Add(new FilterPair("DMM Exclusive", ""));
            cs.GenreFilters.Add(new FilterPair("MultipleSubmit", ""));
            cs.GenreFilters.Add(new FilterPair("Solowork", "Featured Actress"));
            cs.GenreFilters.Add(new FilterPair("Single Work", "Featured Actress"));
            cs.GenreFilters.Add(new FilterPair("Subjectivity", "POV"));

            // Set Japanese defaults (currently none)
            cs = cultures[(int)LanguageType.Japanese];
            cs.StudioFilters = new List<FilterPair>();
            cs.LabelFilters = new List<FilterPair>();
            cs.DirectorFilters = new List<FilterPair>();
            cs.GenreFilters = new List<FilterPair>();

            return cultures;
        }

        private static List<FilterPair> MergeFilterLists(List<FilterPair> listA, List<FilterPair> listB)
        {
            // Note that we're preserving list order
            var mergedList = new List<FilterPair>();
            HashSet<FilterPair> setA = new HashSet<FilterPair>();
            foreach (FilterPair filterPair in listA)
            {
                setA.Add(filterPair);
                mergedList.Add(filterPair);
            }
            foreach (FilterPair filterPair in listB)
            {
                if (setA.Contains(filterPair) == false)
                    mergedList.Add(filterPair);
            }
            return mergedList;
        }

        #endregion

        #region Private Members

        private List<CultureSettings> m_cultures;
        private static Settings s_settings = new Settings();
        private static string s_filePath;

        #endregion
    }
}
