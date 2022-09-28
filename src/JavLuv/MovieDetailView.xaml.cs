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

            // Ensures WPF sets initial focus to the first element.  By default, no focus is set
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainGrid.Focus();
        }
    }
}
