using Common;
using MovieInfo;

namespace WebScraper
{
    abstract public class ModuleActress : ModuleBase
    {
        #region Constructors

        public ModuleActress(ActressData actressData, LanguageType language) : base(language)
        {
            m_actressData = actressData;
        }

        #endregion

        #region Protected Members

        protected ActressData m_actressData;

        #endregion
    }
}
