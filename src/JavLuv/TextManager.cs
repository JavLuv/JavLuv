using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using Common;

namespace JavLuv
{
    public class TextManager
    {
        #region Public Functions

        public static string GetString(string key)
        {
            Object val = Application.Current.FindResource(key);
            if (val == null)
            {
                Logger.WriteError("Could not find text for key " + key);
                return String.Empty;
            }
            return val.ToString().Replace(@"\n", "\n");
        }

        public static void SetLanguage(LanguageType language)
        {
            if (language == LanguageType.English)
                s_textManager.SelectCulture("en-US");
            else if (language == LanguageType.Japanese)
                s_textManager.SelectCulture("jp-JP");
        }

        #endregion

        #region Private Functions

        private void SelectCulture(string culture)
        {
            if (String.IsNullOrEmpty(culture) || culture == m_currentCulture)
                return;

            // Copy all MergedDictionarys into a auxiliar list.
            var dictionaryList = Application.Current.Resources.MergedDictionaries.ToList();

            // Search for the specified culture.     
            string requestedCulture = string.Format("Localization/StringResources.{0}.xaml", culture);
            var resourceDictionary = dictionaryList.
                FirstOrDefault(d => d.Source.OriginalString == requestedCulture);

            if (resourceDictionary == null)
            {
                //If not found, select our default language.             
                requestedCulture = "Localization/StringResources.xaml";
                resourceDictionary = dictionaryList.
                    FirstOrDefault(d => d.Source.OriginalString == requestedCulture);
            }

            // If we have the requested resource, remove it from the list and place at the end.     
            // Then this language will be our string table to use.      
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }

            // Inform the threads of the new culture.     
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }

        #endregion

        #region Private Members

        private static TextManager s_textManager = new TextManager();
        private string m_currentCulture = String.Empty;

        #endregion
    }
}
