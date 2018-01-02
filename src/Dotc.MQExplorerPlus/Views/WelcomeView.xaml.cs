#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel.Composition;
using System.Windows.Controls;
using Dotc.MQExplorerPlus.Core.Views;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for WelcomeView.xaml
    /// </summary>
    [Export(typeof(IWelcomeView)), PartCreationPolicy(CreationPolicy.NonShared)]

    public partial class WelcomeView : UserControl, IWelcomeView
    {
        public WelcomeView()
        {
            InitializeComponent();
        }
    }
}
