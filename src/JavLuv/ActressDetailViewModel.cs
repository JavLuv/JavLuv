using Common;
using MovieInfo;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
            Parent.Parent.Collection.MovieSearchActress = m_actressData.Name;
            Parent.Parent.Collection.SearchMovies();
        }

        #endregion

        #region Event Handlers

        private void OnImageFinishedLoading(object sender, System.EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]));
            m_loadImage = null;
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
                    Parent.Parent.Collection.RemoveActress(m_actressData);
                    m_actressData.Name = value;
                    NotifyPropertyChanged("Name");
                    Parent.Parent.Collection.AddActress(m_actressData);
                    Parent.Parent.Collection.UpdateActressNames();
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
                    Parent.Parent.Collection.UpdateActressNames();
                }
            }
        }

        public string DateOfBirth
        {
            get 
            {
                return Utilities.DateTimeToString(m_actressData.DateOfBirth);
            }
            set
            {
                var newDateTime = Utilities.StringToDateTime(value);
                if (newDateTime != m_actressData.DateOfBirth && newDateTime != new DateTime())
                {
                    m_actressData.DateOfBirth = newDateTime;
                    NotifyPropertyChanged("DateOfBirth");
                }      
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
                if (String.IsNullOrEmpty(value))
                    return;
                string[] values = value.Split('-');
                if (values.Length < 3)
                    return;
                int bust = 0;
                int waist = 0;
                int hips = 0;
                int.TryParse(values[0], out bust);
                int.TryParse(values[1], out waist);
                int.TryParse(values[2], out hips);
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
        
        public string Age
        {
            get
            {
                if (m_actressData.DateOfBirth == new DateTime())
                    return String.Empty;

                // Calculate age - a little trickier than you'd expect.  
                // Still not 100% precise, but good enough in 99.999% of cases.
                DateTime zeroTime = new DateTime(1, 1, 1);
                DateTime a = m_actressData.DateOfBirth;
                DateTime b = DateTime.Now;
                TimeSpan span = b - a;
                // Because we start at year 1 for the Gregorian
                // calendar, we must subtract a year here.
                int years = (zeroTime + span).Year - 1;
                return years.ToString();
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
                return 0;
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
            openFileDlg.InitialDirectory = Utilities.GetValidSubFolder(Settings.Get().LastFolder);
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
                    Settings.Get().LastFolder = Path.GetDirectoryName(openFileDlg.FileNames[0]);
            }
        }

        private bool CanImportImagesExecute()
        {
            return true;
        }

        public ICommand ImportImagesCommand { get { return new RelayCommand(ImportImagesExecute, CanImportImagesExecute); } }

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
                string path = Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]);
                m_loadImage = new CmdLoadImage(path, ImageSize.Full);
                m_loadImage.FinishedLoading += OnImageFinishedLoading;
                CommandQueue.ShortTask().Execute(m_loadImage, CommandOrder.First);
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

        #endregion
    }
}
