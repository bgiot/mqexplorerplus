#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Globalization;
using System.Windows.Data;
using Dotc.MQExplorerPlus.Core.Models;

namespace Dotc.MQExplorerPlus.Converters
{
    public class CheckQueueDepthTresholdConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var qi = values?[0] as QueueInfo;
            if (qi?.Depth == null) return false;

            var threshold = values?[1] as string;
            if (threshold == null) return false;

            threshold = threshold.Trim();

            return QueueDepthValidator.IsDepthOverThreshold(qi.Depth.Value, qi.ExtendedProperties.MaxDepth, threshold);

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
