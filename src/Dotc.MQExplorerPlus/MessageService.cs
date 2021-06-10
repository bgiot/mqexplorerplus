#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Windows;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.Wpf;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace Dotc.MQExplorerPlus
{

    public sealed class MessageService : IMessageService
    {
        private static MessageBoxResult MessageBoxResult => MessageBoxResult.None;

        public MessageService()
        {
        }

        public event EventHandler Before;
        public event EventHandler After;

        private Style GetStyle()
        {
            return System.Windows.Application.Current.FindResource("MessageBoxCustomStyle") as Style;
        }

        private void OnBefore()
        {
            Before?.Invoke(this, new EventArgs());
        }

        private void OnAfter()
        {
            After?.Invoke(this, new EventArgs());
        }

        private Window GetWindow() 
        {
            return null;
            //return System.Windows.Application.Current.MainWindow;
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="owner">The window that owns this Message Window.</param>
        /// <param name="message">The message.</param>
        public void ShowMessage(string message)
        {
            OnBefore();
            var ownerWindow = GetWindow();
            if (ownerWindow != null)
            {
                UIDispatcher.Execute(() =>
                {
                    MessageBox.Show(ownerWindow, message, ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.None, GetStyle());
                });
            }
            else
            {
                UIDispatcher.Execute(() =>
                {
                    MessageBox.Show(message, ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.None,
                    MessageBoxResult, GetStyle());
                });
            }
            OnAfter();
        }

        /// <summary>
        /// Shows the message as warning.
        /// </summary>
        /// <param name="owner">The window that owns this Message Window.</param>
        /// <param name="message">The message.</param>
        public void ShowWarning(string message)
        {
            OnBefore();
            var ownerWindow = GetWindow();
            if (ownerWindow != null)
            {
                UIDispatcher.Execute(() =>
                {
                    MessageBox.Show(ownerWindow, message, ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning,
                    MessageBoxResult, GetStyle());
                });
            }
            else
            {
                UIDispatcher.Execute(() =>
                {
                    MessageBox.Show(message, ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning,
                    MessageBoxResult, GetStyle());
                });
            }
            OnAfter();
        }

        /// <summary>
        /// Shows the message as error.
        /// </summary>
        /// <param name="owner">The window that owns this Message Window.</param>
        /// <param name="message">The message.</param>
        public void ShowError(string message)
        {
            OnBefore();
            var ownerWindow = GetWindow();
            if (ownerWindow != null)
            {
                UIDispatcher.Execute(() =>
                {
                    MessageBox.Show(ownerWindow, message, ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult, GetStyle());
                });
            }
            else
            {
                UIDispatcher.Execute(() =>
                {
                    MessageBox.Show(message, ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult, GetStyle());
                });
            }
            OnAfter();
        }

        /// <summary>
        /// Shows the specified question.
        /// </summary>
        /// <param name="owner">The window that owns this Message Window.</param>
        /// <param name="message">The question.</param>
        /// <returns><c>true</c> for yes, <c>false</c> for no and <c>null</c> for cancel.</returns>
        public bool? ShowQuestion(string message)
        {
            OnBefore();

            var ownerWindow = GetWindow();
            var result = MessageBoxResult.None;
            if (ownerWindow != null)
            {
                UIDispatcher.Execute(() =>
                {
                    result = MessageBox.Show(ownerWindow, message, ApplicationInfo.ProductName, MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question, MessageBoxResult.Cancel, GetStyle());
                });
            }
            else
            {
                UIDispatcher.Execute(() =>
                {
                    result = MessageBox.Show(message, ApplicationInfo.ProductName, MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question, MessageBoxResult.Cancel, GetStyle());
                });
            }

            OnAfter();

            if (result == MessageBoxResult.Yes) { return true; }
            if (result == MessageBoxResult.No) { return false; }

            return null;
        }

        /// <summary>
        /// Shows the specified yes/no question.
        /// </summary>
        /// <param name="owner">The window that owns this Message Window.</param>
        /// <param name="message">The question.</param>
        /// <returns><c>true</c> for yes and <c>false</c> for no.</returns>
        public bool ShowYesNoQuestion(string message)
        {
            OnBefore();

            var ownerWindow = GetWindow();
            var result = MessageBoxResult.None;
            if (ownerWindow != null)
            {
                UIDispatcher.Execute(() =>
               {
                   result = MessageBox.Show(ownerWindow, message, ApplicationInfo.ProductName, MessageBoxButton.YesNo,
                       MessageBoxImage.Question, MessageBoxResult.No, GetStyle());
               });
            }
            else
            {
                UIDispatcher.Execute(() =>
                {
                    result = MessageBox.Show(message, ApplicationInfo.ProductName, MessageBoxButton.YesNo,
                    MessageBoxImage.Question, MessageBoxResult.No, GetStyle());
                });
            }

            OnAfter();

            return result == MessageBoxResult.Yes;
        }
    }
}
