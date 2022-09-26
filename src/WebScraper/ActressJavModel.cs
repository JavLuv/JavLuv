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

        public ActressJavModel(ActressData actressData, LanguageType language) : base(actressData, language)
        {
        }

        #endregion

        #region Public Functions

        public override void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            string name = m_actressData.Name.Replace(' ', '-').ToLower();
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
                if (element.NodeName == "IMG")
                {
                    string srcAttr = element.GetAttribute("src");
                    if (String.IsNullOrEmpty(srcAttr) == false && srcAttr.StartsWith("http"))
                    {
                        if (String.IsNullOrEmpty(ImageSource))
                        {
                            string source = element.GetAttribute("src");
                            if (source.Contains("idolimages/full/") || source.Contains("/javdata/uploads/"))
                                ImageSource = source.Trim();
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
                            m_actressData.JapaneseName = child.TextContent;
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
                                m_actressData.DateOfBirth = new DateTime(year, month, day);
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
                            m_actressData.Height = Utilities.ParseInitialDigits(nextSibling.TextContent);
                    }
                }
                else if (element.TextContent == " Breast : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                            m_actressData.Bust = Utilities.ParseInitialDigits(nextSibling.TextContent);
                    }
                }
                else if (element.TextContent == " Waist : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                            m_actressData.Waist = Utilities.ParseInitialDigits(nextSibling.TextContent);
                    }
                }
                else if (element.TextContent == " Hips : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                            m_actressData.Hips = Utilities.ParseInitialDigits(nextSibling.TextContent);
                    }
                }
                else if (element.TextContent == " Blood Type : ")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        nextSibling = nextSibling.NextSibling;
                        if (IsValidNode(nextSibling))
                            m_actressData.BloodType = nextSibling.TextContent;
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
