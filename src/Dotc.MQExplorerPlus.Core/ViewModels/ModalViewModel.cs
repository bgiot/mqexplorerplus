#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.MQExplorerPlus.Core.Controllers;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public abstract class ModalViewModel : ViewModel
    {
        protected ModalViewModel(IModalView view , IApplicationController appController)
            : base(view, appController)
        {
            OkCommand = CreateCommand(DoOk, OkAllowed );
            CancelCommand = CreateCommand(DoCancel);
            Result = MessageBoxResult.None;
        }

        public virtual bool ShowDefaultButtons => true;

        public MessageBoxResult Result {get; private set;}

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                SetPropertyAndNotify(ref _isBusy, value);
                OnPropertyChanged(nameof(IsIdle));
            }
        }

        public bool IsIdle
        {
            get { return !IsBusy; }
        }

        protected async Task<bool> ExecuteGuardedAsync(Action action)
        {
            IsBusy = true;
            var result = await Task.Run(() =>
            {
                return ExecuteGuarded(() =>
                {
                    App.ShellService.WithGlobalBusy(() =>
                    {
                        action.Invoke();
                    });
                });
            });
            IsBusy = false;
            CommandManager.InvalidateRequerySuggested();
            return result;
        }

        protected virtual bool OkAllowed()
        {
            return true;
        }

        public virtual string OkLabel => "Ok";

        public virtual string CancelLabel => "Cancel";

        public void DoOk()
        {
            var args = new CancelEventArgs();
            OnOk(args);
            if (args.Cancel == false)
            {
                Result = MessageBoxResult.OK;
                Close();
            }
        }

        public void DoCancel()
        {
            var args = new CancelEventArgs();
            OnCancel(args);
            if (args.Cancel == false)
            {
                Result = MessageBoxResult.Cancel;
                Close();
            }
        }

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnOk(CancelEventArgs e)
        {
        }

        protected virtual void OnCancel(CancelEventArgs e)
        {
        }

        public virtual void OnOpened()
        { }

        public event EventHandler Closed;
               
        public ICommand CancelCommand
        {
            get;
            private set;
        }

        public ICommand OkCommand
        {
            get;
            private set;
        }

    }
}
