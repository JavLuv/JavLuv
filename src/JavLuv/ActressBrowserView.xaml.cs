using System.Windows.Controls;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for ActressBrowserView.xaml
    /// </summary>
    public partial class ActressBrowserView : UserControl
    {
        public ActressBrowserView()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as ActressBrowserViewModel;
            if (viewModel == null)
                return;
            var listView = sender as ListView;
            if (listView == null)
                return;
            viewModel.SelectedItems.Clear();
            foreach (var item in listView.SelectedItems)
                viewModel.SelectedItems.Add(item as ActressBrowserItemViewModel);
        }
    }
}
