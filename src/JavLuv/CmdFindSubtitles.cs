using Common;
using MovieInfo;
using Subtitles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace JavLuv
{
    public class CmdFindSubtitles : IAsyncCommand
    {
        #region Constructors

        public CmdFindSubtitles(List<string> movieIDs, string destination)
        {
            m_movieIDs = movieIDs;
            m_destination = destination;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            if (Application.Current == null || Application.Current.Dispatcher == null)
                return;

            try
            {
                foreach (string movieID in m_movieIDs)
                {
                    string subtitleFolder = SubUtilities.GetSubtitlesFolderByID(Settings.Get().Subtitles, movieID);
                    if (String.IsNullOrEmpty(subtitleFolder))
                        continue;
                    if (Directory.Exists(subtitleFolder) == false)
                        continue;
                    string[] subtitles = Directory.GetFiles(subtitleFolder);
                    foreach (string subtitle in subtitles)
                    {
                        string id = Utilities.ParseMovieID(subtitle);
                        if (String.IsNullOrEmpty(id))
                            continue;
                        if (id != movieID)
                            continue;
                        string destinationFile = Path.Combine(m_destination, Path.GetFileName(subtitle));
                        File.Copy(subtitle, destinationFile, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("Error finding subtitles", ex);
            }
        }

        #endregion

        #region Private Members

        private List<string> m_movieIDs;
        private string m_destination;

        #endregion
    }
}
