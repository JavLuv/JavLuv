using Common;
using MovieInfo;
using System;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace JavLuv
{
    public class MovieBrowserItemViewModel : ObservableObject
    {
        #region Constructors

        public MovieBrowserItemViewModel(MovieBrowserViewModel parent, MovieData movieData)
        {
            Parent = parent;
            m_movieData = movieData;
            CreateDisplayTitle();
        }

        #endregion

        #region Properties

        public MovieBrowserViewModel Parent { get; private set; }

        public MovieData MovieData { get { return m_movieData; } }

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

        public string ID { get { return m_movieData.Metadata.UniqueID.Value; } }

        public string DisplayTitle { get { return m_displayTitle; } }

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

        public void ImportCoverImage()
        {
            if (Parent.ImportCoverImage(m_movieData))
            {
                BeginLoadImage();
            }
        }

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

        private string GetTitleWithOptions()
        {
            StringBuilder sb = new StringBuilder(200);
            if (Settings.Get().ShowID)
                sb.AppendFormat("[{0}] ", m_movieData.Metadata.UniqueID.Value);
            if (Settings.Get().ShowUserRating)
                sb.AppendFormat("({0}) ", MovieUtils.UserRatingToStars(m_movieData.Metadata.UserRating));
            sb.Append(m_movieData.Metadata.Title);
            return sb.ToString();
        }

        private void CreateDisplayTitle()
        {
            switch (Settings.Get().SortMoviesBy)
            {
                case SortMoviesBy.Title:
                    m_displayTitle = GetTitleWithOptions();
                    break;
                case SortMoviesBy.ID:
                    if (Settings.Get().ShowID)
                        m_displayTitle = GetTitleWithOptions();
                    else
                        m_displayTitle = String.Format("[{0}] {1}", m_movieData.Metadata.UniqueID.Value, GetTitleWithOptions());
                    break;
                case SortMoviesBy.Actress:
                    if (m_movieData.Metadata.Actors.Count == 0)
                    {
                        m_displayTitle = String.Format("({0}) {1}", "???", GetTitleWithOptions());
                    }
                    else if (m_movieData.Metadata.Actors.Count == 1)
                    {
                        bool useJpNameOrder = Settings.Get().UseJapaneseNameOrder;
                        string displayName = MovieUtils.GetDisplayActressName(m_movieData.Metadata.Actors[0].Name, useJpNameOrder);
                        m_displayTitle = String.Format("({0}) {1}", displayName, GetTitleWithOptions());
                    }
                    else if (m_movieData.Metadata.Actors.Count == 2)
                    {
                        bool useJpNameOrder = Settings.Get().UseJapaneseNameOrder;
                        string displayName1 = MovieUtils.GetDisplayActressName(m_movieData.Metadata.Actors[0].Name, useJpNameOrder);
                        string displayName2 = MovieUtils.GetDisplayActressName(m_movieData.Metadata.Actors[1].Name, useJpNameOrder);
                        m_displayTitle = String.Format("({0} & {1}) {2}", displayName1, displayName2, GetTitleWithOptions());
                    }
                    else
                    {
                        bool useJpNameOrder = Settings.Get().UseJapaneseNameOrder;
                        string displayName1 = MovieUtils.GetDisplayActressName(m_movieData.Metadata.Actors[0].Name, useJpNameOrder);
                        string displayName2 = MovieUtils.GetDisplayActressName(m_movieData.Metadata.Actors[1].Name, useJpNameOrder);
                        m_displayTitle = String.Format("({0}, {1}, & more...) {2}", displayName1, displayName2, GetTitleWithOptions());
                    }
                    break;
                case SortMoviesBy.Date_Newest:
                    goto case SortMoviesBy.Date_Oldest;
                case SortMoviesBy.Date_Oldest:
                    string datePremiered = m_movieData.Metadata.Premiered;
                    if (String.IsNullOrEmpty(datePremiered))
                        datePremiered = "Unknown";
                    m_displayTitle = String.Format("({0}) {1}", datePremiered, GetTitleWithOptions());
                    break;
                case SortMoviesBy.Random:
                    m_displayTitle = GetTitleWithOptions();
                    break;
                case SortMoviesBy.Resolution:
                    string resolution = MovieUtils.GetMovieResolution(m_movieData.Metadata);
                    if (String.IsNullOrEmpty(resolution))
                        resolution = "Unknown";
                    m_displayTitle = String.Format("({0}) {1}", resolution, GetTitleWithOptions());
                    break;
                case SortMoviesBy.RecentlyAdded:
                    string dateAdded = m_movieData.Metadata.DateAdded;
                    if (String.IsNullOrEmpty(dateAdded))
                        dateAdded = "Unknown";
                    m_displayTitle = String.Format("({0}) {1}", dateAdded, GetTitleWithOptions());
                    break;
                case SortMoviesBy.UserRating:
                    if (Settings.Get().ShowUserRating)
                        m_displayTitle = GetTitleWithOptions();
                    else
                        m_displayTitle = String.Format("({0}) {1}", MovieUtils.UserRatingToStars(m_movieData.Metadata.UserRating), GetTitleWithOptions());
                    break;
            }
        }

        private void BeginLoadImage()
        {
            if (m_loadImage != null)
                return;
            if (String.IsNullOrEmpty(m_movieData.Path) || String.IsNullOrEmpty(m_movieData.CoverFileName))
                return;
            string path = Path.Combine(m_movieData.Path, m_movieData.CoverFileName);
            m_loadImage = new CmdLoadImage(path, ImageSize.Thumbnail);
            m_loadImage.FinishedLoading += LoadImage_FinishedLoading;
            CommandQueue.ShortTask().Execute(m_loadImage);
        }

        private void LoadImage_FinishedLoading(object sender, EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + Path.Combine(m_movieData.Path, m_movieData.CoverFileName));
            m_loadImage = null;
        }

        #endregion

        #region Private Members

        private ImageSource m_image;
        private CmdLoadImage m_loadImage;
        private MovieData m_movieData;
        private string m_displayTitle;

        #endregion
    }
}
