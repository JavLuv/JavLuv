using System.Windows.Controls;
using System.Windows.Input;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for MovieDetailView.xaml
    /// </summary>
    public partial class MovieDetailView : UserControl
    {
        public MovieDetailView()
        {
            InitializeComponent();
        }

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainGrid.Focus();
        }
    }
}
