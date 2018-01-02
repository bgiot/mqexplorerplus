#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Mvvm;
using System.Windows;
using Dotc.MQExplorerPlus.Core.ViewModels;

namespace Dotc.MQExplorerPlus.Core.Services
{
    [Export(typeof(ShellService)), PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class ShellService : BindableBase, IDisposable
    {
        private bool _busy;
        private IShellView _shellView;
        private readonly object _synlock = new object();


        private ICommand _exitCommand;
        private ICommand _openQueueManagerCommand;
        private ICommand _openQueueCommand;
        private ICommand _openRemoteQueueManagerCommand;
        private ICommand _openRemoteQueueCommand;
        private ICommand _showParserEditorCommand;
        private ICommand _showSettingsCommand;
        private ICommand _showAboutCommand;
        private ICommand _useRecentConnectionCommand;
        private ICommand _clearRecentConnectionsCommand;

        private IKeyboardCommands _keyboardCommands;
        private ISupportAutomaticRefresh _automaticRefreshSubscriber;
        private IStatusInfo _statusInfo;

        [ImportingConstructor]
        public ShellService(UserSettings appSettings)
        {
            if (appSettings == null) throw new ArgumentNullException(nameof(appSettings));

            WeakEventManager<UserSettings, EventArgs>
                .AddHandler(appSettings, "OnSettingsChanged",
                (s, e) =>
                    {
                        Countdown.CountdownStart = ((UserSettings)s).AutoRefreshInterval;
                    });

            Countdown = new CountdownService(appSettings.AutoRefreshInterval);

            WeakEventManager<CountdownService, EventArgs>
                .AddHandler(Countdown, "Elapsed", _automaticRefreshTimer_Elapsed);

            AutomaticRefreshOn = false;

            GC.SuppressFinalize(this);

        }

        private CountdownService _countdown;
        public CountdownService Countdown { get { return _countdown; } private set { _countdown = value; } }

        private void _automaticRefreshTimer_Elapsed(object sender, EventArgs e)
        {
            InvokeAutomaticRefresh();
        }

        public void InvokeAutomaticRefresh()
        {
            if (AutomaticRefreshOn && IsIdle && AutomaticRefreshSubscriber != null)
            {
                CurrentDispatcher.Invoke(() => AutomaticRefreshSubscriber.OnAutomaticRefreshTriggered());
                Countdown.ResetCountdown();
            }
        }

        private bool _automaticRefreshOn;
        public bool AutomaticRefreshOn
        {
            get { return _automaticRefreshOn; }
            set
            {
                SetPropertyAndNotify(ref _automaticRefreshOn, value);
                if (AutomaticRefreshSubscriber != null)
                {
                    Countdown.IsOn = value;
                }
            }
        }

        public void EnableAutomaticRefresh()
        {
            Countdown.Allow(true);
        }

        public void DisableAutomaticRefresh()
        {
            Countdown.Disable();
        }

        public IShellView ShellView
        {
            get { return _shellView; }
            set
            {
                SetPropertyAndNotify(ref _shellView, value);
            }
        }

        public ICommand ExitCommand
        {
            get { return _exitCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _exitCommand, value); }
        }

        public ICommand OpenQueueManagerCommand
        {
            get { return _openQueueManagerCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _openQueueManagerCommand, value); }
        }

        public ICommand OpenQueueCommand
        {
            get { return _openQueueCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _openQueueCommand, value); }
        }

        public ICommand OpenRemoteQueueCommand
        {
            get { return _openRemoteQueueCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _openRemoteQueueCommand, value); }
        }

        public ICommand OpenRemoteQueueManagerCommand
        {
            get { return _openRemoteQueueManagerCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _openRemoteQueueManagerCommand, value); }
        }

        public ICommand ShowParserEditorCommand
        {
            get { return _showParserEditorCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _showParserEditorCommand, value); }
        }

        public ICommand ShowSettingsCommand
        {
            get { return _showSettingsCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _showSettingsCommand, value); }
        }

        public ICommand ShowAboutCommand
        {
            get { return _showAboutCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _showAboutCommand, value); }
        }
        public ICommand UseRecentConnectionCommand
        {
            get { return _useRecentConnectionCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _useRecentConnectionCommand, value); }
        }

        public ICommand ClearRecentConnectionsCommand
        {
            get { return _clearRecentConnectionsCommand ?? new DisabledCommand(); }
            set { SetPropertyAndNotify(ref _clearRecentConnectionsCommand, value); }
        }

        public IKeyboardCommands KeyboardCommands
        {
            get { return _keyboardCommands ?? new DisabledKeyboardCommands(); }
            set { SetPropertyAndNotify(ref _keyboardCommands, value); }
        }

        public ISupportAutomaticRefresh AutomaticRefreshSubscriber
        {
            get { return _automaticRefreshSubscriber; }
            set
            {
                SetPropertyAndNotify(ref _automaticRefreshSubscriber, value);
                if (value == null)
                {
                    Countdown.IsOn = false;
                }
                else
                {
                    Countdown.IsOn = AutomaticRefreshOn;
                    if (AutomaticRefreshOn)
                    {
                        InvokeAutomaticRefresh();
                    }
                }
            }
        }

        public IStatusInfo StatusInfo
        {
            get
            {
                return _statusInfo;
            }
            set { SetPropertyAndNotify(ref _statusInfo, value); }
        }

        public void WithGlobalBusy(Action action)
        {
            var ownerOfCursorChange = false;
            try
            {
                ownerOfCursorChange = SetWaitMouseCursor();
                action?.Invoke();
            }
            finally
            {
                if (ownerOfCursorChange) SetDefaultMouseCursor();
            }
        }

        Cursor _previous;
        private bool SetWaitMouseCursor()
        {
            if (IsBusy) return false;

            lock (_synlock)
            {

                if (IsBusy) return false;

                IsBusy = true;
                if (_previous == null)
                {
                    CurrentDispatcher.Invoke(() =>
                        {
                            _previous = Mouse.OverrideCursor;
                            Mouse.OverrideCursor = Cursors.Wait;
                        });

                    return true;
                }
                return false;
            }
        }

        private void SetDefaultMouseCursor()
        {
            if (!IsBusy) return;

            lock (_synlock)
            {
                if (!IsBusy) return;

                IsBusy = false;
                CurrentDispatcher.Invoke(() => Mouse.OverrideCursor = _previous);
                _previous = null;
            }
        }


        public bool IsBusy
        {
            get { return _busy; }
            set
            {
                if (SetPropertyAndNotify(ref _busy, value))
                {
                    OnPropertyChanged("IsIdle");
                }
            }
        }
        public bool IsIdle => !IsBusy;

        public void Dispose()
        {
            _countdown?.Dispose();
        }


    }

    public class DisabledKeyboardCommands : IKeyboardCommands
    {
        public ICommand CtlF5Command => new DisabledCommand();

        public ICommand F5Command => new DisabledCommand();
    }
}
