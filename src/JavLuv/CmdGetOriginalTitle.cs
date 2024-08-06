using MovieInfo;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WebScraper;

namespace JavLuv
{
    public class CmdGetOriginalTitle : IAsyncCommand
    {
        #region Constructors

        public CmdGetOriginalTitle(string movieID, MovieData movieData)
        {
            m_movieID = movieID;
            m_movieData = movieData;
            m_startTime = DateTime.Now;
        }

        #endregion

        #region Events

        public event EventHandler FinishedScraping;

        #endregion

        #region Properties

        public string Resolution { get; set; }

        #endregion

        #region Public Functions

        public void Execute()
        {
            // Check for early out conditions
            if (Application.Current == null ||Application.Current.Dispatcher == null)
                return;     
            var now = DateTime.Now;
            if (now - m_startTime > TimeSpan.FromSeconds(30))
                return;

            // Retrieve on the main thread to ensure thread-safety
            string originalTitle = String.Empty;
            MainWindow mainWindow = null;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                originalTitle = m_movieData.Metadata.OriginalTitle;
                mainWindow = Application.Current.MainWindow as MainWindow;
            }));
            if (String.IsNullOrEmpty(m_movieData.Metadata.OriginalTitle) == false)
                return;

            // Scrape the web for Japanese language title
            var scraper = new Scraper(Application.Current.Dispatcher, mainWindow.webView);
            originalTitle = scraper.ScrapeOriginalTitle(m_movieID);

            // If we've failed to get an original title, just add a space.  This will prevent
            // JavLuv from attempting to scrape it again in the future.
            if (String.IsNullOrEmpty(originalTitle))
                originalTitle = " ";

            // Set new data on the main thread to ensure thread-safety
            if (Application.Current == null || Application.Current.Dispatcher == null)
                return;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                m_movieData.Metadata.OriginalTitle = originalTitle;
                m_movieData.MetadataChanged = true;
                FinishedScraping?.Invoke(this, new EventArgs());
            }));
        }

        #endregion

        #region Private Members

        private string m_movieID;
        private MovieData m_movieData;
        private DateTime m_startTime;

        #endregion
    }
}
