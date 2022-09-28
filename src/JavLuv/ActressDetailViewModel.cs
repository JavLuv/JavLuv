using Common;
using MovieInfo;
using System;
using System.IO;
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
            if (m_actressData.ImageFileNames.Count > 0)
            {
                string path = Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]);
                m_loadImage = new CmdLoadImage(path, ImageSize.Full);
                m_loadImage.FinishedLoading += OnImageFinishedLoading;
                CommandQueue.ShortTask().Execute(m_loadImage, CommandOrder.First);
            }

            Parent.Parent.Collection.SearchMoviesByActress(m_actressData.Name);
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
                }
            }
        }

        public ActressBrowserItemViewModel BrowserItem { get { return m_browserItem; } }

        public ActressData ActressData { get { return m_actressData; } }

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
                    m_actressData.Name = value;
                    NotifyPropertyChanged("Name");
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

        #region Import Cover Image Command

        private void ImportCoverImageExecute()
        {
            /*
            if (Parent.ImportCoverImage(m_movieData))
            {
                m_loadImage = new CmdLoadImage(Path.Combine(m_movieData.Path, m_movieData.CoverFileName));
                m_loadImage.FinishedLoading += LoadImage_FinishedLoading;
                CommandQueue.ShortTask().Execute(m_loadImage, CommandOrder.First);
            }
            */
        }

        private bool CanImportCoverImageExecute()
        {
            return true;
        }

        public ICommand ImportCoverImageCommand { get { return new RelayCommand(ImportCoverImageExecute, CanImportCoverImageExecute); } }

        #endregion

        #region Copy Title And Metadata Command

        private void CopyTitleAndMetadataExecute()
        {
            string text = String.Empty;
            /*
            text += "[" + ID + "] ";
            text += Title;
            text += "\n\n";
            text += "ID: " + ID + "\n";
            text += "Released: " + Released + "\n";
            text += "Runtime: " + Runtime + "\n";
            text += "Studio: " + Studio + "\n";
            text += "Label: " + Label + "\n";
            text += "Director: " + Director + "\n";
            text += "Genres: " + Genres + "\n";
            text += "Actresses: " + Actors + "\n\n";
            */
            Clipboard.SetText(text);
        }

        private bool CanCopyTitleAndMetadataExecute()
        {
            return true;
        }

        public ICommand CopyTitleAndMetadataCommand { get { return new RelayCommand(CopyTitleAndMetadataExecute, CanCopyTitleAndMetadataExecute); } }


        #endregion

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
