using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.IO.Compression;

namespace Common
{
    public sealed class Logger
    {
        #region Constructor

        static Logger()
        {
            try
            {
                // Search user folder in strings and replace with censored version
                s_userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                s_userFolderCensored = Path.GetDirectoryName(s_userFolder);
                s_userFolderCensored = Path.Combine(s_userFolderCensored, "****");

                var folder = Utilities.GetJavLuvSettingsFolder();

                string logFilename = Path.Combine(folder, "JavLuv.log");

                // Check to make sure the file doesn't grow in size forever
                if (File.Exists(logFilename))
                {
                    FileInfo fi = new FileInfo(logFilename);
                    if (fi.Length > 4194304)
                        File.Delete(logFilename);
                }

                s_textWriter = new StreamWriter(Path.Combine(folder, "JavLuv.log"), true);
                s_textWriter.WriteLine();
                s_textWriter.WriteLine();

                s_shutdown = false;
                s_queue = new Queue<LogData>();
                s_thread = new Thread(new ThreadStart(ThreadRun));
                s_thread.Start();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Public Functions

        public static void Close()
        {
            if (s_queue == null)
                return;
            WriteToFile();
            lock (s_queue)
            {
                if (s_textWriter != null)
                {
                    s_textWriter.Close();
                    s_textWriter = null;
                }
                s_shutdown = true;
                s_thread = null;
            }
        }

        public static void WriteInfo(string text, Exception ex = null)
        {
            Write(LogType.Info, text, ex);
        }

        public static void WriteWarning(string text, Exception ex = null)
        {
            Write(LogType.Warning, text, ex);
        }

        public static void WriteError(string text, Exception ex = null)
        {
            Write(LogType.Error, text, ex);
        }

        public static void ZipAndCopyLogTo(string directory)
        {
            var folder = Utilities.GetJavLuvSettingsFolder();
            string logFilename = Path.Combine(folder, "JavLuv.log");
            string zipFilename = Path.Combine(folder, "JavLuv.log.zip");

            lock (s_textWriter)
            {
                try
                {
                    s_textWriter.Flush();
                    s_textWriter.Close();
                    using (FileStream fs = new FileStream(zipFilename, FileMode.Create))
                    using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create))
                    {
                        arch.CreateEntryFromFile(logFilename, "JavLuv.log");
                    }
                    File.Copy(zipFilename, Path.Combine(directory, "JavLuv.log.zip"), true);
                    File.Delete(zipFilename);
                    s_textWriter = new StreamWriter(Path.Combine(folder, "JavLuv.log"), true);
                }
                catch (Exception)
                {
                }
            }
        }

        #endregion

        #region Private Functions

        private static void Write(LogType type, string text, Exception ex)
        {
            // Don't allow user's name into the log
            if (text.Contains(s_userFolder))
                text.Replace(s_userFolder, s_userFolderCensored);

            // Append exception details to text if available
            if (ex != null)
            {
                text += ": ";
                text += ex.ToString();
                text += "\n";
                if (ex.InnerException != null)
                {
                    text += "Inner Exception: ";
                    text += ex.InnerException.ToString();
                    text += "\n";
                }
            }

            // Add structured log data to the queue
            LogData data;
            data.logTime = DateTime.Now;
            data.logType = type;
            data.logText = text;
            if (s_queue != null)
            {
                lock (s_queue)
                {
                    s_queue.Enqueue(data);
                }
            }
        }

        private static void WriteToFile()
        {
            if (s_queue == null)
                return;
            var queue = new Queue<LogData>();
            lock (s_queue)
            {
                while (s_queue.Count != 0)
                    queue.Enqueue(s_queue.Dequeue());
            }

            if (s_shutdown || s_textWriter == null)
                return;

            lock(s_textWriter)
            {
                while (queue.Count != 0)
                {
                    try
                    {
                        var logData = queue.Dequeue();
                        s_textWriter.WriteLine(String.Format("{0} : {1} : {2}", logData.logTime.ToString(), logData.logType.ToString(), logData.logText));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static void ThreadRun()
        {
            while (s_shutdown == false)
            {
                WriteToFile();
                int i = 0;
                while ((i < 100) && (s_shutdown == false))
                {
                    ++i;
                    Thread.Sleep(10);
                }
            }
        }

        #endregion

        #region Private Members

        private enum LogType
        {
            Info,
            Warning,
            Error,
        }

        private struct LogData
        {
            public DateTime logTime;
            public LogType logType;
            public string logText;
        }

        // Logging data
        private static bool s_shutdown;
        private static Thread s_thread;
        private static Queue<LogData> s_queue;
        private static TextWriter s_textWriter;
        private static string s_userFolder = String.Empty;
        private static string s_userFolderCensored = String.Empty;

        #endregion
    }
}
