using Common;
using MovieInfo;

namespace WebScraper
{
    abstract public class ModuleActress : ModuleBase
    {
        #region Constructors

        public ModuleActress(string name, LanguageType language) : base(language)
        {
            Name = name;
        }

        public ActressData Actress { get; protected set; }

        public string Name { get; set; }

        #endregion

    }
}
