#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Windows.Controls;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Views;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for WelcomeView.xaml
    /// </summary>

    public partial class WelcomeView : UserControl, IWelcomeView
    {
        public WelcomeView()
        {
            InitializeComponent();
            uxTitle.Text = ApplicationInfo.ProductName;
        }
    }
}
