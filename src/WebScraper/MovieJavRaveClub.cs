using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using MovieInfo;
using System;
using Common;

namespace WebScraper
{
    public class MovieJavRaveClub : ModuleMovie
    {
        #region Constructors

        public MovieJavRaveClub(MovieMetadata metadata, LanguageType language) : base(metadata, language)
        {
        }

        #endregion

        #region Public Functions

        override public void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            // Look up movie by ID
            string movieID = m_metadata.UniqueID.Value;
            var task = ScrapeAsync("https://javrave.club/" + movieID);
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
            // Parse movie info page
            foreach (IElement element in document.All)
            {
                if (element.NodeName == "META" && element.GetAttribute("property") == "og:image")
                {
                    ImageSource = element.GetAttribute("content");
                }
                else if (element.NodeName == "H1" && element.ClassName == "entry-title1")
                {
                    string title = element.TextContent;
                    int index = title.IndexOf(m_metadata.UniqueID.Value);
                    if (index == -1)
                        continue;
                    m_metadata.Title = title.Substring(index + m_metadata.UniqueID.Value.Length).Trim();
                    var nextElement = element.NextElementSibling;
                    if (nextElement == null)
                        continue;
                    if (nextElement.NodeName == "H2")
                        m_metadata.OriginalTitle = nextElement.TextContent;
                }
                else if (element.NodeName == "P" && element.TextContent != null)
                {
                    if (element.TextContent.StartsWith("Release Date:"))
                        m_metadata.Premiered = element.TextContent.Substring(14).Replace('/', '-');
                    else if (element.TextContent.StartsWith("Duration:"))
                    {
                        var timeString = element.TextContent.Substring(10);
                        try
                        {
                            var span = TimeSpan.Parse(timeString);
                            m_metadata.Runtime = (int)span.TotalMinutes;
                        }
                        catch { }
                    }
                    else if (element.TextContent.StartsWith("Studio:"))
                        m_metadata.Studio = element.TextContent.Substring(8);
                }
                else if (element.NodeName == "A")
                {
                    string href = element.GetAttribute("href");
                    if (href == null)
                        continue;                  
                    if (href.StartsWith("https://javrave.club/pornstar/"))
                    {
                        string name = element.GetAttribute("title");
                        if (name == null)
                            continue;
                        bool found = false;
                        foreach(ActorData actor in m_metadata.Actors)
                        {
                            if (actor.Name == name)
                            {
                                found = true; 
                                break;
                            }
                        }    
                        if (!found)
                            m_metadata.Actors.Add(new ActorData(name));
                    }
                    else if (href.StartsWith("https://javrave.club/tag/") && element.ClassName == "font-size-1")
                    {
                        string genre = element.TextContent;
                        if (String.IsNullOrEmpty(genre) == false)
                        {
                            m_metadata.Genres.Add(genre);
                        }
                    }                  
                }
                else if (element.NodeName == "H2" && element.ClassName == "widget-title h4")
                {
                    if (element.TextContent == "Random Pornstars")
                    {
                        // This signals that we're finished
                        return;
                    }
                }
            }
        }

        #endregion
    }
}
