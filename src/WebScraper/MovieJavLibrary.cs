using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using MovieInfo;
using System;
using Common;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace WebScraper
{
    public class MovieJavLibrary : ModuleMovie
    {
        #region Constructors

        public MovieJavLibrary(MovieMetadata metadata, Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language) : base(metadata, dispatcher, webBrowser, language)
        {
        }

        #endregion

        #region Public Functions

        override public void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            string movieID = m_metadata.UniqueID.Value;
            ScrapeWebsite("javlibrary.com", "https://www.javlibrary.com/" + GetLanguageString() + "/vl_searchbyid.php?keyword=" + movieID);

            // Due to a bug in JAVLibrary's search function, even searching by exact ID doesn't
            // always work for IDs beginning with zeroes in the numeric section.  In this
            // case, our first attempt may have landed on a results page, instead of landing
            // directly on the movie info page.  We've checked for this and searched the page
            // for the correct link (typically the first one), and stored the precise link to
            // the page in m_pageLink.  Now we scrape that exact page, then try again.
            if (String.IsNullOrEmpty(m_pageLink) == false)
            {
                ScrapeWebsite("javlibrary.com", m_pageLink);
            }
        }

        #endregion

        #region Protected Functions

        override protected void ParseDocument(IHtmlDocument document)
        {
            // Check to see if we've landed on a search results page.
            if (CheckResultsPage(document))
            {
                return;
            }

            // Parse movie info page
            foreach (IElement element in document.All)
            {
                if (element.NodeName == "DIV")
                {
                    if (element.Id == "video_title")
                    {
                        var childElement = element.FirstElementChild;
                        if (childElement != null)
                        {
                            m_metadata.Title = childElement.TextContent;
                            string id = Utilities.ParseMovieID(m_metadata.Title);
                            if (String.IsNullOrEmpty(id) == false)
                            {
                                int len = m_metadata.Title.Length - id.Length - 1;
                                if (len > 0)
                                {
                                    // We're stripping off off ID string at beginning and "- JAVLibrary" at end, plus two spaces.
                                    m_metadata.Title = m_metadata.Title.Substring(id.Length + 1, len);
                                }
                            }

                        }

                    }
                }
                else if (element.NodeName == "IMG")
                {
                    if (CheckAttribute(element, "id", "video_jacket_img"))
                    {
                        ImageSource = element.GetAttribute("src");
                    }
                }

                string value;
                if (ParseInfoPair(element, GetToken(Token.ReleaseDate), out value))
                {
                    m_metadata.Premiered = value;
                    int year = Utilities.ParseInitialDigits(m_metadata.Premiered);
                    if (year != -1)
                        m_metadata.Year = year;
                }
                else if (ParseInfoPair(element, GetToken(Token.Length), out value))
                {
                    int intVal = 0;
                    if (Int32.TryParse(value, out intVal))
                        m_metadata.Runtime = intVal;
                }
                else if (ParseInfoPair(element, GetToken(Token.Director), out value))
                {
                    if (value != "----")
                        m_metadata.Director = Utilities.ReverseNames(value);
                }
                else if (ParseInfoPair(element, GetToken(Token.Maker), out value))
                {
                    m_metadata.Studio = value.Trim();
                }
                else if (ParseInfoPair(element, GetToken(Token.Label), out value))
                {
                    m_metadata.Label = value.Trim();
                }
                else if (CheckAttribute(element, "class", "genre"))
                {
                    m_metadata.Genres.Add(element.TextContent);
                }
                else if (CheckAttribute(element, "class", "cast"))
                {
                    ParseActors(element);
                }
            }
        }

        protected override bool IsLanguageSupported()
        {
            if (m_language == LanguageType.English)
                return true;
            if (m_language == LanguageType.Japanese)
                return true;
            return false;
        }

        #endregion

        #region Private Functions

        private bool CheckAttribute(IElement element, string attributeName, string attributeText)
        {
            if (element == null)
                return false;
            var attributeValue = element.GetAttribute(attributeName);
            return (attributeValue == attributeText);
        }

        private bool ParseInfoPair(IElement element, string label, out string value)
        {
            if (CheckAttribute(element, "class", "header"))
            {
                string content = element.TextContent;
                if (content == label)
                {
                    var next = element.NextElementSibling;
                    if (next != null)
                    {
                        var child = next.FirstElementChild;
                        if (child != null)
                        {
                            next = child;
                        }
                        if (CheckAttribute(next, "class", "text"))
                        {
                            value = next.TextContent;
                            return true;
                        }
                        if (CheckAttribute(element.NextElementSibling, "class", "text"))
                        {
                            value = next.TextContent;
                            return true;
                        }
                    }
                }            
            }
            value = String.Empty;
            return false;
        }

        private void ParseActors(IElement element)
        {
            var actor = new ActorData();
            actor.Order = m_metadata.Actors.Count;

            var child = element.FirstElementChild;
            if (CheckAttribute(child, "class", "star"))
            {
                actor.Name = Utilities.ReverseNames(child.TextContent);
                actor.Order = m_metadata.Actors.Count;
                child = child.NextElementSibling;
            }
            while (child != null)
            {
                if (CheckAttribute(child, "class", "alias"))
                    actor.Aliases.Add(Utilities.ReverseNames(child.TextContent));
                child = child.NextElementSibling;
            }
            if (String.IsNullOrEmpty(actor.Name) == false)
                m_metadata.Actors.Add(actor);
        }

        private bool CheckResultsPage(IHtmlDocument document)
        {
            // Check to see if we've landed on a search results page.  If so,
            // set the page link we've found and return true.
            
            foreach (var element in document.All)
            {
                if (element.HasAttribute("title") && element.HasAttribute("href"))
                {
                    var nextElement = element.FirstChild;
                    if (nextElement != null && Utilities.MovieIDEquals(nextElement.TextContent, m_metadata.UniqueID.Value))
                    {
                        string href = element.Attributes["href"].Value;
                        m_pageLink = "https://www.javlibrary.com/" + GetLanguageString() + "/" + href;
                        return true;            
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private Members

        private string m_pageLink = String.Empty;

        #endregion
    }
}
