#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel.Composition;
using Dotc.MQExplorerPlus.Core.ViewModels;
using Dotc.MQExplorerPlus.Core.Views;
using Xceed.Wpf.AvalonDock;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    [Export(typeof(IMainView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MainView : /*UserControl,*/ IMainView
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void DockingManager_DocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            var vm = ((IView)e.Document.Content).DataContext as DocumentViewModel;
            if (vm != null) e.Cancel = (vm.CanClose() == false);
        }

        private void DockingManager_DocumentClosed(object sender, DocumentClosedEventArgs e)
        {
            var vm = ((IView)e.Document.Content).DataContext as DocumentViewModel;
            vm?.Close();
        }
    }
}
