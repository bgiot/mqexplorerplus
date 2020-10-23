#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel;
using System.Windows;
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.ViewModels;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Mvvm;
using static System.FormattableString;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Dotc.Wpf;

namespace Dotc.MQExplorerPlus.Core.Controllers
{

    public interface IApplicationController
    {
        AppSettings AppSettings { get; }
        UserSettings UserSettings { get; }
        IMessageService MessageService { get; }
        ShellService ShellService { get; }
        IViewService ViewService { get; }
        IFileDialogService FileDialogService { get; }
        MqController MqController { get; }
        void OpenPutMessageView(QueueInfo queue);
        void SelectQueue(IQueueManager qm, Action<IQueue> onOk);
        void OpenQueueView(IQueue queue, bool setActiveView = true);
        IShellView Run();
        void Shutdown();
        void OpenChannelStopParametersView(Action<ChannelStopParameters> callback);
        void OpenChannelResetParametersView(Action<ChannelResetParameters> callback);
        void OpenChannelResolveParametersView(Action<ChannelResolveParameters> callback);

        void OpenDumpCreationSettingsView(QueueInfo queue, Action<string, DumpCreationSettings> callback);
        void OpenDumpLoadSettingsView(QueueInfo queue, Action<string, DumpLoadSettings> callback);
        void OpenExportMessagesSettingsView(QueueInfo queue, int messagesCount, Action<string, CsvExportSettings> callback);

        T GetViewModel<T>();


    }


    public sealed class ApplicationController : IApplicationController
    {


        public ApplicationController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IServiceProvider ServiceProvider { get; }
        public AppSettings AppSettings { get { return ServiceProvider.GetService<AppSettings>();} }
        public UserSettings UserSettings { get { return ServiceProvider.GetService<UserSettings>(); } }
        public MqController MqController { get { return ServiceProvider.GetService<MqController>(); } }
        public IMessageService MessageService { get { return ServiceProvider.GetService<IMessageService>(); } }
        public ShellService ShellService { get { return ServiceProvider.GetService<ShellService>(); } }
        public IViewService ViewService { get { return ServiceProvider.GetService<IViewService>(); } }
        public IFileDialogService FileDialogService { get { return ServiceProvider.GetService<IFileDialogService>(); } }
        public ShellViewModel Shell { get { return ServiceProvider.GetService<ShellViewModel>(); } }

        public T GetViewModel<T>()
        {
            return UIDispatcher.Execute<T>(() => ServiceProvider.GetService<T>());
        }

        private void Initialize()
        {

        }


        private void ShowAbout()
        {
            var svm = GetViewModel<AboutViewModel>();
            ViewService.ShowModalView(svm);
        }


        private bool IsLocalQmInstalled()
        {
            return MqController.LocalMqInstalled;
        }

        private void ShowSettings()
        {
            var svm = GetViewModel<SettingsViewModel>(); 
            svm.Initialize(UserSettings);

            ViewService.ShowModalView(svm);
        }

        private void ShowParsingEditor()
        {

            if (!ViewService.DocumentViewExists(ParsingEditorViewModel.ID))
            {
                var vm = GetViewModel<ParsingEditorViewModel>(); 
                ViewService.AddDocumentView(vm, true);
            }
            else
            {
                ViewService.SetActiveDocumentView(ParsingEditorViewModel.ID);
            }
        }

        private void ShellView_Closing(object sender, CancelEventArgs e)
        {
            Window w = (Window)sender;
            if (!w.IsVisible)
            {
                e.Cancel = true;
                return;
            }

            if (!MessageService.ShowYesNoQuestion("You are exiting the application.\nAre you sure?"))
            {
                e.Cancel = true;
            }
        }

        private void OpenLocalQueueManager(RecentQueueManagerConnection recentConn = null)
        {
            if (recentConn != null)
            {
                bool automaticOpenOk = false;
                ShellService.WithGlobalBusy(() =>
                {
                    var qm = MqController.TryOpenQueueManager(recentConn);
                    if (qm != null)
                    {
                        IObjectNameFilter filter = null;
                        if (recentConn.QueueList != null && recentConn.QueueList.Count > 0)
                        {
                            filter = new StaticQueueList(recentConn.QueueList.ToArray());
                        }
                        else
                        {
                            filter = qm.NewObjectNameFilter(recentConn.ObjectNamePrefix);
                        }
                        var provider = qm.NewObjectProvider(filter);
                        OpenQueueManagerView(qm, provider);
                        UserSettings.AddRecentConnection(recentConn);
                        automaticOpenOk = true;
                    }
                });
                if (automaticOpenOk) return;
            }

            SelectQueueManagerInternal(false, true, recentConn, (qm, qpfx) =>
                {
                    if (qm != null)
                    {
                        AddRecentConnection(qm, qpfx, recentConn);
                        var provider = qm.NewObjectProvider(qpfx);
                        OpenQueueManagerView(qm, provider);
                    }
                });

        }

        private void OpenLocalQueue(RecentQueueConnection rqc = null)
        {
            if (rqc != null)
            {
                bool automaticOpenOk = false;
                ShellService.WithGlobalBusy(() =>
                {
                    var q = MqController.TryOpenQueue(rqc);
                    if (q != null)
                    {
                        OpenQueueView(q);
                        UserSettings.AddRecentConnection(rqc);
                        automaticOpenOk = true;
                    }
                });
                if (automaticOpenOk) return;
            }

            SelectQueueInternal(null, rqc, q =>
                {
                    if (q != null)
                    {
                        AddRecentConnection(q, rqc);
                        OpenQueueView(q);
                    }
                });
        }

        private void OpenRemoteQueueManager(RecentRemoteQueueManagerConnection rrqmc = null)
        {
            if (rrqmc != null)
            {
                bool automaticOpenOk = false;
                ShellService.WithGlobalBusy(() =>
                {
                    var qm = MqController.TryOpenRemoteQueueManager(rrqmc);
                    if (qm != null)
                    {
                        IObjectNameFilter filter = null;
                        if (rrqmc.QueueList != null && rrqmc.QueueList.Count > 0)
                        {
                            filter = new StaticQueueList(rrqmc.QueueList.ToArray());
                        }
                        else
                        {
                            filter = qm.NewObjectNameFilter(rrqmc.ObjectNamePrefix);
                        }
                        var provider = qm.NewObjectProvider(filter);
                        OpenQueueManagerView(qm, provider);
                        UserSettings.AddRecentConnection(rrqmc);
                        automaticOpenOk = true;
                    }
                });
                if (automaticOpenOk) return;
            }

            SelectQueueManagerInternal(true, true, rrqmc, (qm, qpfx) =>
             {
                 if (qm != null)
                 {
                     AddRecentConnection(qm, qpfx, rrqmc);
                     var provider = qm.NewObjectProvider(qpfx);
                     OpenQueueManagerView(qm, provider);
                 }
             });
        }

        private void OpenRemoteQueue(RecentRemoteQueueConnection rrqc = null)
        {
            if (rrqc != null)
            {
                bool automaticOpenOk = false;
                ShellService.WithGlobalBusy(() =>
                {
                    var q = MqController.TryOpenRemoteQueue(rrqc);
                    if (q != null)
                    {
                        OpenQueueView(q);
                        UserSettings.AddRecentConnection(rrqc);
                        automaticOpenOk = true;
                    }
                });
                if (automaticOpenOk) return;
            }

            SelectQueueManagerInternal(true, false, rrqc, (qm, qpfx) =>
            {
                if (qm != null)
                {
                    SelectQueueInternal(qm, rrqc, q =>
                        {
                            if (q != null)
                            {
                                AddRecentConnection(q, rrqc);
                                OpenQueueView(q);
                            }
                        });
                }
            });
        }

        private bool QueueListEquals(List<string> l1, string[] l2)
        {
            if (l1 == null && l2 == null) return true;

            if (l1 == null || l2 == null || l1.Count != l2.Length)
                return false;

            if (l1.Except(l2, StringComparer.OrdinalIgnoreCase).FirstOrDefault() != null)
                return false;

            return true;

        }

        private bool FilterEquals(RecentQueueManagerConnection conn, IObjectNameFilter filter)
        {
            if (filter is StaticQueueList)
            {
                return QueueListEquals(conn.QueueList, ((StaticQueueList)filter).Names);
            }
            return (conn.ObjectNamePrefix == filter?.NamePrefix[0]);
        }

        private void AddRecentConnection(IQueueManager qm, IObjectNameFilter filter, IRecentConnection previous)
        {
            RecentConnection data = null;
            if (previous != null)
            {
                if (qm.ConnectionProperties.IsLocal)
                {
                    var temp = previous as RecentQueueManagerConnection;
                    if (temp != null)
                    {
                        if (temp.QueueManagerName == qm.Name &&
                            FilterEquals(temp, filter))
                        {
                            data = temp;
                        }
                    }
                }
                else
                {
                    var temp = previous as RecentRemoteQueueManagerConnection;
                    if (temp != null)
                    {
                        if (temp.QueueManagerName == qm.Name &&
                            FilterEquals(temp, filter) &&
                            temp.HostName == qm.ConnectionProperties.HostName &&
                            temp.Port == qm.ConnectionProperties.Port &&
                            temp.Channel == qm.ConnectionProperties.Channel &&
                            (temp.UserId ?? "") == (qm.ConnectionProperties.UserId ?? ""))
                        {
                            data = temp;
                        }
                    }
                }
            }
            if (data == null)
            {
                if (qm.ConnectionProperties.IsLocal)
                {
                    data = new RecentQueueManagerConnection
                    {
                        QueueManagerName = qm.Name
                    };
                    if (filter is StaticQueueList)
                    {
                        ((RecentQueueManagerConnection)data).QueueList = new List<string>(((StaticQueueList)filter).Names);
                    }
                    else
                    {
                        ((RecentQueueManagerConnection)data).ObjectNamePrefix = filter?.NamePrefix?[0];
                    }
                }
                else
                {
                    data = new RecentRemoteQueueManagerConnection
                    {
                        QueueManagerName = qm.Name,
                        HostName = qm.ConnectionProperties.HostName,
                        Port = qm.ConnectionProperties.Port,
                        Channel = qm.ConnectionProperties.Channel,
                        UserId = qm.ConnectionProperties.UserId
                    };
                    if (filter is StaticQueueList)
                    {
                        ((RecentRemoteQueueManagerConnection)data).QueueList = new List<string>(((StaticQueueList)filter).Names);
                    }
                    else
                    {
                        ((RecentRemoteQueueManagerConnection)data).ObjectNamePrefix = filter?.NamePrefix?[0];
                    }
                    if (!string.IsNullOrWhiteSpace(qm.ConnectionProperties.UserId))
                    {
                        ((RecentRemoteQueueManagerConnection)data).UserId = qm.ConnectionProperties.UserId;
                    }
                }
            }
            UserSettings.AddRecentConnection(data);
        }

        private void AddRecentConnection(IQueue q, IRecentQueueConnection previous)
        {
            RecentConnection data = null;
            if (previous != null)
            {
                if (q.QueueManager.ConnectionProperties.IsLocal)
                {
                    var temp = previous as RecentQueueConnection;
                    if (temp != null)
                    {
                        if (temp.QueueManagerName == q.QueueManager.Name &&
                            temp.QueueName == q.Name)
                        {
                            data = temp;
                        }
                    }
                }
                else
                {
                    var temp = previous as RecentRemoteQueueConnection;
                    if (temp != null)
                    {
                        if (temp.QueueManagerName == q.QueueManager.Name &&
                            temp.QueueName == q.Name &&
                            temp.HostName == q.QueueManager.ConnectionProperties.HostName &&
                            temp.Port == q.QueueManager.ConnectionProperties.Port &&
                            temp.Channel == q.QueueManager.ConnectionProperties.Channel &&
                            (temp.UserId ?? "") == (q.QueueManager.ConnectionProperties.UserId ?? ""))
                        {
                            data = temp;
                        }
                    }
                }
            }
            if (data == null)
            {
                if (q.QueueManager.ConnectionProperties.IsLocal)
                {
                    data = new RecentQueueConnection
                    {
                        QueueManagerName = q.QueueManager.Name,
                        QueueName = q.Name
                    };
                }
                else
                {
                    data = new RecentRemoteQueueConnection
                    {
                        QueueManagerName = q.QueueManager.Name,
                        QueueName = q.Name,
                        HostName = q.QueueManager.ConnectionProperties.HostName,
                        Port = q.QueueManager.ConnectionProperties.Port,
                        Channel = q.QueueManager.ConnectionProperties.Channel
                    };
                    if (!string.IsNullOrEmpty(q.QueueManager.ConnectionProperties.UserId))
                    {
                        ((RecentRemoteQueueConnection)data).UserId = q.QueueManager.ConnectionProperties.UserId;
                    }
                }
            }
            UserSettings.AddRecentConnection(data);
        }
        private void SelectQueueManagerInternal(bool remote, bool showObjectFilter, RecentQueueManagerConnection rqmc, Action<IQueueManager, IObjectNameFilter> onOk)
        {
            var oqmvm = GetViewModel<OpenQueueManagerViewModel>(); 
            oqmvm.Initialize(remote, showObjectFilter, rqmc);


            ViewService.ShowModalView(oqmvm, () =>
            {
                if (oqmvm.Result == MessageBoxResult.OK)
                {
                    onOk?.Invoke(oqmvm.QueueManager, oqmvm.GetObjectNameFilter());
                }
            });
        }

        public void SelectQueue(IQueueManager qm, Action<IQueue> onOk)
        {
            SelectQueueInternal(qm, null, onOk);
        }

        private void SelectQueueInternal(IQueueManager qm, IRecentQueueConnection rqc, Action<IQueue> onOk)
        {
            var oqvm = GetViewModel<OpenQueueViewModel>(); 

            oqvm.Initialize(qm);
            if (rqc != null)
            {
                oqvm.QueueName = rqc.QueueName;
            }

            ViewService.ShowModalView(oqvm, () =>
            {
                if (oqvm.Result == MessageBoxResult.OK)
                {
                    onOk?.Invoke(oqvm.SelectedQueue);
                }
            });
        }

        public void OpenQueueManagerView(IQueueManager qm, IObjectProvider objProvider)
        {
            if (qm == null) throw new ArgumentNullException(nameof(qm));
            if (objProvider == null) throw new ArgumentNullException(nameof(objProvider));


            ShellService.WithGlobalBusy(() =>
            {
                var filter = objProvider.Filter;
                var key = string.Concat(qm.UniqueId, "!", filter.UniqueId);
                if (!ViewService.DocumentViewExists(key))
                {
                    var title = filter.NamePrefix == null || filter is StaticQueueList
                        ? qm.Name
                        : Invariant($"{qm.Name} ({filter.NamePrefix[0]}*)");

                    if (filter is StaticQueueList)
                    {
                        title = title + "(#)";
                    }

                    var vm = GetViewModel<QueueManagerViewModel>(); 
                    vm.Title = title;
                    vm.UniqueId = key;
                    ViewService.AddDocumentView(vm, true);
                    vm.StartInitialize(qm, objProvider);
                }
                else
                {
                    ViewService.SetActiveDocumentView(key);
                }
            });

        }

        public void OpenQueueView(IQueue queue, bool setActiveView = true)
        {

            if (queue == null) throw new ArgumentNullException(nameof(queue));

            ShellService.WithGlobalBusy(() =>
            {
                if (!ViewService.DocumentViewExists(queue.UniqueId))
                {
                    var vm = GetViewModel<MessageListViewModel>(); 

                    vm.Title = Invariant($"{queue.Name}@{queue.QueueManager.Name}");
                    vm.UniqueId = queue.UniqueId;
                    ViewService.AddDocumentView(vm, setActiveView);
                    vm.StartInitialize(queue);

                }
                else
                {
                    if (setActiveView) ViewService.SetActiveDocumentView(queue.UniqueId);
                }
            });

        }

        public void OpenPutMessageView(QueueInfo queue)
        {
            var pmvm = GetViewModel<PutMessageViewModel>(); 
            pmvm.Initialize(queue);

            ViewService.ShowModalView(pmvm);
        }

        private void Exit()
        {
            ShellService.ShellView.Close();
        }

        public IShellView Run()
        {
            Initialize();

            ShellService.ShellView = Shell.View as IShellView;

            ShellService.ExitCommand = new RelayCommand(Exit);
            ShellService.OpenQueueManagerCommand = new RelayCommand(() => OpenLocalQueueManager(), IsLocalQmInstalled);
            ShellService.OpenQueueCommand = new RelayCommand(() => OpenLocalQueue(), IsLocalQmInstalled);
            ShellService.OpenRemoteQueueManagerCommand = new RelayCommand(() => OpenRemoteQueueManager());
            ShellService.OpenRemoteQueueCommand = new RelayCommand(() => OpenRemoteQueue());
            ShellService.ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShellService.ShowAboutCommand = new RelayCommand(ShowAbout);
            ShellService.UseRecentConnectionCommand = new RelayCommand<RecentConnection>(UseRecentConnection);
            ShellService.ClearRecentConnectionsCommand = new RelayCommand(ClearRecentConnections,
                () => UserSettings.RecentConnections.Count > 0);
            ShellService.ShowParserEditorCommand = new RelayCommand(ShowParsingEditor);

            MessageService.Before += (s, e) =>
            {
                ShellService.DisableAutomaticRefresh();
            };
            MessageService.After += (s, e) =>
            {
                ShellService.EnableAutomaticRefresh();
            };

            if (ShellService.ShellView != null)
            {

                WeakEventManager<IShellView, CancelEventArgs>
                        .AddHandler(ShellService.ShellView, "Closing", ShellView_Closing);

            }

            return ShellService.ShellView;
        }

        private void ClearRecentConnections()
        {
            if (MessageService.ShowYesNoQuestion("Clearing the recent connections history.\nAre you sure?"))
                UserSettings.ClearRecentConnections();
        }

        private void UseRecentConnection(RecentConnection obj)
        {
            if (obj is RecentRemoteQueueConnection)
            {
                var rc = (RecentRemoteQueueConnection)obj;
                OpenRemoteQueue(rc);
                return;
            }

            if (obj is RecentRemoteQueueManagerConnection)
            {
                var rc = (RecentRemoteQueueManagerConnection)obj;
                OpenRemoteQueueManager(rc);
                return;
            }

            if (obj is RecentQueueConnection)
            {
                var rc = (RecentQueueConnection)obj;
                OpenLocalQueue(rc);
                return;
            }

            if (obj is RecentQueueManagerConnection)
            {
                var rc = (RecentQueueManagerConnection)obj;
                OpenLocalQueueManager(rc);
                return;
            }
        }


        public void Shutdown()
        {
        }

        public void OpenChannelStopParametersView(Action<ChannelStopParameters> callback)
        {
            var vm = GetViewModel<ChannelStopParametersViewModel>();

            ViewService.ShowModalView(vm, () =>
            {
                if (vm.Result == MessageBoxResult.OK)
                {
                    callback?.Invoke(vm.Parameters);
                }
            });
        }

        public void OpenChannelResetParametersView(Action<ChannelResetParameters> callback)
        {
            var vm = GetViewModel<ChannelResetParametersViewModel>();

            ViewService.ShowModalView(vm, () =>
            {
                if (vm.Result == MessageBoxResult.OK)
                {
                    callback?.Invoke(vm.Parameters);
                }
            });
        }

        public void OpenChannelResolveParametersView(Action<ChannelResolveParameters> callback)
        {
            var vm = GetViewModel<ChannelResolveParametersViewModel>();

            ViewService.ShowModalView(vm, () =>
            {
                if (vm.Result == MessageBoxResult.OK)
                {
                    callback?.Invoke(vm.Parameters);
                }
            });
        }

        public void OpenDumpCreationSettingsView(QueueInfo queue, Action<string, DumpCreationSettings> callback)
        {
            var vm = GetViewModel<DumpCreationSettingsViewModel>();
            vm.Initialize(queue);

            ViewService.ShowModalView(vm, () =>
            {
                if (vm.Result == MessageBoxResult.OK)
                {
                    callback?.Invoke(vm.Filename, vm.Settings);
                }
            });
        }

        public void OpenDumpLoadSettingsView(QueueInfo queue, Action<string, DumpLoadSettings> callback)
        {
            var vm = GetViewModel<DumpLoadSettingsViewModel>();
            vm.Initialize(queue);

            ViewService.ShowModalView(vm, () =>
            {
                if (vm.Result == MessageBoxResult.OK)
                {
                    callback?.Invoke(vm.Filename, vm.Settings);
                }
            });
        }

        public void OpenExportMessagesSettingsView(QueueInfo queue, int messagesCount, Action<string, CsvExportSettings> callback)
        {
            var vm = GetViewModel<ExportMessagesSettingsViewModel>();
            vm.Initialize(queue, messagesCount);

            ViewService.ShowModalView(vm, () =>
            {
                if (vm.Result == MessageBoxResult.OK)
                {
                    callback?.Invoke(vm.Filename, vm.Settings);
                }
            });
        }
    }
}
