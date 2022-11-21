using Common;
using MovieInfo;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace JavLuv
{
    public class MovieDetailActressItemViewModel : ObservableObject
    {
        #region Constructor

        public MovieDetailActressItemViewModel(MovieDetailActressViewModel parent, ActorData actor)
        {
            Parent = parent;
            m_actressData = Parent.Parent.Parent.Parent.Collection.FindActress(actor.Name);
            Name = m_actressData.Name;
            if (m_actressData.ImageFileNames.Count > 0)
            {
                string path = Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]);
                m_loadImage = new CmdLoadImage(path, ImageSize.Thumbnail);
                m_loadImage.FinishedLoading += OnImageFinishedLoading;
                CommandQueue.ShortTask().Execute(m_loadImage, CommandOrder.First);       
            }
        }

        private void OnImageFinishedLoading(object sender, EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]));
            m_loadImage.FinishedLoading -= OnImageFinishedLoading;
            m_loadImage = null;
        }

        #endregion

        #region Properties

        public MovieDetailActressViewModel Parent { get; private set; }

        public bool HasImage
        {
            get { return (m_actressData.ImageFileNames.Count > 0) ? true : false; }
        }

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
                if (Image == null || SuppressVisibility)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool SuppressVisibility { get; set; }

        public string Name { get; private set; }

        public int MovieCount
        {
            get
            {
                return m_actressData.MovieCount;
            }
        }

        #endregion

        #region Commands

        #region View Actress Command

        private void ViewActressExecute()
        {
            Parent.Parent.Parent.Parent.Overlay = new ActressDetailViewModel(this, m_actressData);
        }

        private bool CanViewActressExecute()
        {
            return true;
        }

        public ICommand ViewActressCommand { get { return new RelayCommand(ViewActressExecute, CanViewActressExecute); } }

        #endregion

        #endregion

        #region Private Members

        private ActressData m_actressData;
        private ImageSource m_image;
        private CmdLoadImage m_loadImage;

        #endregion
    }
}
