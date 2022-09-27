using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Common;
using MovieInfo;
using System;

namespace WebScraper
{
    public class ActressJavDatabase : ModuleActress
    {
        #region Constructor

        public ActressJavDatabase(ActressData actressData, LanguageType language) : base(actressData, language)
        {
        }

        #endregion

        #region Public Functions

        public override void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            string name = m_actressData.Name.Replace(' ', '-').ToLower();
            var task = ScrapeAsync("https://www.javdatabase.com/idols/" + name + "/");
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
                            if (source.Contains("idolimages/full/"))
                                ImageSource = source.Trim();
                        }
                    }               
                }

                if (element.TextContent == "Japanese Name")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        m_actressData.JapaneseName = nextSibling.TextContent.Trim();
                    }
                }
                else if (element.TextContent == "Date of Birth")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        string[] dateParts = nextSibling.TextContent.Split('-');
                        try
                        {
                            int year = int.Parse(dateParts[0]);
                            int month = int.Parse(dateParts[1]);
                            int day = int.Parse(dateParts[2]);
                            m_actressData.DateOfBirth = new DateTime(year, month, day);
                        }
                        catch(Exception)
                        { }
                    }
                }
                else if (element.TextContent == "Height")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        try
                        {
                            m_actressData.Height = int.Parse(nextSibling.TextContent);
                        }
                        catch (Exception)
                        { }
                    }
                }
                else if (element.TextContent == "Cup")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        string cupText = nextSibling.TextContent.Trim();
                        // Saw one example of multiple entries.
                        string[] cups = cupText.Split(' ');
                        m_actressData.Cup = cups[0];
                    }
                }
                else if (element.TextContent == "Measurements")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        string[] dateParts = nextSibling.TextContent.Split('-');
                        try
                        {
                            m_actressData.Bust = int.Parse(dateParts[0]);
                            m_actressData.Waist = int.Parse(dateParts[1]);
                            m_actressData.Hips = int.Parse(dateParts[2]);
                        }
                        catch (Exception)
                        { }
                    }
                }
                else if (element.TextContent == "Blood Type")
                {
                    var nextSibling = element.NextSibling;
                    if (IsValidNode(nextSibling))
                    {
                        m_actressData.BloodType = nextSibling.TextContent.Trim();
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
