using MovieInfo;
using System;
using System.Windows;
using System.Windows.Threading;

namespace JavLuv
{
    public class CmdGetResolution : IAsyncCommand
    {
        #region Constructors

        public CmdGetResolution(string fileName)
        {
            m_fileName = fileName;
            m_startTime = DateTime.Now;
            m_dispatcher = Dispatcher.CurrentDispatcher;
        }

        #endregion

        #region Events

        public event EventHandler FinishedScanning;

        #endregion

        #region Properties

        public string Resolution { get; set; }

        #endregion

        #region Public Functions

        public void Execute()
        {
            var now = DateTime.Now;
            if (now - m_startTime > TimeSpan.FromSeconds(30))
                return;
            Resolution = MovieUtils.GetMovieResolution(m_fileName);
            if (Application.Current == null || m_dispatcher == null)
                return;
            m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () 
            {
                FinishedScanning?.Invoke(this, new EventArgs());
            }));       
        }

        #endregion

        #region Private Members

        private string m_fileName;
        private DateTime m_startTime;
        private Dispatcher m_dispatcher;
        #endregion
    }
}
