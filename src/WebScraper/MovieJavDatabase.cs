using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WebScraper
{
    public class MovieJavDatabase : ModuleMovie
    {
        #region Constructors

        public MovieJavDatabase(MovieMetadata metadata, LanguageType language) : base(metadata, language)
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

        #endregion

        #region Protected Functions

        override protected void ParseDocument(IHtmlDocument document)
        {
            foreach (var element in document.All)
            {

                // Check for cover image
                if (element.NodeName == "IMG")
                {
                    if (element.GetAttribute("alt")?.StartsWith(m_metadata.UniqueID.Value) == true)
                    {
                        string srcAttr = element.GetAttribute("src");
                        if (String.IsNullOrEmpty(srcAttr) == false && srcAttr.StartsWith("http"))
                        {
                            // Only get the first image.  The second is much smaller.
                            if (String.IsNullOrEmpty(ImageSource))
                                ImageSource = element.GetAttribute("src");
                        }
                    }
                }

                // Check for actress' parent
                if (element.NodeName == "DIV") 
                {
                    // Ensure this is the feature idols group
                    var child = element.FirstElementChild;
                    if (child?.NodeName == "H2" && child?.TextContent.EndsWith("Actress/Idols") == true)
                    {
                        child = child.NextElementSibling;
                        if (child?.NodeName == "DIV" && child?.ClassName == "row")
                        {
                            // Loop over all actress elements
                            var actressChild = child.FirstElementChild;
                            while (actressChild != null)
                            {
                                // Find actress text
                                var subchild = actressChild.FirstElementChild;
                                do
                                    subchild = subchild.FirstElementChild;
                                while (subchild?.NodeName != "A");
                                string actressName = subchild?.TextContent;
                                if (String.IsNullOrEmpty(actressName) == false)
                                    m_metadata.Actors.Add(new ActorData(actressName));
                                actressChild = actressChild.NextElementSibling;
                            }
                        }
                    }
                }

                if (element.TextContent == "Translated Title:")
                {
                    var nextElement = element.NextElementSibling;
                    if (nextElement != null)
                    {
                        string title = FixCensored(nextElement.TextContent);
                        if (title.StartsWith(Metadata.UniqueID.Value))
                            title = title.Substring(Metadata.UniqueID.Value.Length).Trim();
                        Metadata.Title = title;
                    }
                }
                else if (element.TextContent == "DVD ID:")
                {
                    var nextElement = element.NextElementSibling;
                    if (nextElement != null)
                    {
                        // If the ID doesn't match, we've landed on the wrong page.  Clear metaddata and exit
                        if (Utilities.MovieIDEquals(m_metadata.UniqueID.Value, nextElement.TextContent) == false)
                        {
                            m_metadata = new MovieMetadata(m_metadata.UniqueID.Value);
                            ImageSource = String.Empty;
                            return;
                        }
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
                        string tagContent = element.TextContent.Trim();
                        if (String.IsNullOrEmpty(tagContent))
                            continue;
                        if (tagType == TagType.Genre)
                        {
                            if (m_metadata.Genres.Contains(tagContent) == false)
                                m_metadata.Genres.Add(FixCensored(tagContent));
                        }
                        else if (tagType == TagType.Studio)
                        {
                            if (String.IsNullOrEmpty(m_metadata.Studio))
                                m_metadata.Studio = tagContent;
                        }
                        else if (tagType == TagType.Label)
                        {
                            if (String.IsNullOrEmpty(m_metadata.Label))
                                m_metadata.Label = tagContent;
                        }
                        else if (tagType == TagType.Director)
                        {
                            if (String.IsNullOrEmpty(m_metadata.Director))
                                m_metadata.Director = tagContent;
                        }
                        else if (tagType == TagType.Series)
                        {
                            if (String.IsNullOrEmpty(m_metadata.Series))
                                m_metadata.Series = FixCensored(tagContent);
                        }
                    }
                }
            }
        }

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
