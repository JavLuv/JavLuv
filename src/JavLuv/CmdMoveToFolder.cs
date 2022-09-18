using MovieInfo;
using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace JavLuv
{
    public class CmdMoveToFolder : IAsyncCommand
    {
        #region Constructors

        public CmdMoveToFolder(ProgressWindow progressWindow, string destination, List<MovieData> movies, MovieCollection collection)
        {
            m_progressWindow = progressWindow;
            m_destination = destination;
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
                // First remove movies that don't need to be movied
                List<MovieData> movies = new List<MovieData>();
                foreach (var movie in m_movies)
                {
                    if (Cancel)
                        break;
                    if (Directory.Exists(movie.Path) == false)
                        continue;
                    if (movie.SharedPath)
                    {
                        if (movie.Path == m_destination)
                            continue;
                    }
                    else
                    {
                        if (movie.Path == Path.Combine(m_destination, movie.Folder))
                            continue;
                    }
                    movies.Add(movie);
                }

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    m_progressWindow.TotalActions = movies.Count;
                    m_progressWindow.UpdateProgress();
                }));

                // Check to see if the destination is a shared path
                bool destSharedPath = m_collection.IsFolderSharedPath(m_destination);

                // Move all folders / files
                foreach (var movie in movies)
                {
                    if (Cancel)
                        break;
                    try
                    {
                        if (movie.SharedPath || destSharedPath)
                        {
                            if (String.IsNullOrEmpty(movie.CoverFileName) == false)
                                Utilities.MoveFile(Path.Combine(movie.Path, movie.CoverFileName), Path.Combine(m_destination, movie.CoverFileName));
                            if (String.IsNullOrEmpty(movie.MetadataFileName) == false)
                                Utilities.MoveFile(Path.Combine(movie.Path, movie.MetadataFileName), Path.Combine(m_destination, movie.MetadataFileName));
                            foreach (var movieFileName in movie.MovieFileNames)
                                Utilities.MoveFile(Path.Combine(movie.Path, movieFileName), Path.Combine(m_destination, movieFileName));
                            foreach (var thumbnailFileName in movie.ThumbnailsFileNames)
                                Utilities.MoveFile(Path.Combine(movie.Path, thumbnailFileName), Path.Combine(m_destination, thumbnailFileName));
                            foreach (var subtitleFileName in movie.SubtitleFileNames)
                                Utilities.MoveFile(Path.Combine(movie.Path, subtitleFileName), Path.Combine(m_destination, subtitleFileName));
                            movie.Path = m_destination;
                        }
                        else
                        {
                            string newPath = Path.Combine(m_destination, movie.Folder);                        
                            Utilities.MoveFolder(movie.Path, newPath);
                            movie.Path = newPath;
                        }
                        ImageCache.Get().Delete(Path.Combine(movie.Path, movie.CoverFileName));

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

        private string m_destination;
        private List<MovieData> m_movies;
        private ProgressWindow m_progressWindow;
        private MovieCollection m_collection;

        #endregion
    }
}
