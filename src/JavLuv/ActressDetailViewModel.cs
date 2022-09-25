using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (String.IsNullOrEmpty(m_actressData.DetailImageFileName) == false)
            {
                m_loadImage = new CmdLoadImage(Path.Combine(Utilities.GetActressImageFolder(), m_actressData.DetailImageFileName));
                m_loadImage.FinishedLoading += LoadImage_FinishedLoading;
                CommandQueue.ShortTask().Execute(m_loadImage, CommandOrder.First);
            }
        }

        #endregion

        #region Event Handlers

        private void LoadImage_FinishedLoading(object sender, System.EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + Path.Combine(Utilities.GetActressImageFolder(), m_actressData.DetailImageFileName));
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
        /*
        public string AlternateNames
        {
            get { return m_movieData.Metadata.Premiered; }
            set
            {
                if (value != m_movieData.Metadata.Premiered)
                {
                    m_movieData.Metadata.Premiered = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Released");
                }
            }
        }

        public string Studio
        {
            get { return m_movieData.Metadata.Studio; }
            set
            {
                if (value != m_movieData.Metadata.Studio)
                {
                    m_movieData.Metadata.Studio = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Studio");
                }
            }
        }

        public string Label
        {
            get { return m_movieData.Metadata.Label; }
            set
            {
                if (value != m_movieData.Metadata.Label)
                {
                    m_movieData.Metadata.Label = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Label");
                }
            }
        }

        public string Director
        {
            get { return m_movieData.Metadata.Director; }
            set
            {
                if (value != m_movieData.Metadata.Director)
                {
                    m_movieData.Metadata.Director = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Director");
                }
            }
        }
        */
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
        /*
        public string Genres
        {
            get
            {
                return MovieUtils.GenresToString(m_movieData);
            }
            set
            {
                if (MovieUtils.StringToGenres(m_movieData, value))
                {
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Genres");
                }
            }
        }

        public string Actors
        {
            get
            {
                return MovieUtils.ActorsToString(m_movieData.Metadata.Actors);
            }
            set
            {
                var actors = m_movieData.Metadata.Actors;
                if (MovieUtils.StringToActors(value, ref actors))
                {
                    m_movieData.Metadata.Actors = actors;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Actors");
                }
            }
        }

        public string Resolution
        {
            get { return m_movieData.MovieResolution; }
            set
            {
                if (value != m_movieData.MovieResolution)
                {
                    m_movieData.MovieResolution = value;
                    NotifyPropertyChanged("Resolution");
                }
            }
        }

        public string Notes
        {
            get { return m_movieData.Metadata.Plot; }
            set
            {
                if (value != m_movieData.Metadata.Plot)
                {
                    m_movieData.Metadata.Plot = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Notes");
                }
            }
        }
        */
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
