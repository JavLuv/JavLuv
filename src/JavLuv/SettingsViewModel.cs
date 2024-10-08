﻿using Common;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;
using System.Windows.Input;
using LanguageTypePair = JavLuv.ObservableStringValuePair<Common.LanguageType>;
using LanguageTypePairList = System.Collections.ObjectModel.ObservableCollection<JavLuv.ObservableStringValuePair<Common.LanguageType>>;
using ThemeTypePair = JavLuv.ObservableStringValuePair<JavLuv.ThemeType>;
using ThemeTypePairList = System.Collections.ObjectModel.ObservableCollection<JavLuv.ObservableStringValuePair<JavLuv.ThemeType>>;

namespace JavLuv
{
    public class SettingsViewModel : ObservableObject
    {
        #region Constructors

        public SettingsViewModel(MainWindowViewModel parent)
        {
            m_parent = parent;
            Languages = new LanguageTypePairList();
            Languages.Add(new LanguageTypePair("Text.English", LanguageType.English));
            Languages.Add(new LanguageTypePair("Text.Japanese", LanguageType.Japanese));
            LanguageType currentLanguage = Settings.Get().Language;    
            foreach (var language in Languages)
            {
                if (Settings.Get().Language == language.Value)
                {
                    SelectedLanguage = language;
                    break;
                }
            }
            Themes = new ThemeTypePairList();
            Themes.Add(new ThemeTypePair("Text.Dark", ThemeType.Dark));
            Themes.Add(new ThemeTypePair("Text.Light", ThemeType.Light));
            ThemeType currentTheme = Settings.Get().Theme;
            foreach (var theme in Themes)
            {
                if (Settings.Get().Theme == theme.Value)
                {
                    SelectedTheme = theme;
                    break;
                }
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

        public LanguageTypePair SelectedLanguage
        {
            get
            {
                return m_selectedLanguage;
            }
            set
            {
                if (m_selectedLanguage == null || value.Value != m_selectedLanguage.Value)
                {
                    m_selectedLanguage = value;
                    Settings.Get().Language = m_selectedLanguage.Value;
                    TextManager.SetLanguage(m_selectedLanguage.Value);
                    foreach (var languageData in Languages)
                        languageData.Notify();
                    if (Parent.SidePanel != null)
                        Parent.SidePanel.NotifyAllProperty();
                    NotifyAllPropertiesChanged();
                    Parent.DisplayTitle();
               }
            }
        }

        public LanguageTypePairList Languages { get; private set; }

        public ThemeTypePair SelectedTheme
        {
            get
            {
                return m_selectedTheme;
            }
            set
            {
                if (m_selectedTheme == null || value.Value != m_selectedTheme.Value)
                {
                    m_selectedTheme = value;
                    Settings.Get().Theme = m_selectedTheme.Value;
                    ThemeManager.Get().SetTheme(m_selectedTheme.Value);
                    foreach (var themeData in Themes)
                        themeData.Notify();
                    if (Parent.SidePanel != null)
                        Parent.SidePanel.NotifyAllProperty();
                    NotifyAllPropertiesChanged();
                }
            }
        }

        public ThemeTypePairList Themes { get; private set; }

        public bool ShowAdvancedOptions
        {
            get { return Settings.Get().ShowAdvancedOptions; }
            set
            {
                if (value != Settings.Get().ShowAdvancedOptions)
                {
                    Settings.Get().ShowAdvancedOptions = value;
                    Parent.SidePanel.NotifyAllProperty();
                    Parent.MovieBrowser.NotifyAllProperties();
                    NotifyPropertyChanged("ShowAdvancedOptions");
                }
            }
        }

        public bool AllowMultipleInstances
        {
            get { return Settings.Get().AllowMultipleInstances; }
            set
            {
                if (value != Settings.Get().AllowMultipleInstances)
                {
                    Settings.Get().AllowMultipleInstances = value;
                    Parent.SidePanel.NotifyAllProperty();
                    Settings.Save();
                    NotifyPropertyChanged("AllowMultipleInstances");
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

        public bool AutoSyncActresses
        {
            get { return Settings.Get().AutoSyncActresses; }
            set
            {
                if (value != Settings.Get().AutoSyncActresses)
                {
                    Settings.Get().AutoSyncActresses = value;
                    Parent.Collection.AutoSyncActresses = value;
                    NotifyPropertyChanged("AutoSyncActresses");
                }
            }
        }

        public bool ShowActressAgeAtPremier
        {
            get { return Settings.Get().ShowActressAgeAtPremier; }
            set
            {
                if (value != Settings.Get().ShowActressAgeAtPremier)
                {
                    Settings.Get().ShowActressAgeAtPremier = value;
                    NotifyPropertyChanged("ShowActressAgeAtPremier");
                }
            }
        }

        public bool UseJapaneseNameOrder
        {
            get { return Settings.Get().UseJapaneseNameOrder; }
            set
            {
                if (value != Settings.Get().UseJapaneseNameOrder)
                {
                    Settings.Get().UseJapaneseNameOrder = value;
                    Parent.Collection.UseJapaneseNameOrder = value;
                    NotifyPropertyChanged("UseJapaneseNameOrder");
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
                    Parent.MovieBrowser.NotifyAllProperties();
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

        public Visibility DebugVisible
        {
            get
            {
                #if DEBUG
                return Visibility.Visible;
                #else
                return Visibility.Collapsed;
                #endif
            }
        }

        #endregion

        #region Commands

        #region CloseSettings Command

        private void CloseSettingsExecute()
        {
            if (Settings.Get().AutoSyncActresses == true)
                Parent.Collection.UpdateActressNames();
            Parent.Overlay = null;
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
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.EnsurePathExists = true;
            dlg.InitialDirectory = Utilities.GetValidSubFolder(Subtitles);
            var result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;
            Subtitles = dlg.FileName;
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
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.EnsurePathExists = true;
            dlg.InitialDirectory = Utilities.GetValidSubFolder(Library);
            var result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;
            Library = dlg.FileName;
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
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.EnsurePathExists = true;
            var result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;
            Logger.ZipAndCopyLogTo(dlg.FileName);
        }

        private bool CanCopyLogFileToExecute()
        {
            return true;
        }

        public ICommand CopyLogFileToCommand { get { return new RelayCommand(CopyLogFileToExecute, CanCopyLogFileToExecute); } }

        #endregion

        #region Open Settings Folder Command

        private void OpenSettingsFolderExecute()
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = Utilities.GetJavLuvSettingsFolder(),
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private bool CanOpenSettingsFolderExecute()
        {
            return true;
        }

        public ICommand OpenSettingsFolderCommand { get { return new RelayCommand(OpenSettingsFolderExecute, CanOpenSettingsFolderExecute); } }

        #endregion

        #endregion

        #region Private Members

        private MainWindowViewModel m_parent;
        private LanguageTypePair m_selectedLanguage;
        private ThemeTypePair m_selectedTheme;

        #endregion
    }
}
