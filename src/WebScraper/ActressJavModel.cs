using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Common;
using MovieInfo;
using System;

namespace WebScraper
{
    public class ActressJavModel : ModuleActress
    {
        #region Constructor

        public ActressJavModel(string name, LanguageType language) : base(name, language)
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
            var task = ScrapeAsync("https://www.javmodel.com/jav/" + name + "/");
            task.Wait();
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

                // Check for Japanese name
                else if (element.NodeName == "DIV")
                {
                    if (element.ClassName == "geodir-category-location japannametext fl-wrap")
                    {
                        var child = element.FirstElementChild;
                        if (child != null && child.NodeName == "A")
                        {
                            Actress.JapaneseName = child.TextContent;
                        }
                    }
                }

                if (element.TextContent == " Born : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                        {
                            string[] dateParts = nextSibling.TextContent.Split('/');
                            try
                            {
                                int month = int.Parse(dateParts[0]);
                                int day = int.Parse(dateParts[1]);
                                int year = int.Parse(dateParts[2]);
                                Actress.DateOfBirth = new DateTime(year, month, day);
                            }
                            catch (Exception)
                            { }
                        }
                    }
                }
                else if (element.TextContent == " Height : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                            Actress.Height = Utilities.ParseInitialDigits(nextSibling.TextContent);
                    }
                }
                else if (element.TextContent == " Breast : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                            Actress.Bust = Utilities.ParseInitialDigits(nextSibling.TextContent);
                    }
                }
                else if (element.TextContent == " Waist : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                            Actress.Waist = Utilities.ParseInitialDigits(nextSibling.TextContent);
                    }
                }
                else if (element.TextContent == " Hips : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                            Actress.Hips = Utilities.ParseInitialDigits(nextSibling.TextContent);
                    }
                }
                else if (element.TextContent == " Blood Type : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                            Actress.BloodType = nextSibling.TextContent;
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
