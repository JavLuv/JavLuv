using Common;
using MovieInfo;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JavLuv
{
    public class ActressDetailViewModel : ObservableObject
    {
        #region Constructors

        public ActressDetailViewModel(ActressBrowserViewModel parent, ActressBrowserItemViewModel browserItem)
        {
            Logger.WriteInfo("Viewing details of actress " + browserItem.ActressData.Name);
            m_parent = parent;
            m_browserItem = browserItem;
            m_actressData = m_browserItem.ActressData;
            LoadCurrentImage();
            Parent.Parent.Collection.MovieSearchActress = m_actressData;
            Parent.Parent.Collection.MoviesDisplayedChanged += OnMoviesDisplayedChanged;
            Parent.Parent.Collection.SearchMovies();
        }

        public ActressDetailViewModel(ActressDetailViewModel actressDetailViewModel) : 
            this(actressDetailViewModel.m_parent, actressDetailViewModel.m_browserItem)
        {
        }

        #endregion

        #region Event Handlers

        private void OnMoviesDisplayedChanged(object sender, EventArgs e)
        {
            AverageMovieRating = Parent.Parent.Collection.AverageMovieRating;
            Parent.Parent.Collection.MoviesDisplayedChanged -= OnMoviesDisplayedChanged;
        }

        private void OnImageFinishedLoading(object sender, System.EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]));
            m_loadImage.FinishedLoading -= OnImageFinishedLoading;
            m_loadImage = null;
            if (m_pendingImageLoad)
            {
                LoadCurrentImage();
                m_pendingImageLoad = false;
            }
        }

        #endregion

        #region Properties

        public ActressBrowserViewModel Parent { get { return m_parent; } }

        public ImageSource Image
        {
            get { return m_image; }
            set
            {
                if (value != m_image)
                {
                    m_image = value;
                    NotifyPropertyChanged("Image");
                    NotifyPropertyChanged("ImageVisibility");
                }
            }
        }

        public Visibility ImageVisibility
        {
            get
            {
                if (Image == null)
                    return Visibility.Hidden;
                return Visibility.Visible;
            }
        }

        public Visibility LeftNavArrowVisibility
        {
            get
            {
                if (Image == null || m_actressData.ImageFileNames.Count < 2)
                    return Visibility.Hidden;
                return m_leftNavArrowVisibility;
            }
            set
            {
                if (value != m_leftNavArrowVisibility)
                {
                    m_leftNavArrowVisibility = value;
                    NotifyPropertyChanged("LeftNavArrowVisibility");
                }
            }
        }

        public Visibility RightNavArrowVisibility
        {
            get
            {
                if (Image == null || m_actressData.ImageFileNames.Count < 2)
                    return Visibility.Hidden;
                return m_rightNavArrowVisibility;
            }
            set
            {
                if (value != m_rightNavArrowVisibility)
                {
                    m_rightNavArrowVisibility = value;
                    NotifyPropertyChanged("RightNavArrowVisibility");
                }
            }
        }

        public ActressBrowserItemViewModel BrowserItem { get { return m_browserItem; } }

        public ActressData ActressData { get { return m_actressData; } }

        public bool NameChanged { get; private set; }

        public string Name
        {
            get
            {
                 return m_actressData.Name;
            }
            set
            {
                if (value != m_actressData.Name)
                {
                    if (String.IsNullOrEmpty(value))
                    {
                        MessageBox.Show(
                            TextManager.GetString("Text.ActressEmptyNameMessage"),
                            TextManager.GetString("Text.ActressRenameError")
                            );
                        return;
                    }
                    ActressData actress = Parent.Parent.Collection.FindActress(value);
                    if (actress != null && Name != actress.Name)
                    {
                        MessageBox.Show(
                            String.Format(TextManager.GetString("Text.ActressRenameMessage"), actress.Name),
                            TextManager.GetString("Text.ActressRenameError")
                            );
                        return;
                    }
                    if (Utilities.Equals(m_actressData.Name, m_actressData.AltNames) == false)
                        m_actressData.AltNames.Add(m_actressData.Name);
                    var index = m_actressData.AltNames.FindIndex(x => x == value);
                    if (index != -1)
                        m_actressData.AltNames.RemoveAt(index);
                    m_actressData.Name = value;
                    NotifyPropertyChanged("Name");
                    Parent.Parent.Collection.RenameActress(m_actressData);
                }
            }
        }

        public string JapaneseName
        {
            get
            {
                return m_actressData.JapaneseName;
            }
            set
            {
                if (value != m_actressData.JapaneseName)
                {
                    m_actressData.JapaneseName = value;
                    NotifyPropertyChanged("JapaneseName");
                    Parent.Parent.Collection.RenameActress(m_actressData);
                }
            }
        }

        public string AlternateNames
        {
            get { return Utilities.StringListToString(m_actressData.AltNames); }
            set
            {
                var names = value;
                var currentNames = Utilities.StringListToString(m_actressData.AltNames);
                if (value != currentNames)
                {
                    var altNames = Utilities.StringToStringList(value);
                    foreach (var name in altNames)
                    {
                        ActressData actress = Parent.Parent.Collection.FindActress(value);
                        if (actress != null && Name != actress.Name)
                        {
                            MessageBox.Show(
                                String.Format(TextManager.GetString("Text.ActressRenameMessage"), actress.Name), 
                                TextManager.GetString("Text.ActressRenameError")
                                );
                            return;
                        }
                    }
                    m_actressData.AltNames = altNames;
                    NotifyPropertyChanged("AlternateNames");
                    Parent.Parent.Collection.RenameActress(m_actressData);
                }
            }
        }

        public string DateOfBirth
        {
            get 
            {
                if (m_actressData.DobYear == 0 && m_actressData.DobMonth == 0 && m_actressData.DobDay == 0)
                    return String.Empty;
                return Utilities.DateTimeToString(m_actressData.DobYear, m_actressData.DobMonth, m_actressData.DobDay);
            }
            set
            {
                var dateTimeStr = Utilities.DateTimeToString(m_actressData.DobYear, m_actressData.DobMonth, m_actressData.DobDay);
                if (value != dateTimeStr)
                {
                    int year = 0;
                    int month = 0;
                    int day = 0;
                    try
                    {
                        Utilities.StringToDateTime(value, out year, out month, out day);
                        m_actressData.DobYear = year;
                        m_actressData.DobMonth = month;
                        m_actressData.DobDay = day;
                        NotifyPropertyChanged("DateOfBirth");
                        NotifyPropertyChanged("Age");
                    }
                    catch (Exception)
                    {
                        Logger.WriteInfo("User entered invalid actress date");
                    }
                }      
            }
        }
        public string Age
        {
            get
            {
                if (m_actressData.DobYear == 0)
                    return String.Empty;
                int years = MovieUtils.GetAgeFromDateOfBirth(m_actressData.DobYear, m_actressData.DobMonth, m_actressData.DobDay);
                return years.ToString();
            }
        }

        public string Height
        {
            get 
            {
                if (m_actressData.Height < 50)
                    return String.Empty;
                return String.Format(TextManager.GetString("Text.ActressHeightValue"), m_actressData.Height); 
            }
            set
            {
                string currentVal = String.Format(TextManager.GetString("Text.ActressHeightValue"), m_actressData.Height);
                if (value != currentVal)
                {
                    m_actressData.Height = Utilities.ParseInitialDigits(value);
                    NotifyPropertyChanged("Height");
                    NotifyPropertyChanged("ImperialHeight");
                }
            }
        }

        public string ImperialHeight
        {
            get
            {
                return Utilities.CentimetersToFeetAndInchesString(m_actressData.Height);
            }
        }

        public string Cup
        {
            get { return m_actressData.Cup; }
            set
            {
                if (value != m_actressData.Cup)
                {
                    m_actressData.Cup = value;
                    NotifyPropertyChanged("Cup");
                }
            }
        }

        public string Measurements
        {
            get 
            {
                if (m_actressData.Bust == 0 || m_actressData.Waist == 0 || m_actressData.Hips == 0)
                        return String.Empty;
                return String.Format("{0}-{1}-{2}", m_actressData.Bust, m_actressData.Waist, m_actressData.Hips); 
            }
            set
            {
                int bust = 0;
                int waist = 0;
                int hips = 0;
                string[] values = value.Split('-');
                if (values.Length == 3)
                {
                    int.TryParse(values[0], out bust);
                    int.TryParse(values[1], out waist);
                    int.TryParse(values[2], out hips);
                }
                if (bust != m_actressData.Bust || waist != m_actressData.Waist || hips != m_actressData.Hips)
                {
                    m_actressData.Bust = bust;
                    m_actressData.Waist = waist;
                    m_actressData.Hips = hips;
                    NotifyPropertyChanged("Measurements");
                }
            }
        }

        public int UserRating
        {
            get { return m_actressData.UserRating; }
            set
            {
                if (value != m_actressData.UserRating)
                {
                    m_actressData.UserRating = value;                 
                    NotifyPropertyChanged("UserRating");
                }
            }
        }

        public string Notes
        {
            get { return m_actressData.Notes; }
            set
            {
                if (value != m_actressData.Notes)
                {
                    m_actressData.Notes = value;
                    NotifyPropertyChanged("Notes");
                }
            }
        }
        
        public string MoviesInCollection
        {
            get
            {
                return String.Empty;                
            }
        }

        public int AverageMovieRating
        {
            get
            {
                return m_averageMovieCollection;
            }
            set
            {
                if (value != m_averageMovieCollection)
                {
                    m_averageMovieCollection = value;
                    NotifyPropertyChanged("AverageMovieRating");
                }
            }
        }

        #endregion

        #region Commands

        #region Next Image Command

        private void NextImageExecute()
        {
            m_actressData.ImageIndex = ++m_actressData.ImageIndex % m_actressData.ImageFileNames.Count;
            LoadCurrentImage();
        }

        private bool CanNextImageExecute()
        {
            return m_actressData.ImageFileNames.Count > 1;
        }

        public ICommand NextImageCommand { get { return new RelayCommand(NextImageExecute, CanNextImageExecute); } }

        #endregion

        #region Previous Image Command

        private void PreviousImageExecute()
        {
            m_actressData.ImageIndex = (--m_actressData.ImageIndex + m_actressData.ImageFileNames.Count) % m_actressData.ImageFileNames.Count;
            LoadCurrentImage();
        }

        private bool CanPreviousImageExecute()
        {
            return m_actressData.ImageFileNames.Count > 1;
        }

        public ICommand PreviousImageCommand { get { return new RelayCommand(PreviousImageExecute, CanPreviousImageExecute); } }

        #endregion

        #region Import Image Command

        private void ImportImagesExecute()
        {
            var openFileDlg = new System.Windows.Forms.OpenFileDialog();
            openFileDlg.Filter = Utilities.GetImagesFileFilter();
            openFileDlg.CheckFileExists = true;
            openFileDlg.CheckPathExists = true;
            openFileDlg.Multiselect = true;
            openFileDlg.InitialDirectory = Utilities.GetValidSubFolder(Settings.Get().ImageImportFolder);
            var results = openFileDlg.ShowDialog();
            if (results == System.Windows.Forms.DialogResult.OK)
            {
                string actressImagefolder = Utilities.GetActressImageFolder();
                foreach (string filename in openFileDlg.FileNames)
                {
                    string ext = Path.GetExtension(filename);
                    string actressFileName = Path.ChangeExtension(Guid.NewGuid().ToString(), ext);
                    string actressFullPath = Path.Combine(actressImagefolder, actressFileName);
                    try
                    {
                        File.Copy(filename, actressFullPath);
                        m_actressData.ImageFileNames.Add(actressFileName);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError("Unable to import actress image", ex);
                    }
                }

                // Remove any potential duplicate filenames
                m_actressData.ImageFileNames = Utilities.DeleteDuplicateFiles(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames);

                if (m_actressData.ImageFileNames.Count > 0)
                {
                    // Pick the last image imported as the new current image
                    m_actressData.ImageIndex = m_actressData.ImageFileNames.Count - 1;
                    LoadCurrentImage();
                }
                if (openFileDlg.FileNames.Count() > 0)
                    Settings.Get().ImageImportFolder = Path.GetDirectoryName(openFileDlg.FileNames[0]);
            }
        }

        private bool CanImportImagesExecute()
        {
            return true;
        }

        public ICommand ImportImagesCommand { get { return new RelayCommand(ImportImagesExecute, CanImportImagesExecute); } }

        #endregion

        #region Paste Command

        private void PasteExecute()
        {
            var image = Clipboard.GetImage() as BitmapSource;
            string fileName = Path.ChangeExtension(Guid.NewGuid().ToString(), ".png");
            string fileNamePath = Path.Combine(Utilities.GetActressImageFolder(), fileName);
            using (FileStream stream = new FileStream(fileNamePath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Interlace = PngInterlaceOption.Off;
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
            }
            m_actressData.ImageFileNames.Add(fileName);
            m_actressData.ImageFileNames = Utilities.DeleteDuplicateFiles(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames);
            m_actressData.ImageIndex = m_actressData.ImageFileNames.Count - 1;
            LoadCurrentImage();
        }

        private bool CanPasteExecute()
        {
            return Clipboard.ContainsImage();
        }

        public ICommand PasteCommand { get { return new RelayCommand(PasteExecute, CanPasteExecute); } }

        #endregion

        #region Delete Image Command

        private void DeleteImageExecute()
        {
            var fileNameToDelete = m_actressData.ImageFileNames[m_actressData.ImageIndex];
            fileNameToDelete = Path.Combine(Utilities.GetActressImageFolder(), fileNameToDelete);
            try
            {
                File.Delete(fileNameToDelete);
                m_actressData.ImageFileNames.RemoveAt(m_actressData.ImageIndex);
                if (m_actressData.ImageIndex >= m_actressData.ImageFileNames.Count)
                    m_actressData.ImageIndex = 0;
                LoadCurrentImage();
            }
            catch (Exception ex)
            {
                Logger.WriteError("Could not delete actress image", ex);
            }
        }

        private bool CanDeleteImageExecute()
        {
            return m_actressData.ImageFileNames.Count() > 0;
        }

        public ICommand DeleteImageCommand { get { return new RelayCommand(DeleteImageExecute, CanDeleteImageExecute); } }

        #endregion

        #endregion

        #region Private Functions

        private void LoadCurrentImage()
        {
            if (m_actressData.ImageFileNames.Count > 0)
            {
                if (m_loadImage == null)
                {
                    string path = Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]);
                    m_loadImage = new CmdLoadImage(path, ImageSize.Full);
                    m_loadImage.FinishedLoading += OnImageFinishedLoading;
                    CommandQueue.ShortTask().Execute(m_loadImage, CommandOrder.First);
                }
                else
                {
                    m_pendingImageLoad = true;
                }
            }
            else
            {
                Image = null;
            }
        }

        #endregion

        #region Private Members

        private ActressBrowserViewModel m_parent;
        private ActressData m_actressData;
        private ActressBrowserItemViewModel m_browserItem;
        private ImageSource m_image;
        private CmdLoadImage m_loadImage;
        private int m_averageMovieCollection;
        private bool m_pendingImageLoad;
        private Visibility m_leftNavArrowVisibility = Visibility.Hidden;
        private Visibility m_rightNavArrowVisibility = Visibility.Hidden;

        #endregion
    }
}
