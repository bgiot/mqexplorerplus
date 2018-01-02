using System;
using System.Windows.Data;

namespace Dotc.Wpf.Controls.TreeListView
{
    /// <summary>
    /// Represents a converter thats converts an TreeListViewExpander in a TreeListViewItem level to its left distance 
    /// </summary>
    public class TreeListViewConverter : IValueConverter
    {
        /// <summary>
        /// Converts a TreeListViewExpander in a TreeListViewItem level to the left distance
        /// </summary>
        /// <param name="value">The TreeListViewExpander value to convert</param>
        /// <param name="targetType">This parameter is not used</param>
        /// <param name="parameter">This parameter is not used</param>
        /// <param name="culture">This parameter is not used</param>
        /// <returns>The left distance for the given level</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var container = VisualTreeAssist.FindParent<TreeListViewItem>(value);
                return container?.Level * 10;
            }
            return null;
        }

        /// <summary>
        /// ConvertBack is not supported
        /// </summary>
        /// <param name="value">This parameter is not used</param>
        /// <param name="targetType">This parameter is not used</param>
        /// <param name="parameter">This parameter is not used</param>
        /// <param name="culture">This parameter is not used</param>
        /// <returns>Nothing; NotImplementedException is thrown</returns>
        /// <exception cref="NotImplementedException">ConvertBack is not supported</exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
