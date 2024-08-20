using AngleSharp.Html.Dom;
using Common;
using System;
using System.Windows.Threading;

namespace WebScraper
{
    public class ActressAsianScreens : ModuleActress
    {
        #region Constructor

        public ActressAsianScreens(string name, Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language) : base(name, dispatcher, webBrowser, language)
        {
        }

        #endregion

        #region Public Functions

        public override void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            string name = Actress.Name.Replace(' ', '_').ToLower();
            ScrapeWebsite("asianscreens.com", "https://www.asianscreens.com/" + name + "2.asp");
        }

        #endregion

        #region Protected Functions

        protected override void ParseDocument(IHtmlDocument document)
        {

            // Scrape required information from page
            foreach (var element in document.All)
            {
                if (element.TextContent == "This page either doesn't exist, or it moved somewhere else.")
                {
                    SearchNotFound = true;
                    m_parsingSuccessful = true;
                    return;
                }
                else if (element.NodeName == "TD")
                {
                    var child = element.FirstChild;
                    if (child != null && child.NodeName == "FONT")
                    {
                        if (child.TextContent.StartsWith("Name:"))
                        {
                            var sibling = element.NextElementSibling;
                            if (sibling != null)
                                Actress.Name = sibling.TextContent;
                        }
                        else if (child.TextContent.StartsWith("DOB:"))
                        {
                            var sibling = element.NextElementSibling;
                            if (sibling != null)
                            {
                                string s = sibling.TextContent.Trim();
                                var dateTime = new DateTime();
                                if (DateTime.TryParse(s, out dateTime))
                                {
                                    Actress.DobDay = dateTime.Day;
                                    Actress.DobMonth = dateTime.Month;
                                    Actress.DobYear = dateTime.Year;
                                }
                            }
                        }
                        else if (child.TextContent.StartsWith("Height:"))
                        {
                            var sibling = element.NextElementSibling;
                            if (sibling != null)
                            {
                                string[] str = sibling.TextContent.Split(' ');
                                if (str.Length == 2)
                                {
                                    int height = 0;
                                    if (Int32.TryParse(str[0], out height))
                                        Actress.Height = height;
                                }
                            }
                        }
                        else if (child.TextContent.StartsWith("Measurements:"))
                        {
                            var sibling = element.NextElementSibling;
                            if (sibling != null)
                            {
                                string[] str = sibling.TextContent.Split(' ');
                                if (str.Length >= 2)
                                {
                                    string[] s = str[0].Split('-');
                                    if (s.Length == 3)
                                    {
                                        int bust = 0;
                                        int waist = 0;
                                        int hips = 0;
                                        if (Int32.TryParse(s[0], out bust))
                                            Actress.Bust = bust;
                                        if (Int32.TryParse(s[1], out waist))
                                            Actress.Waist = waist;
                                        if (Int32.TryParse(s[2], out hips))
                                            Actress.Hips = hips;
                                    }
                                }
                            }
                        }
                        else if (child.TextContent.StartsWith("Cup Size:"))
                        {
                            var sibling = element.NextElementSibling;
                            if (sibling != null)
                                Actress.Cup = sibling.TextContent;
                        }
                    }
                }
                else if (element.NodeName == "IMG")
                {
                    var alt = element.GetAttribute("alt");
                    if (alt != null)
                    {
                        if (alt.StartsWith(Actress.Name))
                        {
                            ImageSource = "https://www.asianscreens.com/" + element.GetAttribute("src");
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

        #endregion
    }
}

