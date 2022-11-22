using System;
using System.Linq;
using System.Windows;

namespace JavLuv
{

    public enum ThemeType
    {
        Dark,
        Light,
    }

    public class ThemeManager
    {
        #region Public Functions

        public static ThemeManager Get() { return s_themeManager; }

        public void SetTheme(ThemeType newTheme)
        {
            if (newTheme == m_currentTheme)
                return;

            // Copy all MergedDictionarys into a auxiliary list.
            var dictionaryList = Application.Current.Resources.MergedDictionaries.ToList();

            // Search for the specified culture.     
            string dictionaryName = string.Format("Themes/{0}.xaml", newTheme.ToString());
            var newThemeDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == dictionaryName);
            if (newThemeDictionary == null)
                throw new Exception("Error finding default theme");

            Application.Current.Resources.MergedDictionaries.Remove(newThemeDictionary);
            Application.Current.Resources.MergedDictionaries.Add(newThemeDictionary);

            m_currentTheme = newTheme;
        }

        #endregion

        #region Private Members

        private static ThemeManager s_themeManager = new ThemeManager();
        private ThemeType m_currentTheme = ThemeType.Dark;

        #endregion
    }
}
