#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core;
using Dotc.MQExplorerPlus.Core.Views;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for ChannelResolveParametersView.xaml
    /// </summary>

    public partial class ChannelResolveParametersView : UserControl, IChannelResolveParametersView
    {
        public ChannelResolveParametersView()
        {
            InitializeComponent();
            uxAction.ItemsSource = new List<LabelValuePair<bool>>
            {
                new LabelValuePair<bool> { Label = "Rollback", Value=false },
                new LabelValuePair<bool> { Label = "Commit", Value = true },
            };
        }
    }
}
