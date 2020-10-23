#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security;
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public sealed class OpenQueueManagerViewModel : ModalViewModel
    {
        private string _queueManagerName;
        private string _queueNamePrefixFilter;
        private bool _isRemote = true;
        private bool _showObjectFilter = true;

        public OpenQueueManagerViewModel(IOpenQueueManagerView view, IApplicationController appc) : base(view, appc)
        {
            Title = "Select Queue Manager...";
            FilterByPrefix = true;
            StaticQueueNames = new QueueNames();
        }

        public void Initialize(bool remote = false, bool showObjectFilter = true, RecentQueueManagerConnection rqmc = null)
        {
            IsRemote = remote;
            ShowObjectFilter = showObjectFilter;


            if (rqmc != null)
            {
                QueueManagerName = rqmc.QueueManagerName;
                QueueNamePrefixFilter = rqmc.ObjectNamePrefix;
                if (rqmc.QueueList != null && rqmc.QueueList.Count > 0)
                {
                    FilterByPrefix = false;
                    FilterByQueueNames = true;
                    foreach(var s in rqmc.QueueList)
                    {
                        StaticQueueNames.List.Add(s);
                    }
                }
                if (rqmc is RecentRemoteQueueManagerConnection && remote)
                {
                    var rrqmc = (RecentRemoteQueueManagerConnection)rqmc;
                    RemoteConfig = new RemoteConfiguration
                    {
                        Host = rrqmc.HostName,
                        Channel = rrqmc.Channel,
                        Port = rrqmc.Port
                    };
                    if (!string.IsNullOrEmpty((rrqmc.UserId)))
                    {
                        RemoteConfig.UserId = rrqmc.UserId;
                    }
                }
            }
            else
            {
                if (!remote)
                {
                    App.ShellService.WithGlobalBusy(() => QueueManagerName = App.MqController.TryGetDefaultQueueManagerName());
                }
                else
                {
                    RemoteConfig = new RemoteConfiguration
                    {
                        Channel = App.UserSettings.Channel,
                        Port = App.UserSettings.Port
                    };
                }
            }
        }

        public override string OkLabel => "Open";

        public override string CancelLabel => "Close";

        public bool IsRemote
        {
            get { return _isRemote; }
            set { SetPropertyAndNotify(ref _isRemote, value); }
        }

        public bool ShowObjectFilter
        {
            get { return _showObjectFilter; }
            set
            {
                SetPropertyAndNotify(ref _showObjectFilter, value);
                OnPropertyChanged(nameof(ShowObjectPrefixFilter));
            }
        }

        public bool ShowObjectPrefixFilter
        {
            get { return ShowObjectFilter && FilterByPrefix; }
        }

        public RemoteConfiguration RemoteConfig { get; private set; }


        [Required(ErrorMessage = "Queue manager name required")]
        public string QueueManagerName
        {
            get { return _queueManagerName; }
            set { SetPropertyAndNotify(ref _queueManagerName, value); }
        }

        public string QueueNamePrefixFilter
        {
            get { return _queueNamePrefixFilter; }
            set
            {
                SetPropertyAndNotify(ref _queueNamePrefixFilter, value);
            }
        }

        protected override bool OkAllowed()
        {
            return HasErrors == false && (RemoteConfig == null || RemoteConfig.HasErrors == false) &&
                (FilterByPrefix || (FilterByQueueNames && StaticQueueNames.List.Count > 0));
        }

        public IQueueManager QueueManager { get; private set; }

        private bool _filterByPrefix;
        public bool FilterByPrefix
        {
            get
            {
                return _filterByPrefix;
            }
            set
            {
                SetPropertyAndNotify(ref _filterByPrefix, value);
                OnPropertyChanged(nameof(ShowObjectPrefixFilter));
            }
        }

        private bool _filterByQueueNames;
        public bool FilterByQueueNames
        {
            get
            {
                return _filterByQueueNames;
            }
            set
            {
                SetPropertyAndNotify(ref _filterByQueueNames, value);

            }
        }

        public IObjectNameFilter GetObjectNameFilter()
        {

            if (FilterByPrefix)
            {
                return QueueManager?.NewObjectNameFilter(this.QueueNamePrefixFilter);
            }
            if (FilterByQueueNames)
            {
                return new StaticQueueList((IEnumerable<string>)StaticQueueNames.List);
            }
            return QueueManager?.NewObjectNameFilter(this.QueueNamePrefixFilter); ;
        }

        protected override void OnOk(CancelEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            try
            {
                App.ShellService.WithGlobalBusy(() =>
               {

                   var cp = IsRemote
                            ? App.MqController.CreateConnectionProperties(RemoteConfig.Host,
                                                        RemoteConfig.Port,
                                                        RemoteConfig.Channel,
                                                        RemoteConfig.UserId,
                                                        RemoteConfig.Password)
                            : null;
                   QueueManager = App.MqController.ConnectQueueManager(QueueManagerName, cp);

               });
            }
            catch (MqException ex)
            {
                ShowErrorMessage("Connection failed", ex);
                e.Cancel = true;
            }
        }

        public QueueNames StaticQueueNames { get; private set; }
    }

    public sealed class QueueNames : BindableBase
    {
        public QueueNames()
        {
            List = new ObservableCollection<string>();
            ListView = CollectionViewSource.GetDefaultView(List);
            BuildCommands();
        }

        private void BuildCommands()
        {
            Add = new RelayCommand(
                () => List.Add(NewEntry),
                () => !string.IsNullOrEmpty(NewEntry) && !ListContains(NewEntry)
                );

            Delete = new RelayCommand(
                () => List.Remove(SelectedEntry),
                () => !string.IsNullOrEmpty(SelectedEntry)
                );
        }

        private bool ListContains(string item)
        {
            return List.Any(s => s.Equals(item, StringComparison.OrdinalIgnoreCase));
        }

        public ObservableCollection<string> List { get; private set; }

        public ICollectionView ListView { get; private set; }

        public string NewEntry { get; set; }

        public string SelectedEntry { get; set; }
        public ICommand Add { get; private set; }

        public ICommand Delete { get; private set; }
    }

    public sealed class RemoteConfiguration : ValidatableBindableBase
    {
        private string _host;
        private int _port;
        private string _channel;
        private string _userId;
        private SecureString _password;

        [Required]
        public string Host
        {
            get { return _host; }
            set { SetPropertyAndNotify(ref _host, value); }
        }

        [Required]
        public int Port
        {
            get { return _port; }
            set { SetPropertyAndNotify(ref _port, value); }
        }

        [Required]
        public string Channel
        {
            get { return _channel; }
            set { SetPropertyAndNotify(ref _channel, value); }
        }

        [RequiredIfNotEmpty("Password")]
        public string UserId
        {
            get { return _userId; }
            set { SetPropertyAndNotify(ref _userId, value); }
        }

        public SecureString Password
        {
            get { return _password; }
            set
            {
                if (SetPropertyAndNotify(ref _password, value))
                {
                    ValidateProperty("UserId");
                }
            }
        }
    }
}
