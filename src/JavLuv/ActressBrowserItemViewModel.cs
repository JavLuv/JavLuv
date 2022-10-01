using Common;
using MovieInfo;
using System;
using System.IO;
using System.Text;
using System.Windows;
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

        public Visibility BirthdayVisibility
        {
            get
            {
                if (DateTime.Now.DayOfYear == m_actressData.DateOfBirth.DayOfYear)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public string DisplayTitle 
        { 
            get 
            { 
                return m_displayTitle; 
            } 
        }

        #endregion

        #region Commands

        #region Detail View Command

        private void DetailViewExecute()
        {
            Parent.OpenDetailView(this);
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
                m_loadImage.FinishedLoading -= OnImageFinishedLoading;
                m_loadImage = null;
            }
        }

        #endregion

        #region Private Functions

        private void CreateDisplayTitle()
        {
            m_displayTitle = m_actressData.Name;
            switch (Settings.Get().SortActressesBy)
            {
                case SortActressesBy.Name:
                    break;
                case SortActressesBy.Age_Youngest:
                case SortActressesBy.Age_Oldest:
                    if (m_actressData.DateOfBirth == new DateTime())
                        break;
                    int years = MovieUtils.GetAgeFromDateOfBorth(m_actressData.DateOfBirth);
                    m_displayTitle += "\nAge " + years.ToString();
                    break;
                case SortActressesBy.Birthday:
                    m_displayTitle += "\n" + m_actressData.DateOfBirth.ToString("M");
                    break;
                case SortActressesBy.MovieCount:
                    m_displayTitle += "\nMovies: " + m_actressData.MovieCount.ToString();
                    break;
                case SortActressesBy.UserRating:
                    m_displayTitle += "\n" + MovieUtils.UserRatingToStars(m_actressData.UserRating);
                    break;
            }
            NotifyPropertyChanged("DisplayTitle");
        }

        private void BeginLoadImage()
        {
            if (m_loadImage != null)
                return;
            if (m_actressData.ImageFileNames.Count != 0)
            {
                string path = Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]);
                m_loadImage = new CmdLoadImage(path, ImageSize.Thumbnail);
                m_loadImage.FinishedLoading += OnImageFinishedLoading;
                CommandQueue.ShortTask().Execute(m_loadImage);
            }
        }

        private void OnImageFinishedLoading(object sender, EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]));
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
        private CmdLoadImage m_loadImage;
        private ActressData m_actressData;
        private string m_displayTitle;

        #endregion
    }
}
