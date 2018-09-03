#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.Mvvm;
using static System.FormattableString;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{

    public sealed class QueueListViewModel : ViewModelBase, IStatusInfo
    {
        private List<QueueInfo> _originalQueueList;

        internal QueueListViewModel(QueueManagerViewModel parent)
        {
            Progress = new RangeProgress();
            BuildCommands();
            Queues = new SelectableItemCollection<QueueInfo>();
            BindingOperations.EnableCollectionSynchronization(Queues, new object());
            Parent = parent;
            StatusInfoViewModel = new QueueListStatusInfo(this);
        }

        public QueueManagerViewModel Parent { get; }

        public ShellService ShellService { get { return Parent.App.ShellService; } }

        public RangeProgress Progress { get; }

        private bool _initialized;
        public bool Initialized
        {
            get { return _initialized; }
            private set
            {
                SetPropertyAndNotify(ref _initialized, value);
            }
        }

        private IQueueManager Qm { get; set; }
        private IObjectProvider ObjectProvider { get; set; }
        internal Task InitializeAsync(IQueueManager qm, IObjectProvider objProvider)
        {
            if (qm == null) throw new ArgumentNullException(nameof(qm));
            if (objProvider == null) throw new ArgumentNullException(nameof(objProvider));


            Qm = qm;
            ObjectProvider = objProvider;

            return InitializeCoreAsync();
        }

        private async Task InitializeCoreAsync()
        {
            await Parent.ExecuteAsync(() =>
            {
                Queues.Clear();

                ((QueueListStatusInfo)StatusInfoViewModel).ConnectionInformation = Qm.ConnectionInfo;

                var qInfos = new List<QueueInfo>();

                Initialized = Parent.ExecuteGuarded(() =>
                {
                    foreach (var q in ObjectProvider.GetQueues())
                    {
                        qInfos.Add(new QueueInfo(q, Parent.App.UserSettings));
                    }
                });

                InitCollectionView(qInfos);

            });


        }

        private bool _hasSystemQueues;

        public bool HasSystemQueues
        {
            get { return _hasSystemQueues; }
            set { SetPropertyAndNotify(ref _hasSystemQueues, value); }
        }

        public ICommand ReloadCommand { get; private set; }
        public ICommand RefreshInfosCommand { get; private set; }
        public ICommand ApplyFilterCommand { get; private set; }
        public ICommand SetGetAllowCommand { get; private set; }
        public ICommand SetGetInhibitCommand { get; private set; }
        public ICommand SetPutAllowCommand { get; private set; }
        public ICommand SetPutInhibitCommand { get; private set; }
        public ICommand EmptyWithDeleteCommand { get; private set; }
        public ICommand EmptyWithTruncateCommand { get; private set; }
        public ICommand BrowseCommand { get; private set; }
        public ICommand SelectAllCommand { get; private set; }
        public ICommand SelectNoneCommand { get; private set; }
        public ICommand InvertSelectionCommand { get; private set; }

        private void BuildCommands()
        {
            ReloadCommand = CreateCommand(
                async () =>
                {
                    await InitializeCoreAsync();
                }, () => Initialized && !(ObjectProvider.Filter is StaticQueueList));

            RefreshInfosCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await RefreshQueuesInfoAsync(false);
                }, () => Initialized);

            ApplyFilterCommand = new AwaitableRelayCommand(
                 async () =>
                {
                     await UpdateFilterResultAsync();
                }, () => Initialized);

            SetGetAllowCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await SetGetStatusAsync(GetPutStatus.Allowed, Queues.SelectedItems());
                }, () => QueuesSupportGet(false, false, Queues.SelectedItems()));

            SetGetInhibitCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await SetGetStatusAsync(GetPutStatus.Inhibited, Queues.SelectedItems());
                }, () => QueuesSupportGet(false, false, Queues.SelectedItems()));

            SetPutAllowCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await SetPutStatusAsync(GetPutStatus.Allowed, Queues.SelectedItems());
                }, () => QueuesSupportPut(false, false, Queues.SelectedItems()));

            SetPutInhibitCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await SetPutStatusAsync(GetPutStatus.Inhibited, Queues.SelectedItems());
                }, () => QueuesSupportPut(false, false, Queues.SelectedItems()));

            EmptyWithDeleteCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await EmptyQueuesAsync(false, Queues.SelectedItems());
                }, () => QueuesSupportGet(false, true, Queues.SelectedItems()));

            EmptyWithTruncateCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await EmptyQueuesAsync(true, Queues.SelectedItems());
                }, () => QueuesSupportTruncate(false, Queues.SelectedItems()));

            BrowseCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await BrowseQueuesAsync(Queues.SelectedItems());
                }, () => QueuesSupportGet(true, false, Queues.SelectedItems()));

            SelectAllCommand = CreateCommand(
                () =>
                {
                    foreach (QueueInfo q in QueuesFilterView)
                    {
                        q.Selected = true;
                    }
                }, () => Initialized);

            SelectNoneCommand = CreateCommand(
                () =>
                {
                    foreach (QueueInfo q in QueuesFilterView)
                    {
                        q.Selected = false;
                    }
                }, () => Initialized);

            InvertSelectionCommand = CreateCommand(
                () =>
                {
                    foreach (QueueInfo q in QueuesFilterView)
                    {
                        q.Selected = !q.Selected;
                    }
                }, () => Initialized);

            BuildContextMenuCommands();
        }


        public ICommand SelectedBrowseCommand { get; private set; }
        public ICommand SelectedGetAllowCommand { get; private set; }
        public ICommand SelectedGetInhibitCommand { get; private set; }
        public ICommand SelectedPutAllowCommand { get; private set; }
        public ICommand SelectedPutInhibitCommand { get; private set; }
        public ICommand SelectedClearWithDeleteCommand { get; private set; }
        public ICommand SelectedClearWithTruncateCommand { get; private set; }
        public ICommand SelectedPutMessageCommand { get; private set; }

        private void BuildContextMenuCommands()
        {
            SelectedBrowseCommand = new AwaitableRelayCommand(
                async () => await BrowseQueuesAsync(GetHighlightedQueueAsList()),
                () => HighlightedQueue?.GetStatus != null
                );

            SelectedGetAllowCommand = new AwaitableRelayCommand(
                async () => await SetGetStatusAsync(GetPutStatus.Allowed, GetHighlightedQueueAsList()),
                () => HighlightedQueue?.GetStatus != null && HighlightedQueue.GetStatus == GetPutStatus.Inhibited
                );

            SelectedGetInhibitCommand = new AwaitableRelayCommand(
                async () => await SetGetStatusAsync(GetPutStatus.Inhibited, GetHighlightedQueueAsList()),
                () => HighlightedQueue?.GetStatus != null && HighlightedQueue.GetStatus == GetPutStatus.Allowed
                );

            SelectedPutAllowCommand = new AwaitableRelayCommand(
                async () => await SetPutStatusAsync(GetPutStatus.Allowed, GetHighlightedQueueAsList()),
                () => HighlightedQueue?.PutStatus != null && HighlightedQueue.PutStatus == GetPutStatus.Inhibited
                );

            SelectedPutInhibitCommand = new AwaitableRelayCommand(
                async () => await SetPutStatusAsync(GetPutStatus.Inhibited, GetHighlightedQueueAsList()),
                () => HighlightedQueue?.PutStatus != null && HighlightedQueue.PutStatus == GetPutStatus.Allowed
                );

            SelectedClearWithDeleteCommand = new AwaitableRelayCommand(
                async () => await EmptyQueuesAsync(false, GetHighlightedQueueAsList()),
                () => HighlightedQueue?.GetStatus != null
                );

            SelectedClearWithTruncateCommand = new AwaitableRelayCommand(
                async () => await EmptyQueuesAsync(true, GetHighlightedQueueAsList()),
                () => HighlightedQueue != null && HighlightedQueue.SupportTruncate && HighlightedQueue.GetStatus.HasValue
                );
            SelectedPutMessageCommand = CreateCommand(
                PutMessageInQueue,
                () => HighlightedQueue?.PutStatus != null
                );
        }

        private void PutMessageInQueue()
        {
            Parent.App.OpenPutMessageView(HighlightedQueue);
        }

        private IList<QueueInfo> GetHighlightedQueueAsList()
        {
            return new[] { HighlightedQueue };
        }

        private async Task BrowseQueuesAsync(IList<QueueInfo> list)
        {
            if (list != null && list.Count > 0)
            {
                await Parent.ExecuteAsync((ct) =>
                {
                    using (var ps = Progress.Start(list.Count, ct, LongRunningState.No))
                    {
                        var setActive = true;
                        foreach (var q in list)
                        {
                            if (ps.CancellationToken.IsCancellationRequested)
                                break;

                            if (q.GetStatus.HasValue)
                            {
                                Parent.App.OpenQueueView(q.QueueSource, setActive);
                                setActive = false;
                            }
                        }
                    }

                });
            }
        }

        private async Task SetPutStatusAsync(GetPutStatus status, IList<QueueInfo> queues)
        {

            await Parent.ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(queues.Count, ct, LongRunningState.Yes))
                {
                    ps.SetTitle("Changing 'put' status...");
                    foreach (var q in queues)
                    {
                        if (ps.CancellationToken.IsCancellationRequested)
                            break;
                        ps.ReportNext();
                        if (q.PutStatus.HasValue) q.SetPutStatus(status);
                    }
                }
            });
        }

        private async Task SetGetStatusAsync(GetPutStatus status, IList<QueueInfo> queues)
        {
            await Parent.ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(queues.Count, ct, LongRunningState.Yes))
                {
                    ps.SetTitle("Changing 'get' status...");
                    foreach (var q in queues)
                    {
                        if (ps.CancellationToken.IsCancellationRequested)
                            break;
                        ps.ReportNext();
                        if (q.GetStatus.HasValue) q.SetGetStatus(status);
                    }
                }
            });
        }

        private async Task EmptyQueuesAsync(bool truncate, IList<QueueInfo> queues)
        {

            var msg = "You are going to empty the following queue(s):" +
                string.Concat(queues.Select(q => Invariant($"\n\t- {q.UniqueId}")).ToArray()) +
                "\n\nAre you sure?";

            if (Parent.App.MessageService.ShowYesNoQuestion(Parent.App.ShellService.ShellView, msg))
            {

                await Parent.ExecuteAsync((ct) =>
                {
                    using (var ps = Progress.Start(queues.Count, ct, LongRunningState.Yes))
                    {
                        ps.SetTitle("Emptying queues...");
                        foreach (var q in queues)
                        {
                            if (ps.CancellationToken.IsCancellationRequested)
                                break;

                            if (q.GetStatus.HasValue && q.GetStatus == GetPutStatus.Allowed
                                && (!truncate || q.SupportTruncate))
                            {
                                q.EmptyQueue(truncate);
                            }
                            ps.ReportNext();
                        }
                    }
                });
            }
        }

        private void InitCollectionView(List<QueueInfo> queues)
        {
            _originalQueueList = queues;

            HasSystemQueues = queues.Any(q => q.QueueSource.IsSystemQueue);

            QueuesFilterView = (CollectionView)CollectionViewSource.GetDefaultView(queues);
            Queues.Clear();
            using (_filterProgressScope = Progress.Start(queues.Count, LongRunningState.No))
            {
                using (QueuesFilterView.DeferRefresh())
                {
                    QueuesFilterView.Filter = OnQueuesViewFilter;
                    QueuesFilterView.SortDescriptions.Add(
                        new SortDescription("Name", ListSortDirection.Ascending));
                }
            }

            StatusInfoViewModel.LastUpdateTimestamp = DateTime.Now;

        }

        private ProgressActiveScope _filterProgressScope;
        private bool OnQueuesViewFilter(object item)
        {
            _filterProgressScope?.ReportNext();

            var q = (QueueInfo)item;

            if (!FilterIncludeSystemQueues && q.QueueSource.IsSystemQueue)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(FilterByName))
            {
                if (q.Name.IndexOf(FilterByName, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    return false;
                }
            }

            Parent.ExecuteGuarded(() => q.RefreshInfo());

            if (FilterType.HasValue)
            {
                if (q.Type != FilterType.Value) { return false; }
            }


            if (FilterGetStatus.HasValue && q.GetStatus.HasValue)
            {
                if (q.GetStatus.Value != FilterGetStatus.Value) { return false; }
            }

            if (FilterPutStatus.HasValue && q.PutStatus.HasValue)
            {
                if (q.PutStatus.Value != FilterPutStatus.Value) { return false; }
            }

            if (FilterWithMessages)
            {
                if (!q.Depth.HasValue || q.Depth == 0)
                {
                    return false;
                }
            }

            Queues.Add(q);

            return true;
        }

        private async Task UpdateFilterResultAsync()
        {
            await Parent.ExecuteAsync(() =>
            {
                using (var ps = Progress.Start(_originalQueueList.Count, LongRunningState.No))
                {
                    _filterProgressScope = ps;

                    Queues.Clear();
                    QueuesFilterView.Refresh();
                    _filterProgressScope = null;
                }
            });
        }

        public SelectableItemCollection<QueueInfo> Queues { get; }

        private ICollectionView QueuesFilterView { get; set; }

        private async Task RefreshQueuesInfoAsync(bool allQueues)
        {
            if (!Initialized) return;

            var count = allQueues ? _originalQueueList.Count : Queues.TotalCount;

            await Parent.ExecuteAsync(() =>
            {
                using (var ps = Progress.Start(count))
                {
                    ps.SetTitle("Updating...");
                    var queuesToRefreshInfo = allQueues ? QueuesFilterView.SourceCollection : QueuesFilterView;

                    foreach (QueueInfo q in queuesToRefreshInfo)
                    {
                        Parent.ExecuteGuarded(() => q.RefreshInfo());
                        ps.ReportNext();
                    }
                }
                StatusInfoViewModel.LastUpdateTimestamp = DateTime.Now;
            });
        }

        private void StartUpdateFilterResult()
        {
            var _ = UpdateFilterResultAsync();
        }

        private bool _filterIncludeSystemQueues;
        public bool FilterIncludeSystemQueues
        {
            get { return _filterIncludeSystemQueues; }
            set
            {
                if (SetPropertyAndNotify(ref _filterIncludeSystemQueues, value))
                {
                    StartUpdateFilterResult();
                }
            }
        }

        private string _filterByName;
        public string FilterByName
        {
            get
            {
                return _filterByName;
            }
            set
            {
                if (SetPropertyAndNotify(ref _filterByName, value))
                {
                    StartUpdateFilterResult();
                }
            }
        }

        private bool _filterWithMessages;
        public bool FilterWithMessages
        {
            get
            {
                return _filterWithMessages;
            }
            set
            {
                if (SetPropertyAndNotify(ref _filterWithMessages, value))
                {
                    StartUpdateFilterResult();
                }
            }
        }


        private int? _filterType;
        public int? FilterType
        {
            get
            {
                return _filterType;
            }
            set
            {
                if (SetPropertyAndNotify(ref _filterType, value))
                {
                    StartUpdateFilterResult();
                }
            }
        }

        private GetPutStatus? _filterGetStatus;
        public GetPutStatus? FilterGetStatus
        {
            get
            {
                return _filterGetStatus;
            }
            set
            {
                if (SetPropertyAndNotify(ref _filterGetStatus, value))
                {
                    StartUpdateFilterResult();
                }
            }
        }

        private GetPutStatus? _filterPutStatus;
        public GetPutStatus? FilterPutStatus
        {
            get
            {
                return _filterPutStatus;
            }
            set
            {
                if (SetPropertyAndNotify(ref _filterPutStatus, value))
                {
                    StartUpdateFilterResult();
                }
            }
        }

        private QueueInfo _highlightedQueue;
        public QueueInfo HighlightedQueue
        {
            get { return _highlightedQueue; }
            set
            {
                SetPropertyAndNotify(ref _highlightedQueue, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool QueuesSupportTruncate(bool all, IList<QueueInfo> queues)
        {
            if (queues.Count == 0)
            {
                return false;
            }
            bool ok = false;
            foreach (var q in queues)
            {
                ok = (q.SupportTruncate && q.GetStatus.HasValue);
                if (ok && !all) return true;
                if (!ok && all) return false;
            }
            return ok;
        }
        private bool QueuesSupportGet(bool all, bool allowedOnly, IList<QueueInfo> queues)
        {
            if (queues.Count == 0)
            {
                return false;
            }
            bool ok = false;
            foreach (var q in queues)
            {
                ok = (q.GetStatus.HasValue && (!allowedOnly || q.GetStatus.Value != GetPutStatus.Inhibited));
                if (ok && !all) return true;
                if (!ok && all) return false;
            }
            return ok;

        }

        private bool QueuesSupportPut(bool all, bool allowedOnly, IList<QueueInfo> queues)
        {
            if (queues.Count == 0)
            {
                return false;
            }
            bool ok = false;
            foreach (var q in queues)
            {
                ok = (q.PutStatus.HasValue && (!allowedOnly || q.PutStatus.Value != GetPutStatus.Inhibited));
                if (ok && !all) return true;
                if (!ok && all) return false;
            }
            return ok;
        }


        public StatusInfoViewModel StatusInfoViewModel { get; private set; }
    }

}
