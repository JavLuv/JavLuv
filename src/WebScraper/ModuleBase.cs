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

        public ModuleBase(MovieMetadata metadata, LanguageType language)
        {
            m_metadata = metadata;
            m_language = language;
            CoverImageSource = String.Empty;
        }

        #endregion

        #region Properties

        public string CoverImageSource { get; protected set; }

        #endregion

        #region Public Functions

        abstract public void Scrape();

        abstract public void ParseDocument(IHtmlDocument document);

        #endregion

        #region Protected Functions

        abstract protected bool IsLanguageSupported();

        protected async Task ScrapeAsync(string siteURL)
        {
            try
            {
                Logger.WriteInfo("Scraping website for metadata and cover art: " + siteURL);

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

        protected string GetToken(Token token)
        {
            if (m_language == LanguageType.English)
            {
                switch (token)
                {
                    case Token.ReleaseDate:
                        return "Release Date:";
                    case Token.Length:
                        return "Length:";
                    case Token.Director:
                        return "Director:";
                    case Token.Series:
                        return "Series:";
                    case Token.Studio:
                        return "Studio:";
                    case Token.Maker:
                        return "Maker:";
                    case Token.Label:
                        return "Label:";
                }
            }
            else if (m_language == LanguageType.Japanese)
            {
                switch (token)
                {
                    case Token.ReleaseDate:
                        return "発売日:";
                    case Token.Length:
                        return "収録時間:";
                    case Token.Director:
                        return "監督:";
                    case Token.Series:
                        return "シリーズ:";
                    case Token.Studio:
                        return "メーカー:";
                    case Token.Maker:
                        return "メーカー:";
                    case Token.Label:
                        return "レーベル:";
                }
            }
            return String.Empty;
        }

        #endregion

        #region Protected Members

        protected enum Token
        {
            ReleaseDate,
            Length,
            Director,
            Series,
            Studio,
            Maker,
            Label,
        }

        protected MovieMetadata m_metadata;
        protected LanguageType m_language;

        #endregion
    }
}
