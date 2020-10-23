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
    /// Interaction logic for PutMessageView.xaml
    /// </summary>

    public partial class PutMessageView : /*UserControl,*/ IPutMessageView
    {
        public PutMessageView()
        {
            InitializeComponent();
            WeakEventManager<PutMessageView, RoutedEventArgs>
                .AddHandler(this, "Loaded", PutMessageView_Loaded);
        }

        void PutMessageView_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(UxStartFocusField);
        }
    }
}
