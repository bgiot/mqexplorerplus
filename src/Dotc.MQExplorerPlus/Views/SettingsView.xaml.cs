#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel.Composition;
using Dotc.MQExplorerPlus.Core.Views;

namespace Dotc.MQExplorerPlus.Views
{
    [Export(typeof(ISettingsView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SettingsView : /*UserControl,*/ ISettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }
    }
}
