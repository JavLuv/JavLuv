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
    /// Interaction logic for RatingControl.xaml
    /// </summary>
    public partial class RatingControl : UserControl
    {

        #region Constructor

        public RatingControl()
        {
            InitializeComponent();
            SetCurrentValue(HeightProperty, 30.0);
            SetStars(Value);
        }

        #endregion

        #region Dependency Properties

        // Value
        public Int32 Value
        {
            get { return (Int32)GetValue(ValueProperty); }
            set { SetCurrentValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(Int32),
                typeof(RatingControl),
                new FrameworkPropertyMetadata((Int32)0, OnValueChanged, CoerceValue)
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                }
            );

        #endregion

        #region Notification and Coersion Functions

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(ValueProperty);
        }

        private static object CoerceValue(DependencyObject d, object value)
        {
            RatingControl n = (RatingControl)d;
            Int32 val = (Int32)value;
            if (val < 0)
                val = 0;
            if (val > 10)
                val = 10;
            return val;
        }

        #endregion

        #region Handler Functions

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            m_mouseHovering = false;
            SetStars(Value);
        }

        private void m_button0_Click(object sender, RoutedEventArgs e)
        {
            Value = 0;
        }

        private void m_button1_Click(object sender, RoutedEventArgs e)
        {
            Value = 1;
        }

        private void m_button2_Click(object sender, RoutedEventArgs e)
        {
            Value = 2;
        }

        private void m_button3_Click(object sender, RoutedEventArgs e)
        {
            Value = 3;
        }

        private void m_button4_Click(object sender, RoutedEventArgs e)
        {
            Value = 4;
        }

        private void m_button5_Click(object sender, RoutedEventArgs e)
        {
            Value = 5;
        }

        private void m_button6_Click(object sender, RoutedEventArgs e)
        {
            Value = 6;
        }

        private void m_button7_Click(object sender, RoutedEventArgs e)
        {
            Value = 7;
        }

        private void m_button8_Click(object sender, RoutedEventArgs e)
        {
            Value = 8;
        }

        private void m_button9_Click(object sender, RoutedEventArgs e)
        {
            Value = 9;
        }

        private void m_button10_Click(object sender, RoutedEventArgs e)
        {
            Value = 10;
        }

        private void m_button0_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 0)
            {
                m_hoverValue = 0;
                SetStars(m_hoverValue);
            }
        }

        private void m_button1_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 1)
            {
                m_hoverValue = 1;
                SetStars(m_hoverValue);
            }
        }

        private void m_button2_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 2)
            {
                m_hoverValue = 2;
                SetStars(m_hoverValue);
            }
        }

        private void m_button3_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 3)
            {
                m_hoverValue = 3;
                SetStars(m_hoverValue);
            }
        }

        private void m_button4_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 4)
            {
                m_hoverValue = 4;
                SetStars(m_hoverValue);
            }
        }

        private void m_button5_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 5)
            {
                m_hoverValue = 5;
                SetStars(m_hoverValue);
            }
        }

        private void m_button6_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 6)
            {
                m_hoverValue = 6;
                SetStars(m_hoverValue);
            }
        }

        private void m_button7_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 7)
            {
                m_hoverValue = 7;
                SetStars(m_hoverValue);
            }
        }

        private void m_button8_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 8)
            {
                m_hoverValue = 8;
                SetStars(m_hoverValue);
            }
        }

        private void m_button9_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 9)
            {
                m_hoverValue = 9;
                SetStars(m_hoverValue);
            }
        }

        private void m_button10_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouseHovering = true;
            if (m_hoverValue != 10)
            {
                m_hoverValue = 10;
                SetStars(m_hoverValue);
            }
        }

        #endregion

        #region Overridden Methods

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ValueProperty)
            {
                if (m_mouseHovering == false)
                    SetStars(Value);
            }
        }

        #endregion

        #region Private Functions

        private void SetStars(int value)
        {
            m_starFilled1.Visibility = value >= 1 ? Visibility.Visible : Visibility.Hidden;
            m_starFilled2.Visibility = value >= 2 ? Visibility.Visible : Visibility.Hidden;
            m_starFilled3.Visibility = value >= 3 ? Visibility.Visible : Visibility.Hidden;
            m_starFilled4.Visibility = value >= 4 ? Visibility.Visible : Visibility.Hidden;
            m_starFilled5.Visibility = value >= 5 ? Visibility.Visible : Visibility.Hidden;
            m_starFilled6.Visibility = value >= 6 ? Visibility.Visible : Visibility.Hidden;
            m_starFilled7.Visibility = value >= 7 ? Visibility.Visible : Visibility.Hidden;
            m_starFilled8.Visibility = value >= 8 ? Visibility.Visible : Visibility.Hidden;
            m_starFilled9.Visibility = value >= 9 ? Visibility.Visible : Visibility.Hidden;
            m_starFilled10.Visibility = value >= 10 ? Visibility.Visible : Visibility.Hidden;
        }

        #endregion

        #region Private Members

        private bool m_mouseHovering = false;
        private int m_hoverValue = 0;

        #endregion

    }
}
