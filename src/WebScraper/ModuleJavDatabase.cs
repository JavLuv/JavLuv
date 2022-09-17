using AngleSharp.Html.Dom;
using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WebScraper
{
    public class ModuleJavDatabase : ModuleBase
    {
        #region Constructors

        public ModuleJavDatabase(MovieMetadata metadata, LanguageType language) : base(metadata, language)
        {
        }

        #endregion

        #region Public Functions

        override public void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            string movieID = m_metadata.UniqueID.Value;
            var task = ScrapeAsync("https://www.javdatabase.com/movies/" + movieID.ToLower() + "/");
            task.Wait();
        }

        override public void ParseDocument(IHtmlDocument document)
        {
            foreach (var element in document.All)
            {

                // Check for cover image
                if (element.NodeName == "IMG")
                {
                    if (m_metadata.UniqueID.Value == element.GetAttribute("alt"))
                    {
                        if (element.GetAttribute("src").StartsWith("http"))
                        {
                            // Only get the first image.  The second is much smaller.
                            if (String.IsNullOrEmpty(CoverImageSource))
                                CoverImageSource = element.GetAttribute("src");
                        }
                    }
                }              

                if (element.TextContent == "Translated Title:")
                {
                    var nextElement = element.NextElementSibling;
                    if (nextElement != null)
                    {
                        m_metadata.Title = FixCensored(nextElement.TextContent);
                    }
                }
                else if (element.TextContent == "Release Date:")
                {
                    var nextElement = element.NextElementSibling;
                    if (nextElement != null)
                    {
                        m_metadata.Premiered = nextElement.TextContent;
                        int year = Utilities.ParseInitialDigits(m_metadata.Premiered);
                        if (year != -1)
                            m_metadata.Year = year;
                    }
                }
                else if (element.TextContent == "Runtime:")
                {
                    var nextElement = element.NextElementSibling;
                    if (nextElement != null)
                    {
                        int val = Utilities.ParseInitialDigits(nextElement.TextContent);
                        if (val != -1)
                            m_metadata.Runtime = val;
                    }
                }
                else if (element.Attributes["href"] != null && element.Attributes["rel"] != null)
                {
                    string href = element.Attributes["href"].Value;
                    if (href.StartsWith("https://www.javdatabase.com/") && element.Attributes["rel"].Value == "tag")
                    {
                        string s = StripOrigin(href);
                        TagType tagType = ParseTagType(s);
                        string tagContent = element.TextContent;
                        if (String.IsNullOrEmpty(tagContent))
                            continue;
                        if (tagType == TagType.Actors)
                        {
                            var actor = new ActorData(tagContent);
                            actor.Order = m_metadata.Actors.Count;
                            m_metadata.Actors.Add(actor);                        
                        }
                        else if (tagType == TagType.Genre)
                        {
                            if (m_metadata.Genres.Contains(tagContent) == false)
                                m_metadata.Genres.Add(FixCensored(tagContent));
                        }
                        else if (tagType == TagType.Studio)
                        {
                            m_metadata.Studio = tagContent;
                        }
                        else if (tagType == TagType.Label)
                        {
                            m_metadata.Label = tagContent;
                        }
                        else if (tagType == TagType.Director)
                        {
                            m_metadata.Director = tagContent;
                        }
                        else if (tagType == TagType.Series)
                        {
                            m_metadata.Series = FixCensored(tagContent);
                        }
                    }
                }
            }
        }

        #endregion

        #region Protected Functions

        protected override bool IsLanguageSupported()
        {
            if (m_language == LanguageType.English)
                return true;
            return false;
        }

        #endregion

        #region Private Functions

        private string StripOrigin(string s)
        {
            const string origin = "https://www.javdatabase.com/";
            string remainder = s.Substring(origin.Length);
            return remainder;
        }

        private TagType ParseTagType(string s)
        {
            if (s.StartsWith("idols"))
                return TagType.Actors;
            if (s.StartsWith("genres"))
                return TagType.Genre;
            if (s.StartsWith("studios"))
                return TagType.Studio;
            if (s.StartsWith("labels"))
                return TagType.Label;
            if (s.StartsWith("directors"))
                return TagType.Director;
            if (s.StartsWith("series"))
                return TagType.Series;
            return TagType.Unknown;
        }

        private string FixCensored(string original)
        {
            string uncensored = original;
            foreach (var pair in m_censored)
                uncensored = uncensored.Replace(pair.Search, pair.Replace);
            return uncensored;
        }

        #endregion

        #region Private Members

        private enum TagType
        {
            Actors,
            Genre,
            Studio,
            Label,
            Director,
            Series,
            Unknown,
        }

        struct SearchPair
        {
            private readonly string search;
            private readonly string replace;

            public SearchPair(string s, string r)
            {
                search = s;
                replace = r;
            }

            public string Search { get { return search; } }
            public string Replace { get { return replace; } }

        }

        static readonly IList<SearchPair> m_censored = new ReadOnlyCollection<SearchPair>
            (new[] {
                 new SearchPair ("s*****t", "student"),
                 new SearchPair ("S*****t", "Student"),
                 new SearchPair ("s******s", "students"),
                 new SearchPair ("S******s", "Students"),
                 new SearchPair ("s********l", "schoolgirl"),
                 new SearchPair ("S********l", "Schoolgirl"),
                 new SearchPair ("s*********l", "school girl"),
                 new SearchPair ("S*********l", "School Girl"),
                 new SearchPair ("s****l", "school"),
                 new SearchPair ("S****l", "School"),
                 new SearchPair ("sch**l", "school"),
                 new SearchPair ("Sch**l", "School"),
                 new SearchPair ("y********l", "young girl"),
                 new SearchPair ("Y********l", "Young Girl"),
                 new SearchPair ("r**e", "rape"),
                 new SearchPair ("R**e", "Rape"),
                 new SearchPair ("r***s", "rapes"),
                 new SearchPair ("R***s", "Rapes"),
                 new SearchPair ("r****g", "raping"),
                 new SearchPair ("R****g", "Raping"),
                 new SearchPair ("s***e", "slave"),
                 new SearchPair ("S***e", "Slave"),
                 new SearchPair ("m****t", "molest"),
                 new SearchPair ("M****t", "Molest"),
                 new SearchPair ("f***", "fuck"),
                 new SearchPair ("F***", "Fuck"),
                 new SearchPair ("d******e", "disgrace"),
                 new SearchPair ("D******e", "Disgrace"),
                 new SearchPair ("t*****e", "torture"),
                 new SearchPair ("T*****e", "Torture"),
                 new SearchPair ("v*****t", "violent"),
                 new SearchPair ("V*****t", "Violent"),
                 new SearchPair ("h*******m", "hypnotism"),
                 new SearchPair ("H*******m", "Hypnotism"),
                 new SearchPair ("s********n", "submission"),
                 new SearchPair ("S********n", "Submission"),
                 new SearchPair ("g********g", "gangraping"),
                 new SearchPair ("G********g", "Gangraping"),
                 new SearchPair ("g*******g", "gang bang"),
                 new SearchPair ("G*******g", "Gang bang"),
                 new SearchPair ("p****h", "punish"),
                 new SearchPair ("P****h", "Punish"),
                 new SearchPair ("v*****e", "violate"),
                 new SearchPair ("V*****e", "Violate"),
                 new SearchPair ("u*******g", "unwilling"),
                 new SearchPair ("U*******g", "Unwilling"),
                 new SearchPair ("u*********y", "unwillingly"),
                 new SearchPair ("U*********y", "unwillingly"),
                 new SearchPair ("a*****t", "assault"),
                 new SearchPair ("A*****t", "Assault"),
                 new SearchPair ("c***d", "child"),
                 new SearchPair ("C***d", "Child"),
                 new SearchPair ("d***k", "drunk"),
                 new SearchPair ("D***k", "Drunk"),
                 new SearchPair ("d**g", "drug"),
                 new SearchPair ("D**g", "Drug"),
                 new SearchPair ("a***e", "abuse"),
                 new SearchPair ("A***e", "Abuse"),
                 new SearchPair ("k**l", "kill"),
                 new SearchPair ("K**l", "Kill"),
                 new SearchPair ("p*ss", "puss"),
                 new SearchPair ("P*ss", "Puss"),
                 new SearchPair ("b***d", "blood"),
                 new SearchPair ("B***d", "Blood"),
                 new SearchPair ("c*****y", "cruelty"),
                 new SearchPair ("C*****y", "Cruelty"),
                 new SearchPair ("a****p", "asleep"),
                 new SearchPair ("A****p", "Asleep"),
                 new SearchPair ("m************n", "mother and son"),
                 new SearchPair ("M************n", "Mother And Son"),
                  new SearchPair ("k*d", "kid"),
                 new SearchPair ("K*d", "Kid"),
         });
        #endregion
    }
}
