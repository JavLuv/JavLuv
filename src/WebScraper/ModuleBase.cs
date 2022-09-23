using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Common;
using MovieInfo;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebScraper
{
    abstract public class ModuleBase
    {
        #region Constructors

        public ModuleBase(LanguageType language)
        {
            m_language = language;
            ImageSource = String.Empty;
        }

        #endregion

        #region Properties

        public string ImageSource { get; protected set; }

        #endregion

        #region Public Functions

        abstract public void Scrape();

        #endregion

        #region Protected Functions

        abstract protected bool IsLanguageSupported();

        abstract protected void ParseDocument(IHtmlDocument document);

        protected async Task ScrapeAsync(string siteURL)
        {
            try
            {
                Logger.WriteInfo("Scraping website for data: " + siteURL);

                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage request = await httpClient.GetAsync(siteURL);
                cancellationToken.Token.ThrowIfCancellationRequested();

                Stream response = await request.Content.ReadAsStreamAsync();
                cancellationToken.Token.ThrowIfCancellationRequested();

                HtmlParser parser = new HtmlParser();
                IHtmlDocument document = parser.ParseDocument(response);

                ParseDocument(document);
            }
            catch (Exception ex)
            {
                Logger.WriteError("Issue scraping website: " + siteURL, ex);
            }
        }

        protected string GetLanguageString()
        {
            if (m_language == LanguageType.Japanese)
                return "ja";
            return "en";
        }

        #endregion

        #region Protected Members

        protected LanguageType m_language;

        #endregion
    }
}
