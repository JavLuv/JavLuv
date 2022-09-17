using System;
using System.Windows;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for ConcatView.xaml
    /// </summary>
    public partial class ConcatView : Window
    {
        #region Constrctors

        public ConcatView()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        private void Window_Initialized(object sender, EventArgs e)
        {
            var concatViewModel = new ConcatViewModel();
            concatViewModel.OutputLine += OnOutputLine;
            concatViewModel.Start += OnStart;
            concatViewModel.Finished += OnFinished;
            DataContext = concatViewModel;
        }

        private void OnStart(object sender, EventArgs e)
        {
            m_closeButton.Content = TextManager.GetString("Text.Cancel");
            m_selectButton.IsEnabled = false;
            m_concatButton.IsEnabled = false;
        }

        private void OnFinished(object sender, EventArgs e)
        {
            m_closeButton.Content = TextManager.GetString("Text.Close");
            m_selectButton.IsEnabled = true;
            m_concatButton.IsEnabled = true;
        }

        private void OnOutputLine(object sender, EventArgs e)
        {
            m_output.ScrollToBottom();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var ViewModel = DataContext as ConcatViewModel;
            if (ViewModel.CanCloseWindow() == false)
                e.Cancel = true;
        }

        #endregion
    }
}
