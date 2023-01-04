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
    public class ActressJavRaveClub : ModuleActress
    {
        #region Constructor

        public ActressJavRaveClub(string name, LanguageType language) : base(name, language)
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
            var task = ScrapeAsync("https://www.javrave.club/pornstar/" + name + "/");
            task.Wait();
        }

        #endregion

        #region Protected Functions

        protected override bool IsLanguageSupported()
        {
            if (m_language == LanguageType.English)
                return true;
            return false;
        }

        protected override void ParseDocument(IHtmlDocument document)
        {
            // Scrape required information from page
            foreach (var element in document.All)
            {
                if (element.NodeName == "H1" && element.TextContent.StartsWith(Name))
                {
                    // Find Japanese names
                    string[] names = element.TextContent.Split('(')[0].Split('-');
                    if (names.Length > 1)
                        Actress.JapaneseName = names[1].Trim();              
                }
                else if (String.IsNullOrEmpty(ImageSource) && element.NodeName == "DIV" && element.ClassName == "the-img-content")
                {
                    // Get actress preview image
                    var nextNode = element.FirstElementChild;
                    if (nextNode?.NodeName == "IMG")
                        ImageSource = nextNode.GetAttribute("src");
                }
                else if (element.NodeName == "DIV" && element.ClassName == "actorinfo-left")
                {
                    if (element.FirstElementChild?.TextContent == "Birthdate")
                    {
                        // Check for birthday
                        string[] dob = element.NextElementSibling?.TextContent.Split('(')[0].Split('-');
                        if (dob.Length > 0)
                            Actress.DobYear = Utilities.ParseInitialDigits(dob[0]);
                        if (dob.Length > 2)
                        {
                            Actress.DobMonth = Utilities.ParseInitialDigits(dob[1]);
                            Actress.DobDay = Utilities.ParseInitialDigits(dob[2]);
                        }
                    }
                    else if (element.FirstElementChild?.TextContent == "Blood Type")
                    {
                        // Blood type
                        Actress.BloodType = element.NextElementSibling?.TextContent;
                    }
                    else if (element.FirstElementChild?.TextContent == "Cupsize")
                    {
                        // Cup size
                        Actress.Cup = element.NextElementSibling?.TextContent;
                    }
                }
            }
        }
        #endregion

    }
}
