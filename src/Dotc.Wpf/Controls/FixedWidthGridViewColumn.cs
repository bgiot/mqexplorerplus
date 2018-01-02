using System.Windows;
using System.Windows.Controls;

namespace Dotc.Wpf.Controls
{
    public class FixedWidthGridViewColumn : GridViewColumn
    {

        static FixedWidthGridViewColumn()
        {
            WidthProperty.OverrideMetadata(typeof(FixedWidthGridViewColumn),
                new FrameworkPropertyMetadata(null, OnCoerceWidth));
        }

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            var fwc = o as FixedWidthGridViewColumn;
            return fwc?.FixedWidth ?? 0.0;
        }

        public double FixedWidth
        {
            private get { return (double)GetValue(FixedWidthProperty); }
            set { SetValue(FixedWidthProperty, value); }
        }

        public static readonly DependencyProperty FixedWidthProperty =
            DependencyProperty.Register("FixedWidth", typeof(double), typeof(FixedWidthGridViewColumn),
            new FrameworkPropertyMetadata(double.NaN, OnFixedWidthChanged));

        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var fwc = o as FixedWidthGridViewColumn;
            fwc?.CoerceValue(WidthProperty);
        }

    }
}
