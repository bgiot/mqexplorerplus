#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Common;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>

    public partial class AboutView : /* UserControl, */ IAboutView
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            e.Uri.OpenInBrowser();
        }
    }
}
