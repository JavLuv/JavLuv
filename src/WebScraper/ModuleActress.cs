using Common;
using MovieInfo;
using System.Windows.Threading;

namespace WebScraper
{
    abstract public class ModuleActress : ModuleBase
    {
        #region Constructors

        public ModuleActress(string name, Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language) : base(dispatcher, webBrowser, language)
        {
            Name = name;
        }

        public ActressData Actress { get; protected set; }

        public string Name { get; set; }

        #endregion

    }
}
