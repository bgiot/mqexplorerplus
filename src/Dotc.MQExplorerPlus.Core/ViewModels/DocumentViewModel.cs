#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.Mvvm;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public abstract class DocumentViewModel : ViewModel
    {

        private string _uniqueId;
        private bool _isActive;

        protected DocumentViewModel(IDocumentView view, IApplicationController appController) : base(view, appController)
        {
            _uniqueId = Guid.NewGuid().ToString("B");
            Errors = new ErrorViewModel();
        }


        public bool CanClose()
        {
            if (LocalBusy) return false;

            var e = new CancelEventArgs();
            OnClosing(e);
            return e.Cancel == false;
        }

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnClosing(CancelEventArgs e)
        {
            App.ShellService.KeyboardCommands = null;
            App.ShellService.StatusInfo = null;
            App.ShellService.AutomaticRefreshSubscriber = null;
        }

        public event EventHandler Closed;

        public string UniqueId
        {
            get { return _uniqueId; }
            set { SetPropertyAndNotify(ref _uniqueId, value); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetPropertyAndNotify(ref _isActive, value); }
        }

        public virtual void OnActivate()
        {
            App.ShellService.KeyboardCommands = this as IKeyboardCommands;
            App.ShellService.AutomaticRefreshSubscriber = this as ISupportAutomaticRefresh;
            App.ShellService.StatusInfo = this as IStatusInfo;
            IsActive = true;
        }

        public virtual void OnDeactivate()
        {
            IsActive = false;
        }
        protected override void ShowErrorMessage(string message, Exception ex = null)
        {
            Errors.AddErrorMessage(FormatErrorMessage(message, ex));
        }

        private ICommand _cancelRunningActionCommand;
        public ICommand CancelRunningActionCommand
        {
            get
            {
                if (_cancelRunningActionCommand == null)
                {
                    _cancelRunningActionCommand = new RelayCommand(() => CancelRunningAction(), CanCancelRunningAction);
                }
                return _cancelRunningActionCommand;
            }
        }

        private void CancelRunningAction()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private bool CanCancelRunningAction()
        {
            return SupportActionCancellation && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;
        }

        private bool _supportActionCancellation;

        public bool SupportActionCancellation
        {
            get { return _supportActionCancellation; }
            set { SetPropertyAndNotify(ref _supportActionCancellation, value); }
        }

        private CancellationTokenSource _cancellationTokenSource;

        public ErrorViewModel Errors { get; private set; }

        private bool _localBusy;

        public bool LocalBusy
        {
            get { return _localBusy; }
            set
            {
                SetPropertyAndNotify(ref _localBusy, value);
                OnPropertyChanged(nameof(LocalIdle));
                if (_localBusy)
                {
                    App.ShellService.DisableAutomaticRefresh();
                }
                else
                {
                    App.ShellService.EnableAutomaticRefresh();

                }
            }
        }

        public bool LocalIdle
        {
            get { return !LocalBusy; }
        }

        internal Task ExecuteAsync(Action action)
        {
            return ExecuteAsync((ct) => action(), false);
        }


        internal Task ExecuteAsync(Action<CancellationToken> action, bool supportCancellation = true)
        {
            bool ownerOfBusy = (LocalBusy == false);

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            SupportActionCancellation = supportCancellation;

            if (ownerOfBusy)
            {
                LocalBusy = true;
                _cancellationTokenSource = new CancellationTokenSource();
            }

            Task.Run(() =>
            {
                try
                {
                    var result = ExecuteGuarded(() => action(_cancellationTokenSource.Token)); ;
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        tcs.SetResult(result);
                    }
                }
                catch(Exception ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    if (ownerOfBusy)
                    {
                        _cancellationTokenSource = null;
                        LocalBusy = false;
                    }
                }
            });

            return tcs.Task;

        }

        internal bool Execute(Action action)
        {
            bool ownerOfBusy = (LocalBusy == false);

            Func<bool> guardedAction = () => { return ExecuteGuarded(action); };

            LocalBusy = true;
            try
            {
                return guardedAction.Invoke();
            }
            finally
            {
                if (ownerOfBusy)
                    LocalBusy = false;

            }

        }
    }
}
