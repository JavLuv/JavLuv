using MovieInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace JavLuv
{
    public class CmdLoadImage : IAsyncCommand
    {
        #region Constructors

        public CmdLoadImage(string fileName, ImageSize imageSize = ImageSize.Full)
        {
            m_fileName = fileName;
            m_imageSize = imageSize;
        }

        #endregion

        #region Events

        public event EventHandler FinishedLoading;

        #endregion

        #region Properties

        public ImageSource Image { get { return m_image; } }

        public bool Cancel { get;  set; }

        #endregion

        #region Public Functions

        public void Execute()
        {
            if (Cancel)
                return;
            if (Application.Current == null ||Application.Current.Dispatcher == null)
                return;
            m_image = ImageCache.Get().Load(m_fileName, m_imageSize);
            if (Application.Current == null || Application.Current.Dispatcher == null)
                return;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () 
            {
                FinishedLoading?.Invoke(this, new EventArgs());
            }));       
        }

        #endregion

        #region Private Members

        private string m_fileName;
        private ImageSize m_imageSize;
        private ImageSource m_image;

        #endregion
    }
}
