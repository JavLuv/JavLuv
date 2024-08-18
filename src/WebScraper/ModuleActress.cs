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
            Actress = new ActressData(name);
        }
       
        #endregion

        public ActressData Actress { get; protected set; }

        override protected bool IsValidDataParsed()
        {
            if (m_parsingSuccessful == true)
                return true;
            if (Actress != null)
            {
                if (Actress.Waist != 0 || Actress.DobYear != 0)
                    return true;
            }
            return false;
        }

    }
}
