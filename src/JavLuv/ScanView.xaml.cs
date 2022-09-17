using System;
using System.Windows;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for ScanView.xaml
    /// </summary>
    public partial class ScanView : Window
    {
        public ScanView(SidePanelViewModel parentDataContext)
        {
            m_parentDataContext = parentDataContext;
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            DataContext = new ScanViewModel(m_parentDataContext);
        }

        private SidePanelViewModel m_parentDataContext;

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            ScanViewModel vm = DataContext as ScanViewModel;
            if (vm != null)
                vm.ScanMoviesCommand.Execute(null);
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}
