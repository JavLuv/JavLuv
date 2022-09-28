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
using System.Windows.Shapes;

namespace JavLuv
{
    /// <summary>
    /// Interaction logic for ActressMergeView.xaml
    /// </summary>
    public partial class ActressMergeView : Window
    {
        public ActressMergeView(ActressBrowserViewModel parent)
        {
            m_parent = parent;
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            DataContext = new ActressMergeViewModel(m_parent);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private ActressBrowserViewModel m_parent;
    }
}
