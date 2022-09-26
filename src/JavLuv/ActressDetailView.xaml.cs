using System.Windows.Controls;
using System.Windows.Input;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for ActressDetailView.xaml
    /// </summary>
    public partial class ActressDetailView : UserControl
    {
        public ActressDetailView()
        {
            InitializeComponent();
        }

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainGrid.Focus();
        }
    }
}
