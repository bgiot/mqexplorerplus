#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dotc.Wpf;
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for ListenerListView.xaml
    /// </summary>
    public partial class ListenerListView : UserControl
    {
        public ListenerListView()
        {
            InitializeComponent();
            UxListenerStatusList.ItemsSource = FilterStatusList;
        }

        private List<LabelValuePair<ListenerStatus?>> FilterStatusList => new List<LabelValuePair<ListenerStatus?>>
        {
            new LabelValuePair<ListenerStatus?> { Label = "Any", Value = null},
            BuildStatusItem(ListenerStatus.Retrying),
            BuildStatusItem(ListenerStatus.Running),
            BuildStatusItem(ListenerStatus.Starting),
            BuildStatusItem(ListenerStatus.Stopping),
            BuildStatusItem(ListenerStatus.Stopped),
        };

        private LabelValuePair<ListenerStatus?> BuildStatusItem(ListenerStatus value)
        {
            return new LabelValuePair<ListenerStatus?> { Label = value.ToString(), Value = value };
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
