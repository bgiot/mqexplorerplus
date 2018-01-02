#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Mvvm;
using Dotc.MQExplorerPlus.Core.Models;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{

    public enum QueueManagerContentType
    {
        Unknown,
        Queues,
        Channels,
        Listeners,
    }

    [Export(typeof(QueueManagerViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class QueueManagerViewModel : DocumentViewModel, IKeyboardCommands, IStatusInfo, ISupportAutomaticRefresh
    {

        [ImportingConstructor]
        public QueueManagerViewModel(IQueueManagerView view, IApplicationController ac)
            : base(view, ac)
        {
            Queues = new QueueListViewModel(this);
            Channels = new ChannelListViewModel(this);
            Listeners = new ListenerListViewModel(this);
            BuildCommands();
        }

        private void BuildCommands()
        {
            RefreshCommand = CreateCommand(
                () => CallRefresh(),
                () => CanCallRefresh());
            ApplyFilterCommand = CreateCommand(
                () => CallApplyFilter(),
                () => CanCallApplyFilter());
        }

        private ICommand RefreshCommand;
        private ICommand ApplyFilterCommand;

        private void CallRefresh()
        {
            if (ActiveContent == QueueManagerContentType.Queues)
                RelayCommand.Execute(Queues.RefreshInfosCommand);

            if (ActiveContent == QueueManagerContentType.Channels)
                RelayCommand.Execute(Channels.RefreshInfosCommand);

            if (ActiveContent == QueueManagerContentType.Listeners)
                RelayCommand.Execute(Listeners.RefreshInfosCommand);

            App.ShellService.Countdown.ResetCountdown();

        }
        private bool CanCallRefresh()
        {

            if (ActiveContent == QueueManagerContentType.Queues)
                return Queues.Initialized && !LocalBusy;

            if (ActiveContent == QueueManagerContentType.Channels)
                return Channels.Initialized && !LocalBusy;

            if (ActiveContent == QueueManagerContentType.Listeners)
                return Listeners.Initialized && !LocalBusy;

            return false;
        }

        private void CallApplyFilter()
        {
            if (ActiveContent == QueueManagerContentType.Queues)
                RelayCommand.Execute(Queues.ApplyFilterCommand);

            if (ActiveContent == QueueManagerContentType.Channels)
                RelayCommand.Execute(Channels.ApplyFilterCommand);

            if (ActiveContent == QueueManagerContentType.Listeners)
                RelayCommand.Execute(Listeners.ApplyFilterCommand);
        }

        private bool CanCallApplyFilter()
        {
            if (ActiveContent == QueueManagerContentType.Queues)
                return Queues.Initialized && !LocalBusy;

            if (ActiveContent == QueueManagerContentType.Channels)
                return Channels.Initialized && !LocalBusy;

            if (ActiveContent == QueueManagerContentType.Listeners)
                return Listeners.Initialized && !LocalBusy;
            return false;
        }


        public QueueListViewModel Queues
        {
            get;
        }

        public ChannelListViewModel Channels
        {
            get;
        }

        public ListenerListViewModel Listeners
        {
            get;
        }

        public ICommand CtlF5Command
        {
            get { return ApplyFilterCommand; }
        }

        public ICommand F5Command
        {
            get
            {
                return RefreshCommand;
            }
        }

        public StatusInfoViewModel StatusInfoViewModel
        {
            get
            {
                switch (ActiveContent)
                {
                    case QueueManagerContentType.Queues:
                        return Queues.StatusInfoViewModel;
                    case QueueManagerContentType.Channels:
                        return Channels.StatusInfoViewModel;
                    case QueueManagerContentType.Listeners:
                        return Listeners.StatusInfoViewModel;
                    default:
                        return null;
                }
            }
        }

        public RangeProgress Progress
        {
            get
            {
                switch (ActiveContent)
                {
                    case QueueManagerContentType.Queues:
                        return Queues.Progress;
                    case QueueManagerContentType.Channels:
                        return Channels.Progress;
                    case QueueManagerContentType.Listeners:
                        return Listeners.Progress;
                    default:
                        return null;
                }
            }
        }

        public void StartInitialize(IQueueManager qm, IObjectProvider objProvider)
        {
            var _ = InitializeAsync( qm,  objProvider);
        }
        private async Task InitializeAsync(IQueueManager qm, IObjectProvider objProvider)
        {
            await Queues.InitializeAsync(qm, objProvider);
            await Channels.InitializeAsync(qm, objProvider);
            await Listeners.InitializeAsync(qm, objProvider);
        }

        private QueueManagerContentType _activeContent;
        public QueueManagerContentType ActiveContent
        {
            get { return _activeContent; }
            set
            {
                _activeContent = value;
                ActiveContentChanged();
            }
        }

        private void ActiveContentChanged()
        {
            OnPropertyChanged(nameof(StatusInfoViewModel));
            OnPropertyChanged(nameof(Progress));
            CommandManager.InvalidateRequerySuggested();
            App.ShellService.InvokeAutomaticRefresh();
        }

        public void OnAutomaticRefreshTriggered()
        {
            if (!LocalBusy) CallRefresh();
        }
    }

}
