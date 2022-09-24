using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace JavLuv
{
    public class ActressBrowserItemViewModel : ObservableObject
    {
        #region Constructors

        public ActressBrowserItemViewModel(ActressBrowserViewModel parent, ActressData actressData)
        {
            Parent = parent;
            m_actressData = actressData;
            m_imageFileName = String.Empty;
            CreateDisplayTitle();
        }

        #endregion

        #region Properties

        public ActressBrowserViewModel Parent { get; private set; }

        public ActressData ActressData { get { return m_actressData; } }

        public ImageSource Image
        {
            get
            {
                return m_image;
            }
            private set
            {
                if (value != m_image)
                {
                    m_image = value;
                    NotifyPropertyChanged("Image");
                }
            }
        }

        public string DisplayTitle { get { return m_displayTitle; } }

        #endregion

        #region Commands

        #region Detail View Command

        private void DetailViewExecute()
        {
            //Parent.OpenDetailView(this);
        }

        private bool CanDetailViewExecute()
        {
            return true;
        }

        public ICommand DetailViewCommand { get { return new RelayCommand(DetailViewExecute, CanDetailViewExecute); } }

        #endregion

        #endregion

        #region Public Functions

        #endregion

        #region Protected Functions

        protected override void OnShow()
        {
            base.OnShow();
            if (Image == null)
                BeginLoadImage();
        }

        protected override void OnHide()
        {
            base.OnHide();
            Image = null;
            if (m_loadImage != null)
            {
                m_loadImage.Cancel = true;
                m_loadImage.FinishedLoading -= LoadImage_FinishedLoading;
                m_loadImage = null;
            }
        }

        #endregion

        #region Private Functions

        private void CreateDisplayTitle()
        {
            m_displayTitle = m_actressData.Name;
        }

        private void BeginLoadImage()
        {
            if (m_loadImage != null)
                return;
            if (m_actressData.Images.Count == 0)
                return;
            m_imageFileName = m_actressData.Images[0];
            if (String.IsNullOrEmpty(m_actressData.ThumbnailImage) == false)
                m_imageFileName = m_actressData.ThumbnailImage;
            string path = Path.Combine(Utilities.GetActressImageFolder(), m_imageFileName);
            m_loadImage = new CmdLoadImage(path, ImageSize.Thumbnail);
            m_loadImage.FinishedLoading += LoadImage_FinishedLoading;
            CommandQueue.ShortTask().Execute(m_loadImage);
        }

        private void LoadImage_FinishedLoading(object sender, EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + m_imageFileName);
            m_loadImage = null;
        }

        private string UserRatingToStars(int userRating)
        {
            if (userRating == 0)
                return "unrated";
            StringBuilder sb = new StringBuilder(10);
            while (userRating >= 2)
            {
                sb.Append("\u2605");
                userRating -= 2;
            }
            if (userRating != 0)
                sb.Append("½");
            return sb.ToString();
        }

        #endregion

        #region Private Members

        private ImageSource m_image;
        private string m_imageFileName;
        private CmdLoadImage m_loadImage;
        private ActressData m_actressData;
        private string m_displayTitle;

        #endregion
    }
}
