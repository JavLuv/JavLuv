using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Common;
using MovieInfo;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WebScraper
{
    abstract public class ModuleBase
    {
        #region Constructors

        public ModuleBase(Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language)
        {
            m_dispatcher = dispatcher;
            m_webBrowser = webBrowser;
            m_webBrowser.ParsingCompleted += OnParsingCompleted;
            m_language = language;
            ImageSource = String.Empty;
        }

        #endregion

        #region Event Handlers

        private void OnParsingCompleted(object sender, EventArgs e)
        {
            m_parsingComplete = true;
        }

        #endregion

        #region Properties

        public string ImageSource { get; protected set; }
        public bool DebugHtml { get; set; }

        #endregion

        #region Public Functions

        abstract public void Scrape();

        #endregion

        #region Protected Functions

        abstract protected bool IsLanguageSupported();

        abstract protected bool IsValidDataParsed();

        abstract protected void ParseDocument(IHtmlDocument document);

        protected void ScrapeWebsite(string rootURL, string siteURL)
        {
            try
            {
                Logger.WriteInfo("Scraping website for data: " + siteURL);

                if (m_dispatcher.HasShutdownStarted == false)
                    _ = m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                    {
                        m_webBrowser.RootSite = rootURL;
                        m_webBrowser.Address = siteURL;
                        _ = m_webBrowser.LoadSite();
                    }));

                // Pause until parsing is complete or timeout
                int timeoutCount = 0;
                while (m_parsingComplete == false && timeoutCount <= 50)
                {
                    ++timeoutCount;
                    Thread.Sleep(100);
                }

                if (m_parsingComplete)
                {
                    try
                    {
                        int parseTryCount = 0;
                        while (IsValidDataParsed() == false)
                        {
                            IHtmlDocument document = null;
                            if (m_dispatcher.HasShutdownStarted == false)
                                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(async delegate ()
                                {
                                    await m_webBrowser.ParseSite();
                                    document = m_webBrowser.HtmlDocument;
                                }));
                            int waitTimeCount = 0;
                            while (document == null && waitTimeCount <= 50)
                            {
                                ++waitTimeCount;
                                Thread.Sleep(100);
                            }
                            if (document != null)
                            {
                                if (DebugHtml)
                                    DebugHTML(document);
                                ParseDocument(document);
                                Thread.Sleep(100);
                            }
                            parseTryCount++;
                            if (parseTryCount == 20)
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError("Issue parsing website HTML " + ex);
                    }
                }

                timeoutCount = 0;
            }
            catch (Exception ex)
            {
                Logger.WriteError("Issue scraping website: " + siteURL, ex);
            }
        }

        protected void DebugHTML(IHtmlDocument document)
        {
            var sw = new StringWriter();
            document.ToHtml(sw, new PrettyMarkupFormatter());
            var htmlPretty = sw.ToString();
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string className = this.GetType().Name;
            string fullPath = Path.Combine(desktopPath, className + ".html");
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.Write(htmlPretty);
            }
        }

        protected string GetLanguageString()
        {
            if (m_language == LanguageType.Japanese)
                return "ja";
            return "en";
        }

        #endregion

        #region Protected and Private Members

        private Dispatcher m_dispatcher;
        private WebBrowser m_webBrowser;
        protected LanguageType m_language;
        private bool m_parsingComplete = false;
        #endregion
    }
}
