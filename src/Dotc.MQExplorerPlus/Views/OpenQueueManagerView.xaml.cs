#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Windows;
using System.Windows.Input;
using Dotc.MQExplorerPlus.Core.Views;
using System.Windows.Controls;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for OpenQueueManagerView.xaml
    /// </summary>

    public partial class OpenQueueManagerView : UserControl, IOpenQueueManagerView
    {
        public OpenQueueManagerView()
        {
            InitializeComponent();
            WeakEventManager<OpenQueueManagerView, RoutedEventArgs>
                .AddHandler(this, "Loaded", OpenQueueManagerView_Loaded);
        }

        private void OpenQueueManagerView_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(UxStartFocusField);
        }
    }
}
