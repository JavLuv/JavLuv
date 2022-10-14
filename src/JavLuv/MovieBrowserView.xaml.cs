using System.Windows.Controls;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for MovieBrowserView.xaml
    /// </summary>
    public partial class MovieBrowserView : UserControl
    {
        public MovieBrowserView()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as MovieBrowserViewModel;
            if (viewModel == null)
                return;
            var listView = sender as ListView;
            if (listView == null)
                return;
            viewModel.SelectedItems.Clear();
            foreach (var item in listView.SelectedItems)
                viewModel.SelectedItems.Add(item as MovieBrowserItemViewModel);
        }
    }
}
