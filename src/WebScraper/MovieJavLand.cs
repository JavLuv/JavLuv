using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using MovieInfo;
using System;
using Common;

namespace WebScraper
{
    public class MovieJavLand : ModuleMovie
    {
        #region Constructors

        public MovieJavLand(MovieMetadata metadata, LanguageType language) : base(metadata, language)
        {
        }

        #endregion

        #region Public Functions

        override public void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            string movieID = m_metadata.UniqueID.Value;
            var task = ScrapeAsync("https://www.jav.land/" + GetLanguageString() + "/id_search.php?keys=" + movieID);
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
                    if (element.TextContent.Contains("ID Search Result") || element.TextContent.Contains("品番検索結果"))
                        return;
                    string title = element.TextContent;
                    int len = title.Length - 12 - m_metadata.UniqueID.Value.Length;
                    if (len > 0)
                    {
                        // We're stripping off off ID string at beginning and "- JAV.land" at end, plus two spaces.
                        m_metadata.Title = title.Substring(m_metadata.UniqueID.Value.Length + 1, len);
                    }
                }

                // Check for cover image
                else if (element.NodeName == "IMG")
                {
                    if (m_metadata.UniqueID.Value == element.GetAttribute("alt"))
                    {
                        ImageSource = element.GetAttribute("src");
                    }
                }

                if (element.TextContent == GetToken(Token.ReleaseDate))
                {
                    var next = element.NextElementSibling;
                    if (next != null)
                    {
                        m_metadata.Premiered = next.TextContent;
                        int year = Utilities.ParseInitialDigits(m_metadata.Premiered);
                        if (year != -1)
                            m_metadata.Year = year;
                    }
                }
                else if (element.TextContent == GetToken(Token.Length))
                {
                    var next = element.NextElementSibling;
                    if (next != null)
                    {
                        int minutes = Utilities.ParseInitialDigits(next.TextContent);
                        if (minutes != -1)
                            m_metadata.Runtime = minutes;
                    }
                }
                else if (element.TextContent == GetToken(Token.Director))
                {
                    var next = element.NextElementSibling;
                    if (next != null)
                    {
                        if (next.TextContent != "---")
                            m_metadata.Director = next.TextContent;
                    }
                }
                else if (element.TextContent == GetToken(Token.Series))
                {
                    var next = element.NextElementSibling;
                    if (next != null)
                    {
                        if (next.TextContent != "---")
                            m_metadata.Series = next.TextContent;
                    }
                }
                else if (element.TextContent == GetToken(Token.Maker))
                {
                    var next = element.NextElementSibling;
                    if (next != null)
                    {
                        next = next.FirstElementChild;
                        if (next != null)
                        {
                            next = next.FirstElementChild;
                            if (next != null)
                                m_metadata.Studio = next.TextContent;
                        }
                    }
                }
                else if (element.TextContent == GetToken(Token.Label))
                {
                    var next = element.NextElementSibling;
                    if (next != null)
                    {
                        next = next.FirstElementChild;
                        if (next != null)
                        {
                            next = next.FirstElementChild;
                            if (next != null)
                                m_metadata.Label = next.TextContent;
                        }
                    }
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

        private void ParseActors(IElement element)
        {
            var actor = new ActorData();
            actor.Order = m_metadata.Actors.Count;

            var child = element.FirstElementChild;
            if (CheckAttribute(child, "class", "star"))
            {
                actor.Name = child.TextContent;
                if (String.IsNullOrEmpty(actor.Name) == false)
                    m_metadata.Actors.Add(actor);
            }
        }

        #endregion
    }
}
