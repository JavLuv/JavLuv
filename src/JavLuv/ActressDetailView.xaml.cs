using System.Windows.Controls;
using System.Windows.Input;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for ActressDetailView.xaml
    /// </summary>
    public partial class ActressDetailView : UserControl
    {
        #region Constructor

        public ActressDetailView()
        {
            InitializeComponent();

            // Ensures WPF sets initial focus to the first element.  By default, no focus is set
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        #endregion

        #region Event Handlers

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainGrid.Focus();
        }

        private void ImageLeft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as ActressDetailViewModel;
            if (vm == null)
                return;
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            vm.PreviousImageCommand?.Execute(null);
        }

        private void ImageRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as ActressDetailViewModel;
            if (vm == null)
                return;
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            vm.NextImageCommand?.Execute(null);
        }
        private void ImageLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            var vm = DataContext as ActressDetailViewModel;
            if (vm == null)
                return;
            vm.LeftNavArrowVisibility = System.Windows.Visibility.Visible;
        }

        private void ImageLeft_MouseLeave(object sender, MouseEventArgs e)
        {
            var vm = DataContext as ActressDetailViewModel;
            if (vm == null)
                return;
            vm.LeftNavArrowVisibility = System.Windows.Visibility.Hidden;
        }

        private void ImageRight_MouseEnter(object sender, MouseEventArgs e)
        {
            var vm = DataContext as ActressDetailViewModel;
            if (vm == null)
                return;
            vm.RightNavArrowVisibility = System.Windows.Visibility.Visible;
        }

        private void ImageRight_MouseLeave(object sender, MouseEventArgs e)
        {
            var vm = DataContext as ActressDetailViewModel;
            if (vm == null)
                return;
            vm.RightNavArrowVisibility = System.Windows.Visibility.Hidden;
        }

        #endregion
    }
}
