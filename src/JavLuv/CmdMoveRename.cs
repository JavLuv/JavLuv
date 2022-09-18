using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace JavLuv
{
    public class CmdMoveRename : IAsyncCommand
    {
        #region Constructors

        public CmdMoveRename(MovieCollection movieCollection, ProgressWindow progressWindow, List<MovieData> movies)
        {
            m_movieCollection = movieCollection;
            m_progressWindow = progressWindow;
            m_movies = movies;
            m_progressWindow.CancelOperation += OnCancelOperation;
            Cancel = false;
        }

        #endregion

        #region Event Handlers

        private void OnCancelOperation(object sender, EventArgs e)
        {
            Cancel = true;
        }

        #endregion

        #region Properties

        private bool Cancel { get; set; }

        #endregion

        #region Public Functions

        public void Execute()
        {
            if (Application.Current == null || Application.Current.Dispatcher == null)
                return;

            if (String.IsNullOrEmpty(Settings.Get().Library) || Directory.Exists(Settings.Get().Library) == false)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    m_progressWindow.Close();
                }));
                MessageBox.Show(
                    TextManager.GetString("Text.LibraryFolderInSettings"),
                    TextManager.GetString("Text.MoveRenameError")
                    );
                return;
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                m_progressWindow.TotalActions = m_movies.Count;
                m_progressWindow.UpdateProgress();
            }));

            // New movie data after rename / move
            List<MovieData> newMovies = new List<MovieData>();
            try
            {
                foreach (MovieData movieData in m_movies)
                {
                    // Perform move / rename
                    MovieData newMovieData = MovieUtils.MoveRenameMovieData(
                        movieData,
                        Settings.Get().Library,
                        Settings.Get().Folder,
                        Settings.Get().Movie,
                        Settings.Get().Cover,
                        Settings.Get().Preview,
                        Settings.Get().Metadata
                    );
                    
                    newMovies.Add(newMovieData);

                    if (movieData.SharedPath == false)
                        MovieUtils.RemoveEmptyLibraryFolder(Settings.Get().Library, movieData.Path);

                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                    {
                        m_progressWindow.CurrentActions++;
                        m_progressWindow.UpdateProgress();
                    }));

                    if (Cancel)
                        break;
                }

                // Update collection with new folder and file names
                m_movieCollection.RemoveMovies(newMovies);
                String err;
                m_movieCollection.AddMovies(newMovies, out err);

                // Ensure users have enough time to register the dialog box operation is complete
                System.Threading.Thread.Sleep(500);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    m_progressWindow.IsFinished = true;
                    m_progressWindow.Close();
                }));
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    m_progressWindow.IsFinished = true;
                    m_progressWindow.Close();
                }));
                MessageBox.Show(ex.Message, TextManager.GetString("Text.MoveRenameError"));
                Logger.WriteError("Unexpected error moving/renaming movies - operation aborted.", ex);
            }
        }

        #endregion

        #region Private Members

        private MovieCollection m_movieCollection;
        private ProgressWindow m_progressWindow;
        private List<MovieData> m_movies;

        #endregion
    }
}
