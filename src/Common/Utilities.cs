using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Common
{
    public class FilterPair : IEquatable<FilterPair>
    {
        #region Constructors

        public FilterPair()
        {
            Original = String.Empty;
            Filtered = String.Empty;
        }

        public FilterPair(string source)
        {
            Original = String.Empty;
            Filtered = String.Empty;
            string[] pair = source.Split('=');
            if (pair.Count() > 0)
                Original = pair[0];
            if (pair.Count() > 1)
                Filtered = pair[1];
        }
        public FilterPair(string original, string filtered)
        {
            Original = original;
            Filtered = filtered;
        }

        #endregion

        #region Public Functions

        public override int GetHashCode()
        {
            return Original.GetHashCode() ^ Filtered.GetHashCode();
        }

        public bool Equals(FilterPair other)
        {
            return Original == other.Original && Filtered == other.Filtered;
        }

        #endregion

        #region Properties

        public string Original { get; set; }
        public string Filtered { get; set; }

        #endregion
    }

    public static class Utilities
    {
        #region Public Functions

        public static string ParseMovieID(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return String.Empty;

            // Limit initial ID detections to first ten characters of filename
            const int MaxNumCharsToCheck = 10;
            string shortFileName = fileName;
            if (fileName.Length > MaxNumCharsToCheck)
                shortFileName = fileName.Substring(0, MaxNumCharsToCheck);

            string[] shortChecks =
            {
                @"([a-z,A-Z]{1,6}-[0-9]{2,5}[d,D]{0,1})",
                @"([a-z,A-Z]{1,6}[0-9]{0,2}-[0-9]{2,5}[d,D]{0,1})",
                @"([a-z,A-Z]{1,6}[0-9]{0,2}-[0-9]{1}[d,D]{0,1})",
                @"([a-z,A-Z]{1,6}[0-9]{2,5}[d,D]{0,1})",
                @"([a-z,A-Z]{1,6} [0-9]{2,5}[d,D]{0,1})",
                @"([a-z,A-Z]{1,6}_[0-9]{2,5}[d,D]{0,1})",
            };

            Regex regex = null;
            MatchCollection matches = null;
            foreach (string check in shortChecks)
            {
                regex = new Regex(check);
                matches = regex.Matches(shortFileName);
                if (matches.Count != 0)
                    break;
            }

            if (matches.Count == 0)
            {
                // Search full filename with more explicit matching
                string[] longChecks =
                {
                    @"([a-z,A-Z]{3,6}-[0-9]{3,5}[d,D]{0,1})",
                    @"(\[[a-z,A-Z]{2,6}-[0-9]{2,5}[d,D]{0,1}\])",
                    @"(\([a-z,A-Z]{2,6}-[0-9]{2,5}[d,D]{0,1}\))",
                    @"( [a-z,A-Z]{2,6}[ -][0-9]{3,5}[d,D]{0,1})",
                    @"( [a-z,A-Z]{2,6}[ -][0-9]{3,5}[d,D]{0,1}[ .])",
                };

                foreach (string check in longChecks)
                {
                    regex = new Regex(check);
                    matches = regex.Matches(fileName);
                    if (matches.Count != 0)
                        break;
                }

            }

            // Have we finallly found a match?
            if (matches.Count == 0)
                return String.Empty;

            // After match is found, split apart components for additional processing
            string alpha = String.Empty;
            string numeric = String.Empty;
            SplitIDMatch(matches[0].Value, out alpha, out numeric);

            // Count actual digits
            int digitCount = 0;
            foreach (var c in numeric)
            {
                if (Char.IsDigit(c))
                    ++digitCount;
            }

            // Strip leading zeros greater than three
            if (digitCount > 3)
            {
                int leadingZeroes = 0;
                for (int i = 0; i < digitCount - 3; ++i)
                {
                    if (numeric[i] != '0')
                        break;
                    leadingZeroes++;
                }
                if (leadingZeroes > 0)
                {
                    numeric = numeric.Substring(leadingZeroes, numeric.Length - leadingZeroes);
                }
            }

            // Return normalized ID
            return alpha.ToUpper() + "-" + numeric.ToUpper();
        }

        public static bool Equals(string str, List<string> strings, StringComparison comparison)
        {
            foreach (string s in strings)
            {
                if (String.Equals(str, s, comparison))
                    return true;
            }
            return false;
        }

        public static bool ContainsCaseless(this string stringToSearch, string searchTerm)
        {
            return stringToSearch.IndexOf(searchTerm, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static bool ContainsCaseless(this string stringToSearch, string[] searchTerms)
        {
            foreach (string term in searchTerms)
            {
                if (stringToSearch.ContainsCaseless(term))
                    return true;
            }
            return false;
        }

        public static bool ContainsCaseless(this string stringToSearch, List<string> searchTerms)
        {
            foreach (string term in searchTerms)
            {
                if (stringToSearch.ContainsCaseless(term))
                    return true;
            }
            return false;
        }

        public static bool ContainsCaseless(this List<string> stringsToSearch, string searchTerm)
        {
            foreach (string str in stringsToSearch)
            {
                if (str.ContainsCaseless(searchTerm))
                    return true;
            }
            return false;
        }

        public static int NthIndexOf(this string s, char c, int n)
        {
            var takeCount = s.TakeWhile(x => (n -= (x == c ? 1 : 0)) > 0).Count();
            return takeCount == s.Length ? -1 : takeCount;
        }   

        public static string GetValidSubFolder(string folderName)
        {
            if (String.IsNullOrEmpty(folderName) == false)
            {
                while (Directory.Exists(folderName) == false)
                {
                    folderName = Path.GetDirectoryName(folderName);
                    if (String.IsNullOrEmpty(folderName))
                        return String.Empty;
                }
            }
            return folderName;
        }

        public static string GetJavLuvSettingsFolder()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
                "JavLuv");
            if (Directory.Exists(folder) == false)
                Directory.CreateDirectory(folder);
            return folder;
        }

        public static string GetActressImageFolder()
        {
            string folder = Path.Combine(GetJavLuvSettingsFolder(), "actresses");
            if (Directory.Exists(folder) == false)
                Directory.CreateDirectory(folder);
            return folder;
        }

        public static string[] ProcessSettingsList(string s)
        {
            string[] strings = s.ToLower().Split(';');
            for (int i = 0; i < strings.Length; ++i)
                strings[i] = strings[i].Trim();
            return strings;
        }

        public static string DateTimeToString(DateTime dateTime)
        {
            try
            {
                if (dateTime == null || dateTime == new DateTime())
                    return String.Empty;
                return dateTime.ToString("yyyy-MM-dd");
            }
            catch(Exception)
            {
                return String.Empty;
            }
        }

        public static DateTime StringToDateTime(string dateTimeString)
        {
            try
            {
                return DateTime.ParseExact(dateTimeString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch(Exception)
            {
                return new DateTime();
            }
        }

        public static string StringListToString(List<string> stringList)
        {
            if (stringList.Count == 0)
                return String.Empty;
            var sb = new StringBuilder(stringList.Count * 50);
            foreach (var str in stringList)
            {
                sb.Append(str);
                if (stringList.IndexOf(str) != stringList.Count - 1)
                    sb.Append(", ");
            }
            return sb.ToString();
        }

        public static List<string> StringToStringList(string str)
        {
            var stringList = new List<string>();
            var strings = str.Split(',');
            foreach (var s in strings)
                stringList.Add(s.Trim());
            return stringList;
        }

        public static string FilterListToString(List<FilterPair> filterList)
        {
            if (filterList.Count == 0)
                return String.Empty;
            var sb = new StringBuilder(filterList.Count * 50);
            foreach (var pair in filterList)
            {
                sb.Append(pair.Original.Trim());
                sb.Append('=');
                if (String.IsNullOrEmpty(pair.Filtered) == false)
                    sb.Append(pair.Filtered.Trim());
                if (filterList.IndexOf(pair) != filterList.Count - 1)
                    sb.Append("; ");
            }
            return sb.ToString();
        }

        public static List<FilterPair> StringToFilterList(string filterString)
        {
            var filterList = new List<FilterPair>();
            if (String.IsNullOrEmpty(filterString))
                return filterList;
            string[] pairs = filterString.Split(';');
            foreach (string pair in pairs)
            {
                var filterPair = new FilterPair();
                string[] strings = pair.Split('=');
                if (strings.Length > 0)
                {
                    filterPair.Original = strings[0].Trim();
                    if (strings.Length > 1)
                        filterPair.Filtered = strings[1].Trim();
                    filterList.Add(filterPair);
                }
            }
            return filterList;
        }

        public static string ReverseNames(string name)
        {
            var splitNames = name.Split(' ');
            if (splitNames.Count() == 2)
                return splitNames[1] + " " + splitNames[0];
            return name;
        }

        public static float GetSimilarity(string left, string right)
        {
            if (String.IsNullOrEmpty(left) && String.IsNullOrEmpty(right))
                return 1.0f;
            if (String.IsNullOrEmpty(left) || String.IsNullOrEmpty(right))
                return 0.0f;
            if (left == right)
                return 1.0f;

            int leftSize = left.Length;
            int rightSize = right.Length;
            int leftIdx = 0;
            int rightIdx = 0;
            float matchVal = 0.0f;
            int maxSize = Math.Max(leftSize, rightSize);

            while (leftIdx < leftSize && rightIdx < rightSize)
            {
                if (left[leftIdx] == right[rightIdx])
                {
                    matchVal += 1.0f / maxSize;
                    ++leftIdx;
                    ++rightIdx;
                }
                else if (char.ToLowerInvariant(left[leftIdx]) == char.ToLowerInvariant(right[rightIdx]))
                {
                    matchVal += 0.9f / maxSize;
                    ++leftIdx;
                    ++rightIdx;
                }
                else
                {
                    int lidxbest = leftSize;
                    int ridxbest = rightSize;
                    int totalCount = 0;
                    int bestCount = int.MaxValue;
                    int leftCount = 0;
                    for (int lidx = leftIdx; lidx != leftSize; ++lidx)
                    {
                        int rightCount = 0;
                        for (int ridx = rightIdx; ridx != rightSize; ++ridx)
                        {
                            if (char.ToLowerInvariant(left[lidx]) == char.ToLowerInvariant(right[ridx]))
                            {
                                totalCount = leftCount + rightCount;
                                if (totalCount < bestCount)
                                {
                                    bestCount = totalCount;
                                    lidxbest = lidx;
                                    ridxbest = ridx;
                                }
                            }
                            ++rightCount;
                        }
                        ++leftCount;
                    }
                    leftIdx = lidxbest;
                    rightIdx = ridxbest;
                }
            }
            return Math.Max(Math.Min(matchVal, 1.0f), 0.0f);
        }

        public static string GetCommonFileName(List<string> FileNames)
        {
            var common = new StringBuilder();
            int minSize = Int32.MaxValue;
            foreach (var s in FileNames)
                minSize = Math.Min(minSize, s.Length - Path.GetExtension(s).Length);
            common.Capacity = FileNames[0].Length;
            for (int i = 0; i < minSize; ++i)
            {
                bool match = true;
                for (int j = 1; j < FileNames.Count; ++j)
                {
                    if (FileNames[j][i] != FileNames[0][i])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    common.Append(FileNames[0][i]);
            }
            string str = common.ToString();
            string fn = Path.GetFileNameWithoutExtension(str);
            if (fn.EndsWith("-") || fn.EndsWith("_"))
                fn = fn.Substring(0, fn.Length - 1);
            return fn.Trim();
        }

        public static int ParseInitialDigits(string s, int errVal = -1)
        {
            int digits = 0;
            foreach (char c in s)
            {
                if (Char.IsDigit(c))
                    ++digits;
                else
                    break;
            }
            if (digits > 0)
            {
                string numStr = s.Substring(0, digits);
                int num = 0;
                if (Int32.TryParse(numStr, out num))
                    return num;
            }
            return errVal;
        }

        public static List<string> FilterWordList(List<string> wordList, List<FilterPair> filterList, ref bool changed)
        {
            if (wordList.Count == 0)
                return wordList;

            // Filter words into a hash set to make sure they're all unique
            var set = new HashSet<string>();
            foreach (string word in wordList)
            {
                bool foundMatch = false;
                foreach (var pair in filterList)
                {
                    if (String.Compare(pair.Original, word, true) == 0)
                    {
                        // An empty filter value means we want to block the term
                        if (String.IsNullOrEmpty(pair.Filtered) == false)
                            set.Add(pair.Filtered);
                        foundMatch = true;
                        changed = true;
                    }
                }
                if (!foundMatch)
                    set.Add(word);
            }
            wordList.Clear();

            // Add words back to the original list
            foreach (string word in set)
                wordList.Add(word);

            return wordList;
        }

        public static string TitleNormalize(string title)
        {
            if (String.IsNullOrEmpty(title))
                return String.Empty;

            StringBuilder sb = new StringBuilder(title.Length * 2);

            // Normalize all numbers to a 10-digit format, so sorting
            // of otherwise like strings will sort correctly in numerical
            // order, not lexigraphical order.
            for (int i = 0; i < title.Length; ++i)
            {
                if (Char.IsDigit(title[i]))
                {
                    int digitCount = 0;
                    for (int j = i; j < title.Length; ++j)
                    {
                        if (Char.IsDigit(title[j]))
                            ++digitCount;
                        else
                            break;
                    }
                    int zeroesToAdd = 10 - digitCount;
                    for (int j = 0; j < zeroesToAdd; ++j)
                        sb.Append('0');
                    for (int j = 0; j < digitCount; ++j)
                        sb.Append(title[i + j]);
                    i += digitCount;
                }
                else
                    sb.Append(title[i]);
            }

            title = sb.ToString();
            sb.Clear();

            // Strip off any odd haracters at the beginning of the title that
            // may affect sorting order.
            if (Char.IsLetter(title[0]) == false)
            {
                // Search for a few commaon special cases that we want to exclude 
                // from the initial sorting.
                bool initialSkip = true;
                foreach (char c in title)
                {
                    if (initialSkip)
                    {
                        if (c == '"' || c == '\'' || c == '-' || c == '~' || c == '(' || c == '*' || c == ' ')
                            continue;
                        else
                        {
                            initialSkip = false;
                            sb.Append(c);
                        }
                    }
                    else
                        sb.Append(c);
                }
                title = sb.ToString();
            }

            // Handle titles starting with A, An, and The
            foreach (string word in s_strippedTitleWords)
            {
                if (title.StartsWith(word, true, System.Globalization.CultureInfo.InvariantCulture))
                {
                    title = title.Substring(word.Length, title.Length - word.Length);
                    title = title.Trim();
                }
            }
            return title;
        }

        public static void DeleteFile(string fileName)
        {
            FileDeleteRetry(fileName);
        }

        public static void MoveFile(string sourceFile, string destFile)
        {
            // Check to see if we need to do anything
            if (sourceFile == destFile)
                return;

            // Check to make sure source file exists and dest does not
            if (File.Exists(sourceFile) == false)
                return;

            // If we're not changing case and the destination file exists, exit early
            if (String.Compare(sourceFile, destFile, true) != 0 && File.Exists(destFile))
                return;

            // Check to see if we can move the source to the destination folder
            if (Path.GetPathRoot(sourceFile) == Path.GetPathRoot(destFile))
            {
                // Move/rename the file
                MoveFileRetry(sourceFile, destFile);
            }
            else
            {
                // Copy/delete the file
                CopyDeleteFileRetry(sourceFile, destFile);
            }
         }

        public static string MoveFolder(string sourceFolder, string destFolder)
        {
            // Check to see if we need to do anything
            if (sourceFolder == destFolder)
                return destFolder;

            // Check to make sure source folder exists
            if (Directory.Exists(sourceFolder) == false)
                throw new Exception(String.Format("Directory {} does not exist.", sourceFolder));

            // Check to make sure destination folder doesn't already exist
            if (Directory.Exists(destFolder))
            {
                // If so, get a unique replacement name
                destFolder = GetUniqueFolder(destFolder);
            }
            else
            {
                // Check to see if all previous path parts exist.  If not, create it
                string dirPath = Path.GetDirectoryName(destFolder);
                if (Directory.Exists(dirPath) == false)
                    Directory.CreateDirectory(dirPath);
            }

            // Check to see if we can move the source to the destination folder
            if (Path.GetPathRoot(sourceFolder) == Path.GetPathRoot(destFolder))
            {
                // Move the directory, allowing for multiple retries if needed
                MoveFolderRetry(sourceFolder, destFolder);
            }
            else
            {
                // Copy/delete the directory recursively
                destFolder = CopyDeleteFolderRecursive(sourceFolder, destFolder, true);
            }

            return destFolder;
        }

         public static string CreateFolder(string folder)
        {
            // Check to see if the folder exists
            if (Directory.Exists(folder))
            {
                // If so, get a unique replacement name
                folder = GetUniqueFolder(folder);
            }
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static string GetUniqueFolder(string folder)
        {
            if (Directory.Exists(folder) == false)
                return folder;

            int i = 2;
            string uniqueFolder = folder;
            do
            {
                uniqueFolder = folder + " (" + i.ToString() + ")";
                ++i;
            }
            while (Directory.Exists(uniqueFolder));
            return uniqueFolder;
        }

        #endregion

        #region Private Functions

        private static void SplitIDMatch(string match, out string alpha, out string numeric)
        {
            alpha = String.Empty;
            numeric = String.Empty;
            if (String.IsNullOrEmpty(match))
                return;

            // Fix up for extraneous characters
            var charsToRemove = new string[] { "[", "]", "(", ")", ".", "_" };
            foreach (var c in charsToRemove)
                match = match.Replace(c, string.Empty);
            match = match.Trim();

            char[] splits = { '-', ' ' };
            var parts = match.Split(splits);
            if (parts.Length == 2 && String.IsNullOrEmpty(parts[0]) == false && String.IsNullOrEmpty(parts[1]) == false)
            {
                alpha = parts[0];
                numeric = parts[1];
                return;
            }

            // Fall back to splitting based on where alpha and numeric values appear (ABC123 pattern)

            // Get first digit
            int alphaCount = 0;
            foreach (var c in match)
            {
                if (Char.IsDigit(c))
                    break;
                ++alphaCount;
            }

            // Split based on the number of alpha characters
            alpha = match.Substring(0, alphaCount);
            numeric = match.Substring(alphaCount, match.Length - alphaCount);
        }

        private static string CopyDeleteFolderRecursive(string sourceDir, string destinationDir, bool deleteThisFolder)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            if (Directory.Exists(destinationDir))
                destinationDir = GetUniqueFolder(destinationDir);
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                // Trying to copy "desktop.ini" can create problems.  Skip it.
                if (String.Compare(file.Name, "desktop.ini", true) != 0)
                {
                    string targetFilePath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(targetFilePath);
                }
                FileDeleteRetry(file);
            }

            // Recursively copy
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                destinationDir = CopyDeleteFolderRecursive(subDir.FullName, newDestinationDir, true);
            }

            if (deleteThisFolder)
                FolderDeleteRetry(sourceDir, true);

            return destinationDir;
        }

        private static void CopyDeleteFileRetry(string sourceFile, string destFile)
        {
            // Move file, with multiple retry capability in case of IOExceptions
            bool success = false;
            int retries = 0;
            do
            {
                try
                {
                    File.Copy(sourceFile, destFile, true);
                    File.Delete(sourceFile);
                    success = true;
                }
                catch (IOException)
                {
                    Thread.Sleep(s_retryDelayMs);
                    ++retries;
                }
            }
            while (success == false && retries < s_numRetries);
            if (success == false)
            {
                Logger.WriteWarning("IO error moving file " + sourceFile + " to " + destFile);
                throw new IOException("IO error moving file " + sourceFile + " to " + destFile);
            }
        }

        private static void MoveFileRetry(string sourceFile, string destFile)
        {
            // Move file, with multiple retry capability in case of IOExceptions
            bool success = false;
            int retries = 0;
            do
            {
                try
                {
                    File.Move(sourceFile, destFile);
                    success = true;
                }
                catch (IOException)
                {
                    Thread.Sleep(s_retryDelayMs);
                    ++retries;
                }
            }
            while (success == false && retries < s_numRetries);
            if (success == false)
            {
                Logger.WriteWarning("IO error moving file " + sourceFile + " to " + destFile);
                throw new IOException("IO error moving file " + sourceFile + " to " + destFile);
            }
        }

        private static void MoveFolderRetry(string sourceFolder, string destFolder)
        {
            bool success = false;
            int retries = 0;
            do
            {
                try
                {
                    Directory.Move(sourceFolder, destFolder);
                    success = true;
                }
                catch (IOException)
                {
                    Thread.Sleep(s_retryDelayMs);
                    ++retries;
                }
            }
            while (success == false && retries < s_numRetries);
            if (success == false)
            {
                Logger.WriteWarning("Issue moving folder " + sourceFolder + " to " + destFolder);
                throw new IOException("Issue moving folder " + sourceFolder + " to " + destFolder);
            }
        }

        private static void FileDeleteRetry(string fileName)
        {
            var fileinfo = new FileInfo(fileName);
            FileDeleteRetry(fileinfo);
        }

        private static void FileDeleteRetry(FileInfo file)
        {
            bool success = false;
            int retries = 0;
            do
            {
                try
                {
                    file.Delete();
                    success = true;
                }
                catch (IOException)
                {
                    Thread.Sleep(s_retryDelayMs);
                    ++retries;
                }
                catch (Exception ex)
                {
                    Logger.WriteWarning("Issue deleting file " + file.Name, ex);
                    return;
                }
            }
            while (success == false && retries < s_numRetries);
            if (success == false)
                throw new Exception("Issue deleting file " + file.Name);
        }

        private static void FolderDeleteRetry(string folderName, bool recursive)
        {
            bool success = false;
            int retries = 0;
            do
            {
                try
                {
                    Directory.Delete(folderName, recursive);
                    success = true;
                }
                catch (IOException)
                {
                    Thread.Sleep(s_retryDelayMs);
                    ++retries;
                }
                catch (Exception ex)
                {
                    Logger.WriteWarning("Issue deleting folder " + folderName, ex);
                    return;
                }
            }
            while (success == false && retries < s_numRetries);
            if (success == false)
                throw new Exception("Issue deleting folder " + folderName);
        }

        #endregion

        #region Private Members

        private static readonly string[] s_strippedTitleWords = { "A ", "An ", "The " };
        private static readonly int s_numRetries = 10;
        private static readonly int s_retryDelayMs = 250;
        #endregion
    }
}
