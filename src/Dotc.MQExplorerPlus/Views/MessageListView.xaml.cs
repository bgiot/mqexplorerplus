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
using System.Windows.Input;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Views;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Shapes;
using Dotc.Wpf.Controls;
using Dotc.MQExplorerPlus.Core;

namespace Dotc.MQExplorerPlus.Views
{
    [Export(typeof(IMessageListView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MessageListView : UserControl, IMessageListView
    {
        public MessageListView()
        {
            InitializeComponent();

            UxPrioritiesList.ItemsSource = FilterPriorities;
        }


        private List<LabelValuePair<int?>> FilterPriorities => new List<LabelValuePair<int?>>
        {
            new LabelValuePair<int?> { Label = "Any", Value = null},
            new LabelValuePair<int?> { Label = "0", Value=0 },
            new LabelValuePair<int?> { Label = "1", Value=1 },
            new LabelValuePair<int?> { Label = "2", Value=2 },
            new LabelValuePair<int?> { Label = "3", Value=3 },
            new LabelValuePair<int?> { Label = "4", Value=4 },
            new LabelValuePair<int?> { Label = "5", Value=5 },
            new LabelValuePair<int?> { Label = "6", Value=6 },
            new LabelValuePair<int?> { Label = "7", Value=7 },
            new LabelValuePair<int?> { Label = "8", Value=8 },
            new LabelValuePair<int?> { Label = "9", Value=9 }
        };

        private void ListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (sender is ListView lv)
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

        private void uxExpandAll_Click(object sender, RoutedEventArgs e)
        {
            uxJsonViewer.ExpandAll();
        }

        private void uxCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            uxJsonViewer.CollapseAll();
        }
    }
}
