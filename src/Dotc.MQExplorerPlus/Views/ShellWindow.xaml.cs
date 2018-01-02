#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel.Composition;
using System.Windows;
using Dotc.Common;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.ViewModels;
using Dotc.MQExplorerPlus.Core.Views;
using Xceed.Wpf.Toolkit;
using System;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    [Export(typeof(IShellView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ShellWindow : Window, IShellView
    {
        public ShellWindow()
        {
            InitializeComponent();
        }

        private void CenterModal(ChildWindow cw)
        {
            if (cw.Visibility == Visibility.Visible)
            {
                var ps = (IViewService)cw.DataContext;
                cw.Left = (ActualWidth - cw.DesiredSize.Width) / 2;
                cw.Top =  (ActualHeight - (cw.DesiredSize.Height + 100)) / 2;
            }
        }

        private void ChildWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (UxModalWindow.Visibility == Visibility.Visible)
            {
                UxModalWindow.InvalidateMeasure();
                CenterModal(UxModalWindow);
            }
        }


        private ShellViewModel ViewModel
        {
            get { return (ShellViewModel)this.DataContext; }
        }

        private void UxModalWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CenterModal(UxModalWindow);
        }

        private void UxModalWindow_CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.ViewService.CurrentModalViewModel.DoCancel();
            e.Handled = true;
        }
    }
}
