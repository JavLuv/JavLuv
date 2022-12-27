using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;

namespace JavLuv
{
    public static class TextBoxBehavior
    {
        public static readonly DependencyProperty TripleClickSelectAllProperty = DependencyProperty.RegisterAttached(
            "TripleClickSelectAll", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = d as TextBox;
            if (tb != null)
            {
                var enable = (bool)e.NewValue;
                if (enable)
                {
                    tb.PreviewMouseLeftButtonDown += OnTextBoxMouseDown;
                }
                else
                {
                    tb.PreviewMouseLeftButtonDown -= OnTextBoxMouseDown;
                }
            }
        }

        private static void OnTextBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
            {
                ((TextBox)sender).SelectAll();
            }
        }

        public static void SetTripleClickSelectAll(DependencyObject element, bool value)
        {
            element.SetValue(TripleClickSelectAllProperty, value);
        }

        public static bool GetTripleClickSelectAll(DependencyObject element)
        {
            return (bool)element.GetValue(TripleClickSelectAllProperty);
        }
    }
}
