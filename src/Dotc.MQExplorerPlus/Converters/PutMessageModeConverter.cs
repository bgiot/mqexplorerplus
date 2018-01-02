#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Globalization;
using System.Windows.Data;
using Dotc.MQExplorerPlus.Core.ViewModels;

namespace Dotc.MQExplorerPlus.Converters
{
    public class PutMessageModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PutMessageMode mode = (PutMessageMode) value;
            switch (mode)
            {
                case PutMessageMode.Single:
                    return 0; // Tab Index 0
                case PutMessageMode.FromFile:
                    return 1; // Tab Index 1
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;
            switch (index)
            {
                case 0:
                return PutMessageMode.Single;
                case 1:
                return PutMessageMode.FromFile;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
