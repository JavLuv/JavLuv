using System;
using System.Windows;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for FolderMoveWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        #region Constructor

        public ProgressWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        public event EventHandler CancelOperation;

        private void m_cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cancel();
            if (IsFinished == false)
                e.Cancel = true;
        }

        #endregion

        #region Properties

        public string Message
        {
            get { return m_label.Text; }
            set { m_label.Text = value; }
        }

        public int TotalActions { get; set; }

        public int CurrentActions { get; set; }

        public bool IsFinished { get; set; }

        #endregion

        #region Public Functions

        public void UpdateProgress()
        {
            m_count.Text = String.Format("{0}/{1}", CurrentActions, TotalActions);
            m_progressBar.Maximum = TotalActions;
            m_progressBar.Value = CurrentActions;
        }

        #endregion

        #region Private Functions

        private void Cancel()
        {
            m_label.Text = TextManager.GetString("Text.CancellingOperation");
            m_count.Visibility = Visibility.Hidden;
            m_cancel.IsEnabled = false;
            CancelOperation?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
