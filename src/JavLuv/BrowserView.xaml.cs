using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for BrowserView.xaml
    /// </summary>
    public partial class BrowserView : UserControl
    {
        public BrowserView()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as BrowserViewModel;
            if (viewModel == null)
                return;
            var listView = sender as ListView;
            if (listView == null)
                return;
            viewModel.SelectedItems.Clear();
            foreach (var item in listView.SelectedItems)
                viewModel.SelectedItems.Add(item as BrowserItemViewModel);
        }
    }
}
