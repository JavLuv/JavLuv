using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WebScraper
{
    public class ActressJavBody : ModuleActress
    {
        #region Constructor

        public ActressJavBody(string name, Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language) : base(name, dispatcher, webBrowser, language)
        {
        }

        #endregion

        #region Public Functions

        public override void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            string name = Actress.Name.Replace(' ', '-').ToLower();
            ScrapeWebsite("javbody.com", "http://javbody.com/jav/" + name + "/");
        }

        #endregion

        #region Protected Functions

        protected override void ParseDocument(IHtmlDocument document)
        {

            // Scrape required information from page
            foreach (var element in document.All)
            {
                // Check for missing search result
                if (element.TextContent.Contains("was not found on this server."))
                {
                    SearchNotFound = true;
                    m_parsingSuccessful = true;
                    return;
                }

                // Check for actress image
                if (element.NodeName == "DIV" && element.ClassName == "col-lg-4 custom-sm-margin-bottom-1")
                {
                    var childElement = element.FirstElementChild;
                    if (childElement != null)
                    {
                        ImageSource = childElement.GetAttribute("src");
                    }
                }
                else if (element.NodeName == "H1")
                {
                    if (element.TextContent == Actress.Name)
                    {
                        var sibling = element.NextElementSibling as Element;
                        if (sibling != null && sibling.NodeName == "P")
                            Actress.JapaneseName = sibling.TextContent;
                    }
                }
                else if (element.NodeName == "SPAN")
                {
                    var child = element.FirstElementChild as Element;
                    if (child?.NodeName == "STRONG")
                    {
                        if (child.TextContent == "Birthday : ")
                        {
                            string s = element.TextContent.Trim().Substring(child.TextContent.Length).TrimStart();
                            var dateTime = new DateTime();
                            if (DateTime.TryParse(s, out dateTime))
                            {
                                Actress.DobDay = dateTime.Day;
                                Actress.DobMonth = dateTime.Month;
                                Actress.DobYear = dateTime.Year;                               
                            }
                        }
                        else if (child.TextContent == "Blood Type : ")
                        {
                            string s = element.TextContent.Trim().Substring(child.TextContent.Length).TrimStart();
                            Actress.BloodType = s;
                        }
                        else if (child.TextContent == "Breast : ")
                        {
                            string s = element.TextContent.Trim();
                            s = s.Substring(10).TrimStart();
                            int bust = Utilities.ParseInitialDigits(s);
                            if (bust == -1)
                                continue;
                            s = s.Substring(s.IndexOf("Waist : ") + 8).TrimStart();
                            int waist = Utilities.ParseInitialDigits(s);
                            if (waist == -1)
                                continue;
                            s = s.Substring (s.IndexOf("Hip : ") + 6).TrimStart();
                            int hips = Utilities.ParseInitialDigits(s);
                            if (hips == -1)
                                continue;
                            Actress.Bust = bust;
                            Actress.Waist = waist;
                            Actress.Hips = hips;                        
                        }
                        else if (child.TextContent == "Height : ")
                        {
                            string s = element.TextContent.Trim().Substring(child.TextContent.Length).TrimStart();
                            int height = Utilities.ParseInitialDigits(s);
                            if (height != -1)
                                Actress.Height = height;
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
