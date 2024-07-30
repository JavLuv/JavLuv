using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Common;
using MovieInfo;
using System;
using System.Runtime.Remoting.Contexts;
using System.Windows.Threading;

namespace WebScraper
{
    public class ActressJavDatabase : ModuleActress
    {
        #region Constructor

        public ActressJavDatabase(string name, Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language) : base(name, dispatcher, webBrowser, language)
        {
        }

        #endregion

        #region Public Functions

        public override void Scrape()
        {
            if (IsLanguageSupported() == false)
                return;

            string name = Actress.Name.Replace(' ', '-').ToLower();
            ScrapeWebsite("javdatabase.com", "https://www.javdatabase.com/idols/" + name + "/");
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
                if (element.NodeName == "H1")
                {
                    if (element.TextContent.StartsWith(Actress.Name))
                    {
                        if (element.Parent == null)
                            continue;
                        string content = element.Parent.TextContent;
                        if (String.IsNullOrEmpty(content))
                            continue;

                        // Parse Japanese name
                        Actress.JapaneseName = Parse(content, "JP: ");

                        // Parse date of birth
                        string dobText = Parse(content, "DOB: ");
                        int year = 0;
                        int month = 0;
                        int day = 0;
                        Utilities.StringToDateTime(dobText, out year, out month, out day);
                        Actress.DobYear = year;
                        Actress.DobMonth = month;
                        Actress.DobDay = day;

                        // Parse height
                        string height = Parse(content, "Height: ");
                        Actress.Height = Utilities.ParseInitialDigits(height);

                        // Parse measurements
                        string measurements = Parse(content, "Measurements: ");
                        string[] dateParts = measurements.Split('-');
                        try
                        {
                            Actress.Bust = int.Parse(dateParts[0]);
                            Actress.Waist = int.Parse(dateParts[1]);
                            Actress.Hips = int.Parse(dateParts[2]);
                        }
                        catch (Exception) 
                        { }

                        // Parse cup
                        string cupText = Parse(content, "Cup: ");
                        // Saw one example of multiple entries.
                        string[] cups = cupText.Split(' ');
                        if (String.Compare(cups[0], "Unknown", true) != 0)
                            Actress.Cup = cups[0];

                        // Parse blood type
                        Actress.BloodType = Parse(content, "Blood: ");
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

        private string Parse(string text, string search)
        {
            string retVal = String.Empty;
            int index = text.IndexOf(search, StringComparison.Ordinal);
            if (index == -1)
                return retVal;
            string substr = text.Substring(index + search.Length);
            if (substr.Length == 0)
                return retVal;
            int wsIndex = substr.IndexOf(' ');
            int nlIndex = substr.IndexOf('\n');
            if (wsIndex == -1 && nlIndex == -1)
                index = substr.Length - 1;
            else if (wsIndex == -1)
                index = nlIndex;
            else if (nlIndex == -1)
                index = wsIndex;
            else
                index = Math.Min(wsIndex, nlIndex);
            retVal = substr.Substring(0, index);
            return retVal;
        }

        #endregion
    }
}
