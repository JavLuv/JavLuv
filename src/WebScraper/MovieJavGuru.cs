using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using MovieInfo;
using System;
using Common;
using System.Windows.Threading;

namespace WebScraper
{
    public class MovieJavGuru : ModuleMovie
    {
        #region Constructors

        public MovieJavGuru(MovieMetadata metadata, Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language) : base(metadata, dispatcher, webBrowser, language)
        {
        }

        #endregion

        #region Public Functions

        override public void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            // First search the site
            string movieID = m_metadata.UniqueID.Value;
            ScrapeWebsite("jav.guru", "https://jav.guru/?s=" + movieID);
            m_parsingSuccessful = false;

            // Did we find a match?
            if (String.IsNullOrEmpty(m_searchResults))
                return;

            // If so, scrape that address
            ScrapeWebsite("jav.guru", m_searchResults);
        }

        #endregion

        #region Protected Functions

        protected override bool IsLanguageSupported()
        {
            // Temporarily disable JavGuru, unless we can
            //if (m_language == LanguageType.English)
            //    return true;
            return false;
        }

        protected override void ParseDocument(IHtmlDocument document)
        {
            // First, search for a specific movie title.  Once found, we can navigate to that page and scrape it
            if (String.IsNullOrEmpty(m_searchResults))
            {
                // Parse movie info page
                foreach (IElement element in document.All)
                {
                    if (element.NodeName != "A")
                        continue;
                    if (element.Attributes["href"] == null)
                        continue;
                    string href = element.Attributes["href"].Value;
                    if (href.StartsWith("https://jav.guru/") && href.ContainsCaseless(m_metadata.UniqueID.Value))
                    {
                        m_searchResults = href;
                        m_parsingSuccessful = true;
                        break;
                    }
                }

                return;
            }

            // Parse movie info page
            foreach (IElement element in document.All)
            {
                if (element.NodeName == "H1" && element.ClassName == "titl")
                {
                    if (element.TextContent.Length > 20)
                    {
                        if (Utilities.ParseMovieID(element.TextContent) == m_metadata.UniqueID.Value)
                            m_metadata.Title = element.TextContent.Substring(m_metadata.UniqueID.Value.Length + 2).Trim();
                        else
                            m_metadata.Title = element.TextContent.Trim();
                    }
                }
                else if (element.NodeName == "LI")
                {
                    if (element.TextContent.StartsWith("Release Date: "))
                        m_metadata.Premiered = element.TextContent.Substring(14).Trim().Replace('/', '-');
                    else if (element.TextContent.StartsWith("Studio: "))
                        m_metadata.Studio = element.TextContent.Substring(8).Trim();
                    else if (element.TextContent.StartsWith("Label: "))
                        m_metadata.Label = element.TextContent.Substring(7).Trim();
                    else if (element.TextContent.StartsWith("Tags: "))
                    {
                        var child = element.FirstElementChild;
                        while (child != null)
                        {
                            if (child.NodeName == "A" && String.IsNullOrEmpty(child.TextContent) == false)
                                m_metadata.Genres.Add(child.TextContent.Trim());
                            child = child.NextElementSibling;
                        }
                    }
                    else if (element.TextContent.StartsWith("Actress: "))
                    {
                        var child = element.FirstElementChild;
                        while (child != null)
                        {
                            if (child.NodeName == "A" && String.IsNullOrEmpty(child.TextContent) == false)
                                m_metadata.Actors.Add(new ActorData(Utilities.ReverseNames(child.TextContent.Trim())));
                            child = child.NextElementSibling;
                        }
                    }
                }
                else if (element.NodeName == "DIV" && element.ClassName == "large-screenimg")
                {
                    var child = element.FirstElementChild;
                    if (child == null)
                        continue;
                    if (child.NodeName != "IMG")
                        continue;
                    string html = child.Html();
                    string src = child.GetAttribute("src");
                    int index = src.IndexOf("https://");
                    if (index != -1)
                        src = src.Substring(index);
                    ImageSource = src;
                }
            }
        }

        #endregion

        #region Private

        private string m_searchResults = string.Empty;

        #endregion
    }
}
