#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Mvvm;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQ;
using static System.FormattableString;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{

    public abstract class ViewModel : ViewModelBase 
    {
        private string _title;

        protected ViewModel(IView view, IApplicationController appController)
        {
            if (view == null) { throw new ArgumentNullException(nameof(view)); }
            if (appController == null) { throw new ArgumentNullException(nameof(appController)); }

            View = view;
            view.DataContext = this;
            App = appController;
        }

        public IView View { get; private set; }

        public IApplicationController App { get; private set; }

        public string Title
        {
            get { return _title; }
            set
            {
                SetPropertyAndNotify(ref _title, value);
            }
        }

        private bool HandleError(Exception ex)
        {
            if (ex == null) return true;

            TraceHelper.Log(ex);

            if (ex is MqException || ex is MQExplorerPlusException)
            {
                ShowErrorMessage("Error", ex); 
                return true;
            }

            return false;

        }

        protected string FormatErrorMessage(string message, Exception ex = null)
        {
            if (ex == null)
            {
                return message;
            }
            else
            {
                var mqEx = ex as Dotc.MQ.MqException;
                if (mqEx != null)
                {
                    return Invariant($"{message} ({mqEx.ReasonCode}-{mqEx.Message})");
                }
                var appEx = ex as MQExplorerPlusException;
                if (appEx != null)
                {
                    if (appEx.InnerException != null)
                    {
                        return Invariant($"{message} ({appEx.Message}-{appEx.InnerException.Message})");              
                    }
                    else
                    {
                        return Invariant($"{message} ({appEx.Message})");
                    }
                }
                else
                {
                    return Invariant($"{message} ({ex.Message})");
                }

            }
        }
        protected virtual void ShowErrorMessage(string message, Exception ex = null)
        {       
            App.MessageService.ShowError(FormatErrorMessage(message, ex));
        }

        internal bool ExecuteGuarded(Action act)
        {
            if (act == null) return true;

            try
            {
                act.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                if (!HandleError(ex))
                {
                    throw;
                }

                return false;
            }
        }
    }
}
