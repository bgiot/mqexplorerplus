#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace Dotc.MQExplorerPlus.Styles
{
    internal static class LocalExtensions
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            var window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
            if (window != null) action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }

    public partial class WindowStyle
    {
        private void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
                sender.ForWindowFromTemplate(SystemCommands.CloseWindow);
        }

        private void IconMouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                var point = element.PointToScreen(new Point(element.ActualWidth / 2, element.ActualHeight));
                sender.ForWindowFromTemplate(w => SystemCommands.ShowSystemMenu(w, point));
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            var w = ((Window)sender);

            if (w.ResizeMode == ResizeMode.NoResize)
            {
                var minButton = (System.Windows.Controls.Button)w.Template.FindName("MinButton", w);
                minButton.Visibility = Visibility.Hidden;

                var maxButton = (System.Windows.Controls.Button)w.Template.FindName("MaxButton", w);
                maxButton.Visibility = Visibility.Hidden;
            }

            if (w.ResizeMode == ResizeMode.CanMinimize)
            {
                var maxButton = (System.Windows.Controls.Button)w.Template.FindName("MaxButton", w);
                maxButton.Visibility = Visibility.Hidden;
            }

            WeakEventManager<Window, EventArgs>
            .AddHandler((Window)sender, "StateChanged", WindowStateChanged);
            WindowStateChanged(sender, EventArgs.Empty);
        }

        private void WindowStateChanged(object sender, EventArgs e)
        {
            var w = ((Window)sender);

            var handle = w.GetWindowHandle();
            var containerBorder = (Border)w.Template.FindName("PART_Container0", w);

            if (w.WindowState == WindowState.Maximized)
            {
                // Make sure window doesn't overlap with the taskbar.
                var screen = Screen.FromHandle(handle);
                if (screen.Primary)
                {
                    containerBorder.Padding = new Thickness(7,
                        7,
                        0,
                        (SystemParameters.PrimaryScreenHeight - SystemParameters.MaximizedPrimaryScreenHeight) + 14);
                }
                else
                {
                    containerBorder.Padding = new Thickness(7,
                        7,
                        0,
                        0);

                }
                w.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                containerBorder.Padding = new Thickness(0, 7, 0, 0);
                w.ResizeMode = ResizeMode.CanResize;
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(SystemCommands.CloseWindow);
        }

        private void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(SystemCommands.MinimizeWindow);
        }

        private void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w =>
                {
                    if (w.WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(w);
                    else SystemCommands.MaximizeWindow(w);
                });
        }
    }
}