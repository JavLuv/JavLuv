using Common;
using MovieInfo;
using System;
using System.Windows;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        private void Window_Initialized(object sender, EventArgs e)
        {
            Logger.WriteInfo("Main window initialized");
            Settings.Load();

            // Load current culture
            TextManager.SetLanguage(Settings.Get().Language);

            // Restore previous window state
            WindowState = Settings.Get().MainWindowState;
            Width = Settings.Get().MainWindowWidth;
            Height = Settings.Get().MainWindowHeight;
            Left = Settings.Get().MainWindowLeft;
            Top = Settings.Get().MainWindowTop;

            // Make sure the window is accessible
            SizeToFit();
            MoveIntoView();

            DataContext = new MainWindowViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logger.WriteInfo("Main window closing");
            var mainWindowModelView = DataContext as MainWindowViewModel;
            if (mainWindowModelView.IsScanning)
            {
                var msgRes = System.Windows.Forms.MessageBox.Show(
                    TextManager.GetString("Text.ExitJavLuvScanningFiles"),
                    TextManager.GetString("Text.ExitAndCancelScan"),
                    System.Windows.Forms.MessageBoxButtons.YesNo);
                if (msgRes == System.Windows.Forms.DialogResult.Yes)
                {
                    mainWindowModelView.CancelScan();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Logger.WriteInfo("Main window closed");
            var mainWindowModelView = DataContext as MainWindowViewModel;
            mainWindowModelView.Collection.Save();
            Settings.Save();
            Logger.Close();

            // Only close tasks, not commands
            CommandQueue.LongTask().Close();
            CommandQueue.ShortTask().Close();
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// If the saved window dimensions are larger than the current screen shrink the
        /// window to fit.
        /// </summary>
        private void SizeToFit()
        {
            if (Height > System.Windows.SystemParameters.VirtualScreenHeight)
                Height = System.Windows.SystemParameters.VirtualScreenHeight;
            if (Width > System.Windows.SystemParameters.VirtualScreenWidth)
                Width = System.Windows.SystemParameters.VirtualScreenWidth;
        }

        /// <summary>
        /// If the window is more than half off of the screen move it up and to the left 
        /// so half the height and half the width are visible.
        /// </summary>
        private void MoveIntoView()
        {
            if (Top + Height / 2 > System.Windows.SystemParameters.VirtualScreenHeight)
                Top = System.Windows.SystemParameters.VirtualScreenHeight - Height;
            if (Left + Width / 2 > System.Windows.SystemParameters.VirtualScreenWidth)
                Left = System.Windows.SystemParameters.VirtualScreenWidth - Width;
            if (Top < 0)
                Top = 0;
            if (Left < 0)
                Left = 0;
        }

        #endregion //Functions
    }
}
