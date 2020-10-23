#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQ;
using Dotc.MQ.Websphere;
using Dotc.MQExplorerPlus.Core;
using Dotc.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for ChannelListView.xaml
    /// </summary>
    public partial class ChannelListView : UserControl
    {
        public ChannelListView()
        {
            InitializeComponent();
            UxChannelTypesList.ItemsSource = FilterTypesList;
            UxChannelStatusList.ItemsSource = FilterStatusList;
        }

        private List<LabelValuePair<int?>> FilterTypesList => new List<LabelValuePair<int?>>
        {
            new LabelValuePair<int?> { Label = "Any", Value = null},
            BuildTypeItem(WsChannelType.Amqp ),
            BuildTypeItem(WsChannelType.ClientConnection ),
            BuildTypeItem(WsChannelType.ClusterReceiver ),
            BuildTypeItem(WsChannelType.ClusterSender ),
            BuildTypeItem(WsChannelType.Receiver ),
            BuildTypeItem(WsChannelType.Requester ),
            BuildTypeItem(WsChannelType.Sender ),
            BuildTypeItem(WsChannelType.Server ),
            BuildTypeItem(WsChannelType.ServerConnection )
       };

        private List<LabelValuePair<ChannelStatus?>> FilterStatusList => new List<LabelValuePair<ChannelStatus?>>
        {
            new LabelValuePair<ChannelStatus?> { Label = "Any", Value = null},
            BuildStatusItem(ChannelStatus.Binding),
            BuildStatusItem(ChannelStatus.Disconnected),
            BuildStatusItem(ChannelStatus.Inactive),
            BuildStatusItem(ChannelStatus.Initializing),
            BuildStatusItem(ChannelStatus.Paused),
            BuildStatusItem(ChannelStatus.Requesting),
            BuildStatusItem(ChannelStatus.Retrying),
            BuildStatusItem(ChannelStatus.Running),
            BuildStatusItem(ChannelStatus.Starting),
            BuildStatusItem(ChannelStatus.Stopped),
            BuildStatusItem(ChannelStatus.Stopping),
            BuildStatusItem(ChannelStatus.Switching),
        };

        private LabelValuePair<int?> BuildTypeItem(int value)
        {
            return new LabelValuePair<int?> { Label = WsChannelType.GetName(value), Value = value };
        }

        private LabelValuePair<ChannelStatus?> BuildStatusItem(ChannelStatus value)
        {
            return new LabelValuePair<ChannelStatus?> { Label = value.ToString(), Value = value };
        }

        private void ListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    var lv = sender as ListView;
            //    var vm = lv?.DataContext as ChannelListViewModel;
            //    //if (vm != null) RelayCommand.Execute(vm.SelectedBrowseCommand);
            //    e.Handled = true;
            //}


            //if (e.Key == Key.Space)
            //{
            //    var lv = sender as ListView;

            //    if (lv != null)
            //    {
            //        var items = lv.SelectedItems.Cast<SelectableItem>().ToArray();

            //        if (items.Any(x => x.Selected == false))
            //        {
            //            foreach (var item in items)
            //            {
            //                item.Selected = true;
            //            }
            //        }
            //        else
            //        {
            //            foreach (var item in items)
            //            {
            //                item.Selected = false;
            //            }
            //        }
            //    }
            //    e.Handled = true;
            //}
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e == null || e.Handled)
                return;

            var item = ((DependencyObject)e.OriginalSource).FindAncestor<ListViewItem>();
            if (item == null)
                return;

            if (item.Focusable && !item.IsFocused)
                item.Focus();
        }
    }
}
