using Common;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JavLuv
{

    public class LanguageData : ObservableObject
    {
        #region Constructor

        public LanguageData(LanguageType language)
        {
            Language = language;
        }

        #endregion

        #region Properties

        public LanguageType Language { get; private set; }

        public string Text
        {
            get
            {
                if (Language == LanguageType.English)
                    return TextManager.GetString("Text.English");
                else if (Language == LanguageType.Japanese)
                    return TextManager.GetString("Text.Japanese");
                return App.Current.Properties["English"].ToString();
            }
        }

        #endregion

        #region Public Functions

        public void Refresh()
        {
            NotifyPropertyChanged("Text");
        }

        #endregion
    }

    public class SettingsViewModel : ObservableObject
    {
        #region Constructors

        public SettingsViewModel(MainWindowViewModel parent)
        {
            m_parent = parent;
            Languages = new ObservableCollection<LanguageData>();
            LanguageType currentLanguage = Settings.Get().Language;
            foreach (LanguageType lang in Enum.GetValues(typeof(LanguageType)))
            {
                var langData = new LanguageData(lang);
                if (lang == currentLanguage)
                    SelectedLanguage = langData;
                Languages.Add(langData);
            }
        }

        #endregion

        #region Properties

        public MainWindowViewModel Parent { get { return m_parent; } }

        public string Version
        {
            get
            {
                string[] versions = typeof(SettingsViewModel).Assembly.GetName().Version.ToString().Split('.');
                return String.Format("v{0}.{1}.{2}", versions[0], versions[1], versions[2]);
            }
        }

        public Settings Data { get { return Settings.Get(); } }

        public LanguageData SelectedLanguage
        {
            get
            {
                return m_selectedLanguage;
            }
            set
            {
                if (m_selectedLanguage == null || value.Language != m_selectedLanguage.Language)
                {
                    m_selectedLanguage = value;
                    Settings.Get().Language = m_selectedLanguage.Language;
                    TextManager.SetLanguage(m_selectedLanguage.Language);
                    foreach (var languageData in Languages)
                        languageData.Refresh();
                    if (Parent.SidePanel != null)
                        Parent.SidePanel.NotifyAllProperty();
                    NotifyAllPropertiesChanged();
               }
            }
        }

        public ObservableCollection<LanguageData> Languages { get; private set; }

        public bool ShowAdvancedOptions
        {
            get { return Settings.Get().ShowAdvancedOptions; }
            set
            {
                if (value != Settings.Get().ShowAdvancedOptions)
                {
                    Settings.Get().ShowAdvancedOptions = value;
                    Parent.SidePanel.NotifyAllProperty();
                    Parent.Browser.NotifyAllProperties();
                    NotifyPropertyChanged("ShowAdvancedOptions");
                }
            }
        }

        public bool CheckForUpdates
        {
            get { return Settings.Get().CheckForUpdates; }
            set
            {
                if (value != Settings.Get().CheckForUpdates)
                {
                    Settings.Get().CheckForUpdates = value;
                    NotifyPropertyChanged("CheckForUpdates");
                }
            }
        }

        public string Subtitles
        {
            get { return Settings.Get().Subtitles; }
            set
            {
                if (value != Settings.Get().Subtitles)
                {
                    Settings.Get().Subtitles = value;
                    NotifyPropertyChanged("Subtitles");
                }
            }
        }

        public bool HideMetadataAndCovers
        {
            get { return Settings.Get().HideMetadataAndCovers; }
            set
            {
                if (value != Settings.Get().HideMetadataAndCovers)
                {
                    Settings.Get().HideMetadataAndCovers = value;
                    NotifyPropertyChanged("HideMetadataAndCovers");
                }
            }
        }

        public bool GenerateLocalMetadata
        {
            get { return Settings.Get().GenerateLocalMetadata; }
            set
            {
                if (value != Settings.Get().GenerateLocalMetadata)
                {
                    Settings.Get().GenerateLocalMetadata = value;
                    NotifyPropertyChanged("GenerateLocalMetadata");
                }
            }
        }

        public bool UseFolderAsTitle
        {
            get { return Settings.Get().UseFolderAsTitle; }
            set
            {
                if (value != Settings.Get().UseFolderAsTitle)
                {
                    Settings.Get().UseFolderAsTitle = value;
                    NotifyPropertyChanged("UseFolderAsTitle");
                }
            }
        }

        public bool UseMovieFilenameAsTitle
        {
            get { return Settings.Get().UseMovieFilenameAsTitle; }
            set
            {
                if (value != Settings.Get().UseMovieFilenameAsTitle)
                {
                    Settings.Get().UseMovieFilenameAsTitle = value;
                    NotifyPropertyChanged("UseMovieFilenameAsTitle");
                }
            }
        }

        public bool AutoRestoreMetadata
        {
            get { return Settings.Get().AutoRestoreMetadata; }
            set
            {
                if (value != Settings.Get().AutoRestoreMetadata)
                {
                    Settings.Get().AutoRestoreMetadata = value;
                    NotifyPropertyChanged("AutoRestoreMetadata");
                }
            }
        }

        public string CoverNames
        {
            get { return Settings.Get().CoverNames; }
            set
            {
                if (value != Settings.Get().CoverNames)
                {
                    Settings.Get().CoverNames = value;
                    NotifyPropertyChanged("CoverNames");
                }
            }
        }

        public string ThumbnailNames
        {
            get { return Settings.Get().ThumbnailNames; }
            set
            {
                if (value != Settings.Get().ThumbnailNames)
                {
                    Settings.Get().ThumbnailNames = value;
                    NotifyPropertyChanged("ThumbnailNames");
                }
            }
        }

        public string MovieExclusions
        {
            get { return Settings.Get().MovieExclusions; }
            set
            {
                if (value != Settings.Get().MovieExclusions)
                {
                    Settings.Get().MovieExclusions = value;
                    NotifyPropertyChanged("MovieExclusions");
                }
            }
        }

        public string SubtitleExtensions
        {
            get { return Settings.Get().SubtitleExts; }
            set
            {
                if (value != Settings.Get().SubtitleExts)
                {
                    Settings.Get().SubtitleExts = value;
                    NotifyPropertyChanged("SubtitleExtensions");
                }
            }
        }

        public bool EnableMoveRename
        {
            get { return Settings.Get().EnableMoveRename; }
            set
            {
                if (value != Settings.Get().EnableMoveRename)
                {
                    Settings.Get().EnableMoveRename = value;
                    NotifyPropertyChanged("EnableMoveRename");
                    Parent.SidePanel.NotifyAllProperty();
                    Parent.Browser.NotifyAllProperties();
                }
            }
        }

        public string Library
        {
            get { return Settings.Get().Library; }
            set
            {
                if (value != Settings.Get().Library)
                {
                    Settings.Get().Library = value;
                    NotifyPropertyChanged("Library");
                }
            }
        }

        public string Folder
        {
            get { return Settings.Get().Folder; }
            set
            {
                if (value != Settings.Get().Folder)
                {
                    Settings.Get().Folder = value;
                    NotifyPropertyChanged("Folder");
                }
            }
        }

        public string Movie
        {
            get { return Settings.Get().Movie; }
            set
            {
                if (value != Settings.Get().Movie)
                {
                    Settings.Get().Movie = value;
                    NotifyPropertyChanged("Movie");
                }
            }
        }

        public string Cover
        {
            get { return Settings.Get().Cover; }
            set
            {
                if (value != Settings.Get().Cover)
                {
                    Settings.Get().Cover = value;
                    NotifyPropertyChanged("Cover");
                }
            }
        }

        public string Preview
        {
            get { return Settings.Get().Preview; }
            set
            {
                if (value != Settings.Get().Preview)
                {
                    Settings.Get().Preview = value;
                    NotifyPropertyChanged("Preview");
                }
            }
        }

        public string Metadata
        {
            get { return Settings.Get().Metadata; }
            set
            {
                if (value != Settings.Get().Metadata)
                {
                    Settings.Get().Metadata = value;
                    NotifyPropertyChanged("Metadata");
                }
            }
        }

        public string StudioFilters
        {
            get { return Utilities.FilterListToString(Settings.Get().Culture.StudioFilters); }
            set
            {
                var filterList = Utilities.StringToFilterList(value);
                Settings.Get().Culture.StudioFilters = filterList;
                NotifyPropertyChanged("StudioFilters");
            }
        }

        public string LabelFilters
        {
            get { return Utilities.FilterListToString(Settings.Get().Culture.LabelFilters); }
            set
            {
                var filterList = Utilities.StringToFilterList(value);
                Settings.Get().Culture.LabelFilters = filterList;
                NotifyPropertyChanged("LabelFilters");
            }
        }

        public string DirectorFilters
        {
            get { return Utilities.FilterListToString(Settings.Get().Culture.DirectorFilters); }
            set
            {
                var filterList = Utilities.StringToFilterList(value);
                Settings.Get().Culture.DirectorFilters = filterList;
                NotifyPropertyChanged("DirectorFilters");
            }
        }

        public string GenreFilters
        {
            get { return Utilities.FilterListToString(Settings.Get().Culture.GenreFilters); }
            set
            {
                var filterList = Utilities.StringToFilterList(value);
                Settings.Get().Culture.GenreFilters = filterList;
                NotifyPropertyChanged("GenreFilters");
            }
        }

        public string ActorFilters
        {
            get { return Utilities.FilterListToString(Settings.Get().Culture.ActorFilters); }
            set
            {
                var filterList = Utilities.StringToFilterList(value);
                Settings.Get().Culture.ActorFilters = filterList;
                NotifyPropertyChanged("ActorFilters");
            }
        }

        #endregion

        #region Commands

        #region CloseSettings Command

        private void CloseSettingsExecute()
        {
            Parent.CloseOverlay();
        }

        private bool CanCloseSettingsExecute()
        {
            return true;
        }

        public ICommand CloseSettingsCommand { get { return new RelayCommand(CloseSettingsExecute, CanCloseSettingsExecute); } }

        #endregion

        #region Choose Subtitles Folder Command

        private void ChooseSubtitlesFolderExecute()
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (String.IsNullOrEmpty(Settings.Get().Subtitles))
                dlg.SelectedPath = Utilities.GetValidSubFolder(Settings.Get().LastFolder);
            else
                dlg.SelectedPath = Utilities.GetValidSubFolder(Settings.Get().Subtitles);
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;
            Subtitles = dlg.SelectedPath;
        }

        private bool CanChooseSubtitlesFolderExecute()
        {
            return true;
        }

        public ICommand ChooseSubtitlesFolderCommand { get { return new RelayCommand(ChooseSubtitlesFolderExecute, CanChooseSubtitlesFolderExecute); } }

        #endregion

        #region Choose Library Folder Command

        private void ChooseLibraryFolderExecute()
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = Utilities.GetValidSubFolder(Settings.Get().LastFolder);
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;
            Settings.Get().LastFolder = dlg.SelectedPath;
            Library = dlg.SelectedPath;
        }

        private bool CanChooseLibraryFolderExecute()
        {
            return true;
        }

        public ICommand ChooseLibraryFolderCommand { get { return new RelayCommand(ChooseLibraryFolderExecute, CanChooseLibraryFolderExecute); } }

        #endregion

        #region Reset to Defaults Command

        private void ResetToDefaultsExecute()
        {
            Settings.Reset();
            NotifyAllPropertiesChanged();
        }

        private bool CanResetToDefaultsExecute()
        {
            return true;
        }

        public ICommand ResetToDefaultsCommand { get { return new RelayCommand(ResetToDefaultsExecute, CanResetToDefaultsExecute); } }

        #endregion

        #region Reset Filters Command

        private void ResetFiltersExecute()
        {
            Settings.ResetFilters();
            NotifyAllPropertiesChanged();
        }

        private bool CanResetFiltersExecute()
        {
            return true;
        }

        public ICommand ResetFiltersCommand { get { return new RelayCommand(ResetFiltersExecute, CanResetFiltersExecute); } }

        #endregion

        #region Merge Filters Command

        private void MergeFiltersExecute()
        {
            Settings.MergeFilters();
            NotifyAllPropertiesChanged();
        }

        private bool CanMergeFiltersExecute()
        {
            return true;
        }

        public ICommand MergeFiltersCommand { get { return new RelayCommand(MergeFiltersExecute, CanMergeFiltersExecute); } }

        #endregion

        #region Copy Log File To Command

        private void CopyLogFileToExecute()
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;
            Logger.ZipAndCopyLogTo(dlg.SelectedPath);
        }

        private bool CanCopyLogFileToExecute()
        {
            return true;
        }

        public ICommand CopyLogFileToCommand { get { return new RelayCommand(CopyLogFileToExecute, CanCopyLogFileToExecute); } }

        #endregion

        #endregion

        #region Private Members

        private MainWindowViewModel m_parent;
        private LanguageData m_selectedLanguage;

        #endregion
    }
}
