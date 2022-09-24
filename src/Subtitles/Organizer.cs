using Common;
using NTextCat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using UtfUnknown;

namespace Subtitles
{
    public class Organizer
    {
        #region Constructor

        public Organizer(Dispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs> FileProcessed;
        public event EventHandler<EventArgs> Finished;

        #endregion

        #region Properties

        public string ImportFolder { private get; set; }
        public string ExportFolder { private get; set; }
        public Mode ProcessingMode { private get; set; }
        public string[] SubtitleExts { private get; set; }
        public int FilesProcessed { get; private set; }
        public int FilesNoID { get; private set; }
        public int FilesDuplicate { get; private set; }
        public int FilesEncodingFixed { get; private set; }
        public int FilesExtensionFixed { get; private set; }
        public int FilesImported { get; private set; }
        #endregion

        #region Public Functions

        public void Start()
        {
            Logger.WriteInfo("Start subtitle organizer");
            m_cancel = false;
            m_thread = new Thread(new ThreadStart(ThreadRun));
            m_thread.Start();
        }

        public void Cancel()
        {
            Logger.WriteInfo("Cancel subtitle organizer");
            m_cancel = true;
            m_thread = null;
        }

        #endregion

        #region Private Functions

        private void ThreadRun()
        {
            try
            {
                // Load language detection library
                if (m_languageFactory == null)
                {
                    m_languageFactory = new RankedLanguageIdentifierFactory();
                    m_languageIdentifier = m_languageFactory.Load("Core14.profile.xml");
                }

                // Check to make sure our initial params are good
                if (String.IsNullOrEmpty(ImportFolder) == false && 
                    String.IsNullOrEmpty(ExportFolder) == false &&
                    Directory.Exists(ImportFolder) && 
                    Directory.Exists(ExportFolder)
                    )
                {
                    ProcessDirectory(ImportFolder);
                }

                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    Finished?.Invoke(this, EventArgs.Empty);
                }));
                m_thread = null;
            }
            catch (Exception ex)
            {
                Logger.WriteError("Error organizing subtitles");
                Logger.WriteError(ex.ToString());
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    Finished?.Invoke(this, EventArgs.Empty);
                }));
            }
        }

        private void ProcessDirectory(string folder)
        {
            if (m_cancel)
                return;

            foreach (string fn in Directory.GetFiles(folder))
            {
                try
                {
                    ProcessFile(fn);
                }
                catch (Exception ex)
                {
                    Logger.WriteError(String.Format("Error processing file {0} in subtitle organizer", fn));
                    Logger.WriteError(ex.ToString());
                }
            }

            foreach (string dir in Directory.GetDirectories(folder))
            {
                try
                {
                    ProcessDirectory(dir);
                }
                catch (Exception ex)
                {
                    Logger.WriteError(String.Format("Error processing directory {0} in subtitle organizer", dir));
                    Logger.WriteError(ex.ToString());
                }
            }

            // Clear folders in move mode if we can
            if (ProcessingMode == Mode.Move)
            {
                if (Directory.GetFiles(folder).Length == 0 && Directory.GetDirectories(folder).Length == 0 &&
                    folder != ImportFolder)
                {
                    Directory.Delete(folder, false);           
                }
            }
        }

        private void ProcessFile(string fileName)
        {
            if (m_cancel)
                return;
            if (IsSubtitle(fileName) == false)
                return;

            FilesProcessed++;

            // Get filename only without extension for parsing preparation
            string fn = Path.GetFileNameWithoutExtension(fileName);

            // Parse ID from shortened filename and check results
            string uniqueID = Utilities.ParseMovieID(fn);
            if (String.IsNullOrEmpty(uniqueID))
            {
                if (CopyOrMoveUnsorted(fileName))
                    FilesNoID++;
                else
                    FilesDuplicate++;
                InvokeFileEvent();
                Logger.WriteWarning("Could not determine unique movie ID from file: " + fileName);
                return;
            }

            // Load and parse the file for analysis
            AnalyzeResults results = AnalyzeFileContent(fileName);

            // Check to see if we need to change the extension
            if (String.Compare(results.extension, Path.GetExtension(fileName), true) != 0)
                FilesExtensionFixed++;

            // If we couldn't determine a language code, check to see if it's already specified
            if (String.IsNullOrEmpty(results.languageCode))
            {
                string fileLanguageCode = GetLanguageCode(fn);
                if (String.IsNullOrEmpty(fileLanguageCode) == false)
                    results.languageCode = fileLanguageCode;
            }

            // Do we need to save a new file due to an encoding change?
            string sourceFileName = fileName;
            if (results.encoding != null && results.encoding != Encoding.UTF8)
            {
                // We're replacing this as the new source file
                sourceFileName = Path.GetTempFileName();
                CopyAsUtf8(fileName, results.encoding, sourceFileName);
                FilesEncodingFixed++;
            }

            // Create destination folder if needed
            string destinationFolder = SubUtilities.GetSubtitlesFolderByID(ExportFolder, uniqueID);

            // Does the desgination folder exist?
            bool destFolderExists = Directory.Exists(destinationFolder);

            // Create a destination filename
            string destinationFileName = String.Empty;
            if (destFolderExists)
            {
                // Create designation filename AND resolve duplicate files
                bool areDuplicates = ResolveDestinationFileName(
                    sourceFileName, 
                    uniqueID,
                    results.languageCode, 
                    destinationFolder, 
                    results.extension,
                    out destinationFileName);

                // Check to see if this was a duplicate file
                if (areDuplicates)
                {
                    FilesDuplicate++;
                    InvokeFileEvent();
                    return;
                }
            }
            else
            {
                // Create our destination filesname
                destinationFileName = CreateDestinationFileName(
                    destinationFolder,
                    uniqueID,
                    results.languageCode,
                    0,
                    results.extension);

                // Create the destination directory
                Directory.CreateDirectory(destinationFolder);
            }

            // Check to see if we've had to create a temp file
            if (sourceFileName == fileName)
            {
                // Copy or move the files to it's new destination
                if (ProcessingMode == Mode.Copy)
                    File.Copy(sourceFileName, destinationFileName, false);
                else
                    Utilities.MoveFile(sourceFileName, destinationFileName);
            }
            else
            {
                // Move the new file to the destination
                Utilities.MoveFile(sourceFileName, destinationFileName);

                // Delete the source to similate a file move
                if (ProcessingMode == Mode.Move)
                    File.Delete(fileName);
            }
            FilesImported++;
            InvokeFileEvent();
        }

        private void InvokeFileEvent()
        {
            m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                FileProcessed?.Invoke(this, EventArgs.Empty);
            }));
        }

        private void CopyAsUtf8(string sourceFileName, Encoding sourceEncoding, string destFileName)
        {
            using (FileStream sourceFile = File.OpenRead(sourceFileName))
            {
                StreamReader reader = new StreamReader(sourceFile, sourceEncoding);
                string s = reader.ReadToEnd();
                using (FileStream destFile = File.OpenWrite(destFileName))
                {
                    StreamWriter writer = new StreamWriter(destFile, Encoding.UTF8);
                    writer.Write(s);
                }
            }
        }

        private bool CopyOrMoveUnsorted(string sourceFileName)
        {
            string fn = Path.GetFileName(sourceFileName);
            string subfolder = Path.GetFileName(Path.GetDirectoryName(sourceFileName));
            if (String.IsNullOrEmpty(subfolder))
                subfolder = "UNKNOWN";
            string destFileName = Path.Combine(ExportFolder, "_UNSORTED", subfolder, fn);
            if (File.Exists(destFileName) == false)
            {
                if (Directory.Exists(Path.GetDirectoryName(destFileName)) == false)
                    Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                if (ProcessingMode == Mode.Copy)
                    File.Copy(sourceFileName, destFileName, false);
                else
                    Utilities.MoveFile(sourceFileName, destFileName);
                return true;
            }
            return false;
        }

        private AnalyzeResults AnalyzeFileContent(string fileName)
        {
            AnalyzeResults results;
            results.encoding = null;
            results.languageCode = String.Empty;
            results.extension = Path.GetExtension(fileName);

            using (FileStream fs = File.OpenRead(fileName))
            {
                // Detect text encoding
                results.encoding = DetectEncoding(fs);
                fs.Position = 0;

                // Don't continue if we can't determine the encoding
                if (results.encoding == null)
                    return results;

                // Open stream reader with designated encoding
                StreamReader reader = new StreamReader(fs, results.encoding);

                // Read the subtitle file and extract only the text
                SubtitleReader subtitle = new SubtitleReader(reader, Path.GetExtension(fileName));
                results.extension = subtitle.FileFormat;
                if (subtitle.IsValid == false)
                {
                    Logger.WriteWarning(String.Format("File {0} didn't have enough readable text", fileName));
                    return results;
                }

                // Detect language
                var languages = m_languageIdentifier.Identify(subtitle.Text);
                var language = languages.FirstOrDefault();
                results.languageCode = m_languageCodeMap[language.Item1.Iso639_2T];
            }
            return results;
        }

        private Encoding DetectEncoding(FileStream fs)
        {
            // Default encoding
            Encoding encoding = null;

            // Detect encoding
            DetectionResult result = CharsetDetector.DetectFromStream(fs);

            // Get the best Detection
            DetectionDetail resultDetected = result.Detected;
            if (result.Detected != null)
            {
                // Get the alias of the found encoding
                string encodingName = resultDetected.EncodingName;

                // Get the System.Text.Encoding of the found encoding (can be null if not available)
                if (resultDetected.Encoding != null)
                    encoding = resultDetected.Encoding;
            }
            return encoding;
        }

        private bool ResolveDestinationFileName(string sourceFileName, string uniqueID, string languageCode, string destinationFolder, string destExt, out string destinationFileName)
        {
            // Create our destination filename
            int variant = 0;
            destinationFileName = CreateDestinationFileName(
                destinationFolder,
                uniqueID,
                languageCode,
                variant,
                destExt
            );

            bool areDuplicates = false;
            while (File.Exists(destinationFileName))
            {
                // Check to see if they are exact matches
                areDuplicates = FilesAreDuplicates(sourceFileName, destinationFileName);
                if (areDuplicates)
                    break;

                // If not exact matches, we'll pick a new filename
                destinationFileName = CreateDestinationFileName(
                    destinationFolder,
                    uniqueID,
                    languageCode,
                    ++variant,
                    destExt
                );
            }
            return areDuplicates;
        }

        private string CreateDestinationFileName(string destFoler, string uniqueID, string languageCode, int variant, string ext)
        {
            string filePath = Path.Combine(destFoler, uniqueID);
            if (variant != 0)
                filePath += "." + variant.ToString();
            if (String.IsNullOrEmpty(languageCode) == false)
                filePath += "." + languageCode;
            filePath += ext;
            return filePath;
        }

        private bool FilesAreDuplicates(string fn1, string fn2)
        {
            var fileInfo1 = new FileInfo(fn1);
            var fileInfo2 = new FileInfo(fn2);

            // First try simple length check
            if (fileInfo1.Length != fileInfo2.Length)
                return false;

            // Check for exact checksum match
            if (GetMD5Checksum(fn1) != GetMD5Checksum(fn2))
                return false;
            
            return true;
        }

        public string GetMD5Checksum(string filename)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream));
                }
            }
        }
        private bool IsSubtitle(string fileName)
        {
            string fileExt = Path.GetExtension(fileName).Substring(1).ToLower();
            foreach (string ext in SubtitleExts)
            {
                if (fileExt == ext.ToLower())
                    return true;
            }
            return false;
        }

        private string GetLanguageCode(string fileName)
        {
            if (fileName.Length < 5)
                return String.Empty;
            if (fileName[fileName.Length - 3] == '.' || fileName[fileName.Length - 3] == '-')
            {
                string code = fileName.Substring(fileName.Length - 2).ToLower();
                if (m_languageCodes.Contains(code) == false)
                    return String.Empty;
                return code;
            }
            return String.Empty;
        }

        #endregion

        #region Public Members

        public enum Mode
        {
            Copy,
            Move,
        }

        #endregion

        #region Private Members

        private struct AnalyzeResults
        {
            public Encoding encoding;
            public string languageCode;
            public string extension;
        }

        private Dispatcher m_dispatcher;
        private Thread m_thread;
        private bool m_cancel = false;
        RankedLanguageIdentifierFactory m_languageFactory = null;
        RankedLanguageIdentifier m_languageIdentifier = null;
        private HashSet<string> m_languageCodes = new HashSet<string>() {
            { "zh" }, // Chinese
            { "da" }, // Danish
            { "nl" }, // Dutch
            { "en" }, // English
            { "fr" }, // French
            { "de" }, // German
            { "it" }, // Italian
            { "jp" }, // Japanese
            { "ko" }, // Korean
            { "no" }, // Norwegian 
            { "pt" }, // Portugese
            { "ru" }, // Russian
            { "es" }, // Spanish
            { "sv" }, // Swedish
        };
        private Dictionary<string, string> m_languageCodeMap = new Dictionary<string, string>() {
            { "zho", "zh" }, // Chinese
            { "dan", "da" }, // Danish
            { "nld", "nl" }, // Dutch
            { "eng", "en" }, // English
            { "fra", "fr" }, // French
            { "deu", "de" }, // German
            { "ita", "it" }, // Italian
            { "jpn", "jp" }, // Japanese
            { "kor", "ko" }, // Korean
            { "nor", "no" }, // Norwegian 
            { "por", "pt" }, // Portugese
            { "rus", "ru" }, // Russian
            { "spa", "es" }, // Spanish
            { "swe", "sv" }, // Swedish

        };       

        #endregion
    }
}
