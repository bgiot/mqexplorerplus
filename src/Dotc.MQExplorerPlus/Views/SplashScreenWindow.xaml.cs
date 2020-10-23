#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Windows;
using Dotc.MQExplorerPlus.Core.Models;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for SplashScreenWindow.xaml
    /// </summary>
    public partial class SplashScreenWindow : Window
    {
        public SplashScreenWindow()
        {
            InitializeComponent();
            uxVersionInfo.Text = "Version " + ApplicationInfo.Version;
            uxTitle.Text = ApplicationInfo.ProductName;
            uxProgress.IsIndeterminate = true;
        }
    }
}
