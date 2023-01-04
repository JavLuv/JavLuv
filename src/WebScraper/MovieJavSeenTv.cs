using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using MovieInfo;
using System;
using Common;

namespace WebScraper
{
    public class MovieJavSeenTv : ModuleMovie
    {
        #region Constructors

        public MovieJavSeenTv(MovieMetadata metadata, LanguageType language) : base(metadata, language)
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
            var task = ScrapeAsync("https://javseen.tv/search/video/?s=" + movieID);
            task.Wait();

            // Did we find a match?
            if (String.IsNullOrEmpty(m_searchResults))
                return;

            // If so, scrape that address
            task = ScrapeAsync(m_searchResults);
            task.Wait();
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
                else if (element.NodeName == "DIV" && element.ClassName == "col-xs-12 col-sm-6 col-md-7")
                {
                    if (element.TextContent.Contains("Release Day:"))
                    {
                        string premiered = element.TextContent;
                        int index = premiered.IndexOf("Release Day:") + 12;
                        m_metadata.Premiered = premiered.Substring(index).Trim();
                    }
                    else if (element.TextContent.Contains("Studio:"))
                    {
                        string studio = element.TextContent;
                        int index = studio.IndexOf("Studio:") + 7;
                        m_metadata.Studio = studio.Substring(index).Trim();
                    }
                    else if (element.TextContent.Contains("Label:"))
                    {
                        string label = element.TextContent;
                        int index = label.IndexOf("Label:") + 6;
                        m_metadata.Label = label.Substring(index).Trim();
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