#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dotc.MQ;
using Dotc.MQ.Websphere;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.ViewModels;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Mvvm;
using Dotc.Wpf;
using System.Windows.Controls.Ribbon;
using System.Windows.Shapes;
using Dotc.Wpf.Controls;
using Dotc.MQExplorerPlus.Core;

namespace Dotc.MQExplorerPlus.Views
{
    public partial class QueueListView : UserControl
    {
        public QueueListView()
        {
            InitializeComponent();

            UxQueueTypesList.ItemsSource = FilterTypesList;
            UxGetStatusList.ItemsSource = FilterGetPutStatusList;
            UxPutStatusList.ItemsSource = FilterGetPutStatusList;
        }

        private List<LabelValuePair<int?>> FilterTypesList => new List<LabelValuePair<int?>>
        {
            new LabelValuePair<int?> { Label = "Any", Value = null},
            new LabelValuePair<int?> { Label = "Local", Value=WsQueueType.Local },
            new LabelValuePair<int?> { Label = "Alias", Value=WsQueueType.Alias },
            new LabelValuePair<int?> { Label = "Remote", Value=WsQueueType.Remote },
            new LabelValuePair<int?> { Label = "Transmission", Value=WsQueueType.Transmission },
            new LabelValuePair<int?> { Label = "Model", Value=WsQueueType.Model }
        };


        private List<LabelValuePair<GetPutStatus?>> FilterGetPutStatusList => new List<LabelValuePair<GetPutStatus?>>
        {
            new LabelValuePair<GetPutStatus?> { Label = "Any", Value = null },
            new LabelValuePair<GetPutStatus?> { Label = "Allowed", Value = GetPutStatus.Allowed },
            new LabelValuePair<GetPutStatus?> { Label = "Inhibited", Value = GetPutStatus.Inhibited }
        };

        private void ListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var lv = sender as ListView;
                var vm = lv?.DataContext as QueueListViewModel;
                if (vm != null) RelayCommand.Execute(vm.SelectedBrowseCommand);
                e.Handled = true;
            }


            if (e.Key == Key.Space)
            {
                var lv = sender as ListView;

                if (lv != null)
                {
                    var items = lv.SelectedItems.Cast<SelectableItem>().ToArray();

                    if (items.Any(x => x.Selected == false))
                    {
                        foreach (var item in items)
                        {
                            item.Selected = true;
                        }
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            item.Selected = false;
                        }
                    }
                }
                e.Handled = true;
            }
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

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var lvi = sender as ListViewItem;
                var lv = lvi?.GetVisualParent() as VirtualizingStackPanel;
                var vm = lv?.DataContext as QueueListViewModel;
                if (vm != null) RelayCommand.Execute(vm.SelectedBrowseCommand);
                e.Handled = true;
            }
        }
    }
}
