using System;
using System.IO;
using System.Text;

namespace Subtitles
{
    public static class SubUtilities
    {
        #region Public Functions

        public static string GetSubtitlesFolderByID(string rootFolder, string uniqueID)
        {
            return Path.Combine(rootFolder, uniqueID[0].ToString(), uniqueID.Split('-')[0]);
        }

        public static string StripTags(this string text, char opening, char closing)
        {
            StringBuilder sb = new StringBuilder(text.Length);
            bool findClosing = false;
            foreach (char c in text)
            {
                if (findClosing)
                {
                    if (c == closing)
                        findClosing = false;
                }
                else
                {
                    if (c == opening)
                        findClosing = true;
                    else
                        sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string Replace(this string text, string match, string replaceText, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            StringBuilder sb = new StringBuilder(text.Length);
            int index = 0;
            int prevIndex = 0;
            do
            {
                index = text.IndexOf(match, index);
                if (index != -1)
                {
                    string subStr = text.Substring(prevIndex, index - prevIndex);
                    index += match.Length;
                    prevIndex = index;
                    sb.Append(subStr);
                    sb.Append(replaceText);
                }
                else if (index < text.Length)
                {
                    string subStr = text.Substring(prevIndex);
                    sb.Append(subStr);
                }
            }
            while (index >= 0);
            return sb.ToString();
        }

        #endregion
    }
}
