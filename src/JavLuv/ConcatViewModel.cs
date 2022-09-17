using Common;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace JavLuv
{
    public class ConcatViewModel : ObservableObject
    {
        #region Events

        public EventHandler OutputLine;
        public EventHandler Start;
        public EventHandler Finished;

        #endregion

        #region Properties

        public ObservableCollection<string> Files
        {
            get
            {
                return m_files;
            }
        }

        public ObservableCollection<string> Lines
        {
            get
            {
                return m_lines;
            }
        }

        #endregion

        #region Commands

        #region Select Parts Command

        private void SelectPartsExecute()
        {
            var openFileDlg = new System.Windows.Forms.OpenFileDialog();
            openFileDlg.Filter = "Movie files (*.mp4;*.mkv;*.wmv;*.avi)|*.mp4;*.mkv;*.wmv;*.avi|All files(*.*)|*.*";
            openFileDlg.CheckFileExists = true;
            openFileDlg.CheckPathExists = true;
            openFileDlg.Multiselect = true;
            openFileDlg.InitialDirectory = Utilities.GetValidSubFolder(Settings.Get().LastFolder);
            var results = openFileDlg.ShowDialog();
            if (results == System.Windows.Forms.DialogResult.OK)
            {
                Files.Clear();
                foreach (string filename in openFileDlg.FileNames)
                {
                    Files.Add(filename);
                }
            }
        }

        private bool CanSelectPartsExecute()
        {
            return true;
        }

        public ICommand SelectPartsCommand { get { return new RelayCommand(SelectPartsExecute, CanSelectPartsExecute); } }

        #endregion

        #region Concatenate Command

        private void ConcatenateExecute()
        {
            try
            {
                ConcenateMovies();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, TextManager.GetString("Text.UnexpectConcatError")); 
            }
        }

        private bool CanConcatenateExecute()
        {
            return Files.Count >= 2;
        }

        public ICommand ConcatenateCommand { get { return new RelayCommand(ConcatenateExecute, CanConcatenateExecute); } }

        #endregion

        #endregion

        #region Public Functions

        public bool CanCloseWindow()
        {
            if (m_thread != null && m_thread.IsAlive)
            {
                var result = System.Windows.Forms.MessageBox.Show(
                    TextManager.GetString("Text.CancelConcatInProgress"),
                    TextManager.GetString("Text.CancelConcat"), 
                    System.Windows.Forms.MessageBoxButtons.YesNo
                    );
                if (result == System.Windows.Forms.DialogResult.No)
                    return false;
                EndProcess();
            }

            return true;
        }

        #endregion

        #region Private Functions

        private void ConcenateMovies()
        {
            string ext = Path.GetExtension(Files[0]);
            foreach (string filename in Files)
            {
                if (String.CompareOrdinal(Path.GetExtension(filename), ext) != 0)
                {
                    throw new Exception("File extentions are not identical.  Can't concatenate different formats.");
                }
            }

            var mainWindowViewModel = App.Current.MainWindow.DataContext as MainWindowViewModel;
            mainWindowViewModel.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;

            m_thread = new Thread(new ThreadStart(ThreadRun));
            m_thread.Start();
        }

        private void EndProcess()
        {
            m_cancel = true;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                var mainWindowViewModel = App.Current.MainWindow.DataContext as MainWindowViewModel;
                mainWindowViewModel.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                Finished?.Invoke(this, new EventArgs());
            }));
        }

        private void ThreadRun()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    Start?.Invoke(this, new EventArgs());
                }));

                m_cancel = false;

                // Create output path
                string output = Utilities.GetCommonFileName(Files.ToList());
                if (String.IsNullOrEmpty(output))
                    output = Path.ChangeExtension("output", output);
                output = Path.Combine(Path.GetDirectoryName(Files[0]), output);
                output = Path.ChangeExtension(output, Path.GetExtension(Files[0]));

                // If this output file already exists, ask if we want to delete it
                bool CanProceed = true;
                if (File.Exists(output))
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                    {
                        var result = System.Windows.Forms.MessageBox.Show(
                            TextManager.GetString("Text.FileAlreadyExists"),
                            TextManager.GetString("Text.OverwriteFile"), 
                            System.Windows.Forms.MessageBoxButtons.YesNo
                            );
                        if (result == System.Windows.Forms.DialogResult.Yes)
                            File.Delete(output);
                        else
                            CanProceed = false;
                    }));
                }

                // Check to see if can proceed with concatenation
                if (CanProceed)
                {
                    // Clear output text
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                    {
                        Lines.Clear();
                    }));

                    // FFMpeg can handle all known movie formats
                    ConcatMovie(output);

                    // We're done, so signal that we're finished
                    AppendLine("Finished!");
                }
                EndProcess();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, TextManager.GetString("Text.UnexpectConcatError"));
                }));
            }
        }

        private void ConcatMovie(string output)
        {
            // Create a temporary text file containing list of files to concatenate
            string tempFileName = Path.GetTempFileName();
            using (var file = File.Open(tempFileName, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(file))
                {
                    foreach (string filename in Files)
                        writer.WriteLine("file '" + filename + "'");
                }
                file.Close();
            }

            // Create arguments for ffmpeg
            var args = new StringBuilder(1024);
            args.Append("-safe 0 -f concat -i ");
            args.Append("\"");
            args.Append(tempFileName);
            args.Append("\"");
            args.Append(" -c copy \"");
            args.Append(output);
            args.Append("\"");

            // Create process
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = args.ToString(),
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            })
            {
                // Start process and capture output by line
                process.Start();
                while (!process.StandardError.EndOfStream && !m_cancel)
                {
                    string s = process.StandardError.ReadLine();
                    AppendLine(s);
                }

                // If cancelled, kill process and wait for exit
                if (m_cancel)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }

            // If cancelled, delete output file
            if (m_cancel)
                File.Delete(output);

            // Delete temporary text file
            File.Delete(tempFileName);
        }

        private void AppendLine(string s)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                Lines.Add(s);
                OutputLine?.Invoke(this, new EventArgs());
            }));

        }

        private void AppendCharacter(char c)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                if (Lines.Count == 0)
                    Lines.Add("");
                if (c == '\n')
                    Lines.Add("");
                else if (c != '\r')
                    Lines[Lines.Count - 1] += c;
                OutputLine?.Invoke(this, new EventArgs());
            }));
        }

        #endregion

        #region Private Members

        private ObservableCollection<string> m_files = new ObservableCollection<string>();
        private ObservableCollection<string> m_lines = new ObservableCollection<string>();
        private Thread m_thread;
        private bool m_cancel = false;

        #endregion
    }
}
