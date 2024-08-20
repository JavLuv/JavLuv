using AngleSharp.Html;
using AngleSharp.Html.Dom;
using Common;
using System;
using System.IO;
using System.Text;
using System.Threading;
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

        public bool SearchNotFound { get; protected set; }

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
            Logger.WriteInfo("Scraping website for data: " + siteURL);

            bool parseError = false;
            const int browserRetries = 3;

            // Load-retry loop
            int loadCounter = 0;
            do
            {
                try
                {
                    if (m_dispatcher.HasShutdownStarted == false)
                        _ = m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                        {
                            Logger.WriteInfo("Loading site");
                            m_webBrowser.RootSite = rootURL;
                            m_webBrowser.Address = siteURL;
                            _ = m_webBrowser.LoadSite();
                        }));
                }
                catch (Exception ex)
                {
                    Logger.WriteError("Webbrowser loading exception: ", ex);
                    parseError = true;
                    break;
                }

                // Pause until parsing is complete or timeout
                int timeoutCount = 0;
                while (m_parsingComplete == false && timeoutCount <= 60)
                {
                    ++timeoutCount;
                    Thread.Sleep(100);
                }

                if (m_parsingComplete)
                {
                    int parseTryCount = 0;
                    do
                    {
                        IHtmlDocument document = null;
                        try
                        {
                            if (m_dispatcher.HasShutdownStarted == false)
                                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(async delegate ()
                                {
                                    await m_webBrowser.ParseSite();
                                    document = m_webBrowser.HtmlDocument;
                                }));
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteError("Issue with internal browser parsing siteL " + ex);
                            parseError = true;
                            break;
                        }
                        int waitTimeCount = 0;
                        while (document == null && waitTimeCount <= 60)
                        {
                            ++waitTimeCount;
                            Thread.Sleep(100);
                        }
                        if (document != null)
                        {
                            if (DebugHtml)
                                DebugHTML(document);
                            try
                            {
                                ParseDocument(document);
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteError("HTML document parsing exception: ", ex);
                                parseError = true;
                                break;
                            }
                            if (IsValidDataParsed() == false)
                                Thread.Sleep(100);
                        }
                        parseTryCount++;
                        if (parseTryCount >= 20)
                        {
                            Logger.WriteWarning("HTML parsing timeout.  Retrying.");
                            break;
                        }
                    }
                    while (parseError == false && IsValidDataParsed() == false);
                }
                
                // Since we might have broken out of the loop on timeout, check again
                // to see if we've succeessfully parsed an HTML document.
                if (IsValidDataParsed() == false)
                {
                    if (loadCounter <= browserRetries)
                        Logger.WriteWarning("Internal browser parsing timeout.  Retrying.");
                }
                loadCounter++;

                // Reset parsing complete flag
                m_parsingComplete = false;
            }
            while (parseError == false && m_parsingComplete == false && IsValidDataParsed() == false && loadCounter <= browserRetries);

            // Log parsing results
            if (IsValidDataParsed() && parseError == false)
                Logger.WriteInfo("Successfully finished parsing site");
            else
                Logger.WriteWarning("Incomplete site parsing");
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
        protected bool m_parsingSuccessful = false;

        #endregion
    }
}
