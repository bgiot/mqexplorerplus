#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Windows;
using System.Windows.Input;
using Dotc.MQExplorerPlus.Core.Views;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for OpenQueueView.xaml
    /// </summary>

    public partial class OpenQueueView : /*UserControl,*/ IOpenQueueView
    {
        public OpenQueueView()
        {
            InitializeComponent();
            WeakEventManager<OpenQueueView, RoutedEventArgs>
                .AddHandler(this, "Loaded", OpenQueueView_Loaded);
        }

        private void OpenQueueView_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(UxStartFocusField);
        }
    }
}
