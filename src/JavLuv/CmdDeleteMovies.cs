using MovieInfo;
using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Linq;

namespace JavLuv
{
    public class CmdDeleteMovies : IAsyncCommand
    {
        #region Constructors

        public CmdDeleteMovies(ProgressWindow progressWindow, List<MovieData> movies, MovieCollection collection)
        {
            m_progressWindow = progressWindow;
            m_movies = movies;
            m_progressWindow.CancelOperation += OnCancelOperation;
            m_collection = collection;
        }

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

            try
            {
                // Delete movies and associated files, and remove from library
                foreach (var movie in m_movies)
                {
                    if (Cancel)
                        break;
                    try
                    {

                        // Delete all movie files
                        foreach (var movieFileName in movie.MovieFileNames)
                            Utilities.DeleteFile(Path.Combine(movie.Path, movieFileName));
                        foreach (var extraMovieFileName in movie.ExtraMovieFileNames)
                            Utilities.DeleteFile(Path.Combine(movie.Path, extraMovieFileName));
                        foreach (var thumbnailFileName in movie.ThumbnailsFileNames)
                            Utilities.DeleteFile(Path.Combine(movie.Path, thumbnailFileName));
                        foreach (var subtitleFileName in movie.SubtitleFileNames)
                            Utilities.DeleteFile(Path.Combine(movie.Path, subtitleFileName));
                        Utilities.DeleteFile(Path.Combine(movie.Path, movie.CoverFileName));
                        Utilities.DeleteFile(Path.Combine(movie.Path, movie.MetadataFileName));

                        // Delete folder if it's not a share path and is empty
                        if (movie.SharedPath == false && Directory.EnumerateFileSystemEntries(movie.Path).Count() == 0)
                            Directory.Delete(movie.Path);

                        // Remove movie from collection
                        m_collection.RemoveMovie(movie);

                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                        {
                            m_progressWindow.CurrentActions++;
                            m_progressWindow.UpdateProgress();
                        }));
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.ToString(), TextManager.GetString("Text.ErrorMovingFolder"));
                    }
                }

                // Ensure users have enough time to register the dialog box operation is complete
                System.Threading.Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), TextManager.GetString("Text.ErrorMovingFolder"));
            }
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                m_progressWindow.IsFinished = true;
                m_progressWindow.Close();
            }));
        }

        #endregion

        #region Private Members

        private List<MovieData> m_movies;
        private ProgressWindow m_progressWindow;
        private MovieCollection m_collection;

        #endregion
    }
}
