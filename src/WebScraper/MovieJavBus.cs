using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using MovieInfo;
using System;
using Common;

namespace WebScraper
{
    public class MovieJavBus : ModuleMovie
    {
        #region Constructors

        public MovieJavBus(MovieMetadata metadata, LanguageType language) : base(metadata, language)
        {
        }

        #endregion

        #region Public Functions

        override public void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            string movieID = m_metadata.UniqueID.Value;
            var task = ScrapeAsync("https://www.javbus.com/" + GetLanguageString() + "/" + movieID);
            task.Wait();
        }

        #endregion

        #region Protected Functions

        override protected void ParseDocument(IHtmlDocument document)
        {
            // Parse movie info page
            foreach (IElement element in document.All)
            {

                if (element.NodeName == "TITLE")
                {
                    if (element.TextContent.Contains("ID Search Result"))
                        return;
                    string title = element.TextContent;
                    int len = title.Length - 10 - m_metadata.UniqueID.Value.Length;
                    if (len > 0)
                    {
                        // We're stripping off off ID string at beginning and "- JavBus" at end, plus two spaces.
                        m_metadata.Title = title.Substring(m_metadata.UniqueID.Value.Length + 1, len);
                    }
                }
                // Check for cover image
                else if (element.NodeName == "A")
                {
                    if (CheckAttribute(element, "class", "bigImage"))
                    {
                        ImageSource = "https://www.javbus.com" + element.GetAttribute("Href");
                    }
                }
                else if (element.NodeName == "P")
                {
                    string releaseData = GetContent(element, GetToken(Token.ReleaseDate));
                    if (String.IsNullOrEmpty(releaseData) == false)
                    {
                        m_metadata.Premiered = releaseData;
                        int year = Utilities.ParseInitialDigits(m_metadata.Premiered);
                        if (year != -1)
                            m_metadata.Year = year;
                    }
                    string length = GetContent(element, GetContent(element, GetToken(Token.Length)));
                    if (String.IsNullOrEmpty(length) == false)
                    {
                        int minutes = Utilities.ParseInitialDigits(length);
                        if (minutes != -1)
                            m_metadata.Runtime = minutes;
                    }
                    string director = GetContent(element, GetContent(element, GetToken(Token.Director)));
                    if (String.IsNullOrEmpty(director) == false)
                    {
                        m_metadata.Director = director;
                    }
                    string studio = GetContent(element, GetToken(Token.Studio));
                    if (String.IsNullOrEmpty(studio) == false)
                    {
                        m_metadata.Studio = studio;
                    }
                    string label = GetContent(element, GetToken(Token.Label));
                    if (String.IsNullOrEmpty(label) == false)
                    {
                        m_metadata.Label = label;
                    }
                }
                else if (element.NodeName == "SPAN" && "genre" == element.GetAttribute("class"))
                {
                    var child = element.FirstChild;
                    if (child != null)
                    {
                        string genreText = child.TextContent.Trim();
                        if (String.IsNullOrEmpty(genreText) == false)
                            m_metadata.Genres.Add(genreText);
                    }
                }
                else if (element.NodeName == "DIV" && "star-div" == element.GetAttribute("id"))
                {
                    var child1 = element.FirstElementChild;
                    while (child1 != null)
                    {
                        if (child1.NodeName == "DIV" && "avatar-waterfall" == child1.GetAttribute("id"))
                        {
                            var child2 = child1.FirstElementChild;
                            while (child2 != null)
                            {
                                var child3 = child2.FirstElementChild;
                                while (child3 != null)
                                {
                                    if (child3.NodeName == "SPAN")
                                    {
                                        ActorData actorData = new ActorData();
                                        actorData.Name = Utilities.ReverseNames(child3.TextContent.Trim());
                                        m_metadata.Actors.Add(actorData);
                                    }
                                    child3 = child3.NextElementSibling;
                                }
                                child2 = child2.NextElementSibling;
                            }
                        }
                        child1 = child1.NextElementSibling;
                    }
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

        private string GetContent(IElement element, string label)
        {
            if (element == null)
                return String.Empty;
            var next = element.FirstChild;
            if (next != null)
            {
                if (next.TextContent == label)
                {
                    return element.TextContent.Substring(label.Length).Trim();
                }
            }
            return String.Empty;
        }

        private bool CheckAttribute(IElement element, string attributeName, string attributeText)
        {
            if (element == null)
                return false;
            var attributeValue = element.GetAttribute(attributeName);
            return (attributeValue == attributeText);
        }

        #endregion

        #region Private Members

        private string m_pageLink = String.Empty;

        #endregion
    }
}
