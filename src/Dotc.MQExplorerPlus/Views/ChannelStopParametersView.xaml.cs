#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core;
using Dotc.MQExplorerPlus.Core.Views;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for ChannelStopSettingsView.xaml
    /// </summary>

    public partial class ChannelStopParametersView : UserControl, IChannelStopParametersView
    {
        public ChannelStopParametersView()
        {
            InitializeComponent();

            uxMode.ItemsSource = new List<LabelValuePair<ChannelStopMode>>
            {
                new LabelValuePair<ChannelStopMode> {Label="Normal", Value = ChannelStopMode.Normal},
                new LabelValuePair<ChannelStopMode> {Label="Force", Value = ChannelStopMode.Force},
                new LabelValuePair<ChannelStopMode> {Label="Terminate", Value = ChannelStopMode.Terminate},
            };

            uxFinalState.ItemsSource = new List<LabelValuePair<bool>>
            {
                new LabelValuePair<bool> {Label = "Stopped", Value=false},
                new LabelValuePair<bool> {Label = "Inactive", Value=true},
            };
        }
    }
}
