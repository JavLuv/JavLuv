using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using MovieInfo;
using System;
using Common;
using System.Reflection.Emit;
using System.Windows.Threading;

namespace WebScraper
{
    public class MovieJavSeenTv : ModuleMovie
    {
        #region Constructors

        public MovieJavSeenTv(MovieMetadata metadata, Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language) : base(metadata, dispatcher, webBrowser, language)
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
            ScrapeWebsite("javseen.tv", "https://javseen.tv/search/video/?s=" + movieID);

            // Did we find a match?
            if (String.IsNullOrEmpty(m_searchResults))
                return;

            // If so, scrape that address
            ScrapeWebsite("javseen.tv", m_searchResults);
        }

        #endregion

        #region Protected Functions

        protected override bool IsLanguageSupported()
        {
            if (m_language == LanguageType.English)
                return true;
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
                    
                    if (element.NodeName != "DIV" || element.ClassName != "video")
                        continue;
                    var child = element.FirstElementChild;
                    if (child == null || child.NodeName != "A")
                        continue;
                    string href = child.Attributes["href"].Value;
                    if (String.IsNullOrEmpty(href))
                        continue;
                    if (href.ContainsCaseless(m_metadata.UniqueID.Value))
                    {
                        m_searchResults = "https://javseen.tv" + href;
                        break;
                    }               
                }
                return;
            }

            // Parse movie info page
            foreach (IElement element in document.All)
            {
                if (element.NodeName == "META")
                {
                    if (element.GetAttribute("property") == "og:title")
                    {
                        string title = element.GetAttribute("content").TrimStart();
                        if (title.Length > 20)
                        {
                            if (title.StartsWith(m_metadata.UniqueID.Value, StringComparison.OrdinalIgnoreCase))
                                title = title.Substring(m_metadata.UniqueID.Value.Length).Trim();
                        }
                        m_metadata.Title = title;
                    }
                    else if (element.GetAttribute("property") == "og:image")
                    {
                        ImageSource = element.GetAttribute("content");
                    }
                }
                else if (element.NodeName == "DIV" && element.ClassName == "col-xs-12 col-sm-6 col-md-8")
                {
                    if (element.TextContent.Contains("Release Day:"))
                    {
                        string premiered = element.TextContent;
                        int index = premiered.IndexOf("Release Day:") + 12;
                        string dateString = premiered.Substring(index).Trim();
                        DateTime date = new DateTime();
                        if (DateTime.TryParse(dateString, out date))
                            m_metadata.Premiered = date.ToString("yyyy-M-d");
                    }
                    else if (element.TextContent.Contains("Studio:"))
                    {
                        string studio = element.TextContent;
                        int index = studio.IndexOf("Studio:") + 7;
                        studio = studio.Substring(index).Trim();
                        if (studio != "----" && String.IsNullOrEmpty(studio) == false)
                            m_metadata.Studio = studio;
                    }
                    else if (element.TextContent.Contains("Label:"))
                    {
                        string label = element.TextContent;
                        int index = label.IndexOf("Label:") + 6;
                        label = label.Substring(index).Trim();
                        if (label != "----" && String.IsNullOrEmpty(label) == false)
                            m_metadata.Label = label;
                    }
                    else if (element.TextContent.Contains("Director:"))
                    {
                        string director = element.TextContent;
                        int index = director.IndexOf("Director:") + 9;
                        director = director.Substring(index).Trim();
                        if (director != "----" && String.IsNullOrEmpty(director) == false)
                            m_metadata.Director = director;
                    }
                }
            }
        }

        #endregion

        #region Private

        private string m_searchResults = string.Empty;

        #endregion
    }
}