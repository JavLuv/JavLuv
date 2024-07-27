using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Common;
using MovieInfo;
using System;
using System.Windows.Threading;

namespace WebScraper
{
    public class ActressJavModel : ModuleActress
    {
        #region Constructor

        public ActressJavModel(string name, Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language) : base(name, dispatcher, webBrowser, language)
        {
        }

        #endregion

        #region Public Functions

        public override void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            Actress = new ActressData(Name);
            string name = Actress.Name.Replace(' ', '-').ToLower();
            ScrapeWebsite("javmodel.com", "http://javmodel.com/jav/" + name + "/");
        }

        #endregion

        #region Protected Functions

        protected override void ParseDocument(IHtmlDocument document)
        {
            // Scrape required information from page
            foreach (var element in document.All)
            {
                // Check for actress image
                if (element.NodeName == "META")
                {
                    var property = element.GetAttribute("property");
                    if (property != null && property == "og:image")
                    {
                        var content = element.GetAttribute("content");
                        if (content != null)
                        {
                            ImageSource = content.Trim();
                        }
                    }
                }

                // Check for various metadata
                else if (element.NodeName == "H2" && element.ClassName == "h5 mb-4")
                {
                    Actress.JapaneseName = element.TextContent.Trim();
                }
                else if (element.NodeName == "TD" && element.ClassName == "flq-color-meta")
                {
                    if (element.TextContent == "Birthday")
                    {
                        var nextSibling = element.NextSibling;
                        if (nextSibling != null)
                        {
                            nextSibling = nextSibling.NextSibling;
                            if (nextSibling != null)
                            {
                                try
                                {
                                    var birthday = DateTime.Parse(nextSibling.TextContent);
                                    Actress.DobDay = birthday.Day;
                                    Actress.DobMonth = birthday.Month;
                                    Actress.DobYear = birthday.Year;
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                    else if (element.TextContent == "Blood Type")
                    {
                        var nextSibling = element.NextSibling;
                        if (nextSibling != null)
                        {
                            nextSibling = nextSibling.NextSibling;
                            if (nextSibling != null)
                                Actress.BloodType = nextSibling.TextContent.Trim();
                        }
                    }
                    else if (element.TextContent == "Breast")
                    {
                        var nextSibling = element.NextSibling;
                        if (nextSibling != null)
                        {
                            nextSibling = nextSibling.NextSibling;
                            if (nextSibling != null)
                                Actress.Bust = Utilities.ParseInitialDigits(nextSibling.TextContent);
                        }
                    }
                    else if (element.TextContent == "Waist")
                    {
                        var nextSibling = element.NextSibling;
                        if (nextSibling != null)
                        {
                            nextSibling = nextSibling.NextSibling;
                            if (nextSibling != null)
                                Actress.Waist = Utilities.ParseInitialDigits(nextSibling.TextContent);
                        }
                    }
                    else if (element.TextContent == "Hips")
                    {
                        var nextSibling = element.NextSibling;
                        if (nextSibling != null)
                        {
                            nextSibling = nextSibling.NextSibling;
                            if (nextSibling != null)
                                Actress.Hips = Utilities.ParseInitialDigits(nextSibling.TextContent);
                        }
                    }
                    else if (element.TextContent == "Height")
                    {
                        var nextSibling = element.NextSibling;
                        if (nextSibling != null)
                        {
                            nextSibling = nextSibling.NextSibling;
                            if (nextSibling != null)
                                Actress.Height = Utilities.ParseInitialDigits(nextSibling.TextContent);
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

        private bool IsValidNode(INode element)
        {
            if (element == null)
                return false;
            if (String.IsNullOrEmpty(element.TextContent))
                return false;
            if (element.TextContent.Trim() == "Unknown")
                return false;
            return true;
        }

        #endregion
    }
}
