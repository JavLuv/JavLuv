using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (m_notFound)
                Logger.WriteError("Could not find actress: " + m_actressData.Name);
        }

        #endregion

        #region Protected Functions

        protected override void ParseDocument(IHtmlDocument document)
        {
            // Scrape required information from page
            foreach (var element in document.All)
            {
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
                            m_actressData.Breasts = Utilities.ParseInitialDigits(nextSibling.TextContent);
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

        #region Private Members

        private bool m_notFound = false;

        #endregion
    }
}
