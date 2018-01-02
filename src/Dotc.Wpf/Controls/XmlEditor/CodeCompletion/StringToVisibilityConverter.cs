﻿


using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	[ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
        	if (!(value is string))
        		return value == null ? Visibility.Collapsed : Visibility.Visible;
        	return string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
