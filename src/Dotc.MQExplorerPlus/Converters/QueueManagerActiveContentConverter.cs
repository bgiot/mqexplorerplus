#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Dotc.MQExplorerPlus.Core.ViewModels;
using Dotc.MQExplorerPlus.Views;

namespace Dotc.MQExplorerPlus.Converters
{
    public class QueueManagerActiveContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QueueListView)
                return QueueManagerContentType.Queues;

            if (value is ChannelListView)
                return QueueManagerContentType.Channels;

            if (value is ListenerListView)
                return QueueManagerContentType.Listeners;

            return QueueManagerContentType.Unknown;
        }
    }
}
