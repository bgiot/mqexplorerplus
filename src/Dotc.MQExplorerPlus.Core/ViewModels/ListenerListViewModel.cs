#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public class ListenerListViewModel : ViewModelBase, IStatusInfo
    {
        private List<ListenerInfo> _originalListenerList;

        internal ListenerListViewModel(QueueManagerViewModel parent)
        {
            Progress = new RangeProgress();
            BuildCommands();
            Listeners = new SelectableItemCollection<ListenerInfo>();
            BindingOperations.EnableCollectionSynchronization(Listeners, new object());
            Parent = parent;
            StatusInfoViewModel = new ListenerListStatusInfo(this);
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

        private IQueueManager QM { get; set; }
        private IObjectProvider ObjectProvider { get; set; }
        internal Task InitializeAsync(IQueueManager qm, IObjectProvider objProvider)
        {
            if (qm == null) throw new ArgumentNullException(nameof(qm));
            if (objProvider == null) throw new ArgumentNullException(nameof(objProvider));

            QM = qm;
            ObjectProvider = objProvider;

            return InitializeCoreAsync();

        }

        private async Task InitializeCoreAsync()
        {
            if (!ObjectProvider.SupportListeners)
            {
                Initialized = false;
                return;
            }

            await Parent.ExecuteAsync(() =>
            {
                Listeners.Clear();

                ((ListenerListStatusInfo)StatusInfoViewModel).ConnectionInformation = QM.ConnectionInfo;

                var infos = new List<ListenerInfo>();

                Initialized = Parent.ExecuteGuarded(() =>
                {
                    foreach (var c in ObjectProvider.GetListeners())
                    {
                        infos.Add(new ListenerInfo(c, Parent.App.UserSettings));
                    }
                });

                InitCollectionView(infos);

            });
        }

        private bool _hasSystemListeners;

        public bool HasSystemListeners
        {
            get { return _hasSystemListeners; }
            set { SetPropertyAndNotify(ref _hasSystemListeners, value); }
        }

        public ICommand ReloadCommand { get; private set; }
        public ICommand RefreshInfosCommand { get; private set; }
        public ICommand ApplyFilterCommand { get; private set; }
        public ICommand SelectAllCommand { get; private set; }
        public ICommand SelectNoneCommand { get; private set; }
        public ICommand InvertSelectionCommand { get; private set; }

        private void BuildCommands()
        {
            ReloadCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await InitializeCoreAsync();
                }, () => Initialized);

            RefreshInfosCommand = new AwaitableRelayCommand(
                async () =>
                {
                    await RefreshListenersInfoAsync(false);
                }, () => Initialized);

            ApplyFilterCommand = new AwaitableRelayCommand(
                 async () =>
                 {
                     await UpdateFilterResultAsync();
                 }, () => Initialized);

            SelectAllCommand = CreateCommand(
                () =>
                {
                    foreach (ListenerInfo o in ListenersFilterView)
                    {
                        o.Selected = true;
                    }
                }, () => Initialized);

            SelectNoneCommand = CreateCommand(
                () =>
                {
                    foreach (ListenerInfo o in ListenersFilterView)
                    {
                        o.Selected = false;
                    }
                }, () => Initialized);

            InvertSelectionCommand = CreateCommand(
                () =>
                {
                    foreach (ListenerInfo o in ListenersFilterView)
                    {
                        o.Selected = !o.Selected;
                    }
                }, () => Initialized);

            BuildContextMenuCommands();
        }


        public ICommand SelectedStartCommand { get; private set; }
        public ICommand SelectedStopCommand { get; private set; }

        private void BuildContextMenuCommands()
        {
            SelectedStartCommand = new AwaitableRelayCommand(
                async () => await StartListenersAsync(GetHighlightedListenerAsList()),
                () => HighlightedListener != null
                );
            SelectedStopCommand = new AwaitableRelayCommand(
                async () => await AskStopListenersAsync(GetHighlightedListenerAsList()),
                () => HighlightedListener != null
                );
        }

        private async Task StartListenersAsync(IList<ListenerInfo> list)
        {

            await Parent.ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(list.Count, ct))
                {
                    ps.SetTitle("Starting listeners...");
                    foreach (var o in list)
                    {
                        if (ps.CancellationToken.IsCancellationRequested)
                            break;
                        o.Start();
                        ps.ReportNext();
                    }
                }
            });
        }

        private async Task AskStopListenersAsync(IList<ListenerInfo> list)
        {
            string msg = "Are you sure you want to stop listener ?";
            var result =Parent.App.MessageService.ShowYesNoQuestion(msg);
            if (result)
            {
                await StopListenersAsync(list);
            }

        }

        private async Task StopListenersAsync(IList<ListenerInfo> list)
        {

            await Parent.ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(list.Count, ct))
                {
                    ps.SetTitle("Stopping listeners...");
                    foreach (var o in list)
                    {
                        if (ps.CancellationToken.IsCancellationRequested)
                            break;
                        o.Stop();
                        ps.ReportNext();
                    }
                }
            });
        }


        private IList<ListenerInfo> GetHighlightedListenerAsList()
        {
            return new[] { HighlightedListener };
        }


        private void InitCollectionView(List<ListenerInfo> listeners)
        {
            _originalListenerList = listeners;

            HasSystemListeners = listeners.Any(q => q.ListenerSource.IsSystemListener);

            ListenersFilterView = (CollectionView)CollectionViewSource.GetDefaultView(listeners);
            Listeners.Clear();
            using (_filterProgressScope = Progress.Start(listeners.Count, LongRunningState.No))
            {
                using (ListenersFilterView.DeferRefresh())
                {
                    ListenersFilterView.Filter = OnListenersViewFilter;
                    ListenersFilterView.SortDescriptions.Add(
                        new SortDescription("Name", ListSortDirection.Ascending));
                }
            }

            StatusInfoViewModel.LastUpdateTimestamp = DateTime.Now;

        }

        private ProgressActiveScope _filterProgressScope;
        private bool OnListenersViewFilter(object item)
        {
            _filterProgressScope?.ReportNext();

            var l = (ListenerInfo)item;

            if (!FilterIncludeSystemListeners && l.ListenerSource.IsSystemListener)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(FilterByName))
            {
                if (l.Name.IndexOf(FilterByName, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    return false;
                }
            }

            Parent.ExecuteGuarded(() => l.RefreshInfo());

            if (FilterStatus.HasValue)
            {
                if (l.Status != FilterStatus.Value) { return false; }
            }

            Listeners.Add(l);

            return true;
        }

        private async Task UpdateFilterResultAsync()
        {
            await Parent.ExecuteAsync(() =>
            {
                using (var ps = Progress.Start( _originalListenerList.Count, LongRunningState.No))
                {
                    _filterProgressScope = ps;

                    Listeners.Clear();
                    ListenersFilterView.Refresh();
                    _filterProgressScope = null;
                }
            });
        }

        public SelectableItemCollection<ListenerInfo> Listeners { get; }

        private ICollectionView ListenersFilterView { get; set; }

        private async Task RefreshListenersInfoAsync(bool allListeners)
        {
            if (!Initialized) return;

            var count = allListeners ? _originalListenerList.Count : Listeners.TotalCount;

            await Parent.ExecuteAsync(() =>
            {
                using (var ps = Progress.Start(count))
                {
                    ps.SetTitle("Updating...");
                    var listenersToRefreshInfo = allListeners ? ListenersFilterView.SourceCollection : ListenersFilterView;

                    foreach (ListenerInfo l in listenersToRefreshInfo)
                    {
                        l.RefreshInfo();
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

        private bool _filterIncludeSystemListeners;
        public bool FilterIncludeSystemListeners
        {
            get { return _filterIncludeSystemListeners; }
            set
            {
                if (SetPropertyAndNotify(ref _filterIncludeSystemListeners, value))
                {
                    StartUpdateFilterResult();
                }
            }
        }

        private ListenerStatus? _filterStatus;
        public ListenerStatus? FilterStatus
        {
            get
            {
                return _filterStatus;
            }
            set
            {
                if (SetPropertyAndNotify(ref _filterStatus, value))
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


        private ListenerInfo _highlightedListener;
        public ListenerInfo HighlightedListener
        {
            get { return _highlightedListener; }
            set
            {
                SetPropertyAndNotify(ref _highlightedListener, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }


        public StatusInfoViewModel StatusInfoViewModel { get; private set; }
    }
}
