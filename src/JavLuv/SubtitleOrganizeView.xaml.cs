using System;
using System.Windows;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for SubtitleOrganizeView.xaml
    /// </summary>
    public partial class SubtitleOrganizeView : Window
    {
        #region Constructor

        public SubtitleOrganizeView(SidePanelViewModel parent)
        {
            m_parent = parent;
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        private void Window_Initialized(object sender, EventArgs e)
        {
            var subOrgViewModel = new SubtitleOrganizeViewModel(m_parent);
            DataContext = subOrgViewModel;
            subOrgViewModel.Start += OnStart;
            subOrgViewModel.Finished += OnFinished;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as SubtitleOrganizeViewModel;
            vm?.CloseCommand.Execute(null);
            Close();
        }

        private void OnStart(object sender, EventArgs e)
        {
            m_closeButton.Content = TextManager.GetString("Text.Cancel");
        }

        private void OnFinished(object sender, EventArgs e)
        {
            m_closeButton.Content = TextManager.GetString("Text.Close");
        }

        #endregion

        #region Private Members

        private SidePanelViewModel m_parent;

        #endregion
    }
}
