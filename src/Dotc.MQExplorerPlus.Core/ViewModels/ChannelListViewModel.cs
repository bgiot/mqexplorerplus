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

namespace Dotc.MQExplorerPlus.Core.ViewModels
{

    public sealed class ChannelListViewModel : ViewModelBase, IStatusInfo
    {
        private List<ChannelInfo> _originalChannelList;

        internal ChannelListViewModel(QueueManagerViewModel parent)
        {
            Progress = new RangeProgress();
            BuildCommands();
            Channels = new SelectableItemCollection<ChannelInfo>();
            BindingOperations.EnableCollectionSynchronization(Channels, new object());
            Parent = parent;
            StatusInfoViewModel = new ChannelListStatusInfo(this);
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
            if (!ObjectProvider.SupportChannels)
            {
                Initialized = false;
                return;
            }

            await Parent.ExecuteAsync(() =>
            {
                Channels.Clear();

                ((ChannelListStatusInfo)StatusInfoViewModel).ConnectionInformation = QM.ConnectionInfo;

                var cInfos = new List<ChannelInfo>();

                Initialized = Parent.ExecuteGuarded(() =>
                {
                    foreach (var c in ObjectProvider.GetChannels())
                    {
                        cInfos.Add(new ChannelInfo(c, Parent.App.UserSettings));
                    }
                });

                InitCollectionView(cInfos);

            });
        }

        private bool _hasSystemChannels;

        public bool HasSystemChannels
        {
            get { return _hasSystemChannels; }
            set { SetPropertyAndNotify(ref _hasSystemChannels, value); }
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
                    await RefreshChannelsInfoAsync(false);
                }, () => Initialized);

            ApplyFilterCommand = new AwaitableRelayCommand(
                 async () =>
                {
                     await UpdateFilterResultAsync();
                }, () => Initialized);

            SelectAllCommand = CreateCommand(
                () =>
                {
                    foreach (ChannelInfo c in ChannelsFilterView)
                    {
                        c.Selected = true;
                    }
                }, () => Initialized);

            SelectNoneCommand = CreateCommand(
                () =>
                {
                    foreach (ChannelInfo c in ChannelsFilterView)
                    {
                        c.Selected = false;
                    }
                }, () => Initialized);

            InvertSelectionCommand = CreateCommand(
                () =>
                {
                    foreach (ChannelInfo c in ChannelsFilterView)
                    {
                        c.Selected = !c.Selected;
                    }
                }, () => Initialized);

            BuildContextMenuCommands();
        }


        public ICommand SelectedStartCommand { get; private set; }
        public ICommand SelectedStopCommand { get; private set; }
        public ICommand SelectedResetCommand { get; private set; }
        public ICommand SelectedResolveCommand { get; private set; }

        private void BuildContextMenuCommands()
        {
            SelectedStartCommand = new AwaitableRelayCommand(
                async () => await StartChannelsAsync(GetHighlightedChannelAsList()),
                () => HighlightedChannel != null
                );
            SelectedStopCommand = new AwaitableRelayCommand(
                async () => await AskStopChannels(GetHighlightedChannelAsList()),
                () => HighlightedChannel != null
                );
            SelectedResetCommand = new AwaitableRelayCommand(
                 async () => await AskResetChannels(GetHighlightedChannelAsList()),
                () => HighlightedChannel != null
                );
            SelectedResolveCommand = new AwaitableRelayCommand(
                 async () =>  await AskResolveChannels(GetHighlightedChannelAsList()),
                () => HighlightedChannel != null
                );
        }

        private async Task StartChannelsAsync(IList<ChannelInfo> list)
        {

            await Parent.ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(list.Count, ct))
                {
                    ps.SetTitle("Starting channels...");
                    foreach (var c in list)
                    {
                        if (ps.CancellationToken.IsCancellationRequested)
                            break;
                        c.Start();
                        ps.ReportNext();
                    }
                }
            });
        }

        private async Task AskStopChannels(IList<ChannelInfo> list)
        {
            await Task.Yield();
            Parent.App.OpenChannelStopParametersView(async (p) =>
            {
                await StopChannelsAsync(list, p);
            });
        }

        private async Task StopChannelsAsync(IList<ChannelInfo> list, ChannelStopParameters parameters)
        {

            await Parent.ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(list.Count, ct))
                {
                    ps.SetTitle("Stopping channels...");
                    foreach (var c in list)
                    {
                        if (ps.CancellationToken.IsCancellationRequested)
                            break;
                        c.Stop(parameters);
                        ps.ReportNext();
                    }
                }
            });
        }

        private async Task AskResetChannels(IList<ChannelInfo> list)
        {
            await Task.Yield();
            Parent.App.OpenChannelResetParametersView(async (p) =>
            {
                await ResetChannelsAsync(list, p);
            });
        }

        private async Task ResetChannelsAsync(IList<ChannelInfo> list, ChannelResetParameters parameters)
        {

            await Parent.ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(list.Count, ct))
                {
                    ps.SetTitle("Resetting channels...");
                    foreach (var c in list)
                    {
                        if (ps.CancellationToken.IsCancellationRequested)
                            break;
                        c.Reset(parameters);
                        ps.ReportNext();
                    }
                }
            });
        }
        private async Task AskResolveChannels(IList<ChannelInfo> list)
        {
            await Task.Yield();
            Parent.App.OpenChannelResolveParametersView(async (p) =>
            {
                await ResolveChannelsAsync(list, p);
            });
        }

        private async Task ResolveChannelsAsync(IList<ChannelInfo> list, ChannelResolveParameters parameters)
        {

            await Parent.ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(list.Count, ct))
                {
                    ps.SetTitle("Resolving channels...");
                    foreach (var c in list)
                    {
                        if (ps.CancellationToken.IsCancellationRequested)
                            break;
                        c.Resolve(parameters);
                        ps.ReportNext();
                    }
                }
            });
        }
        private IList<ChannelInfo> GetHighlightedChannelAsList()
        {
            return new[] { HighlightedChannel };
        }


        private void InitCollectionView(List<ChannelInfo> channels)
        {
            _originalChannelList = channels;

            HasSystemChannels = channels.Any(q => q.ChannelSource.IsSystemChannel);

            ChannelsFilterView = (CollectionView)CollectionViewSource.GetDefaultView(channels);
            Channels.Clear();
            using (_filterProgressScope = Progress.Start(channels.Count, LongRunningState.No))
            {
                using (ChannelsFilterView.DeferRefresh())
                {
                    ChannelsFilterView.Filter = OnChannelsViewFilter;
                    ChannelsFilterView.SortDescriptions.Add(
                        new SortDescription("Name", ListSortDirection.Ascending));
                }
            }

            StatusInfoViewModel.LastUpdateTimestamp = DateTime.Now;

        }

        private ProgressActiveScope _filterProgressScope;
        private bool OnChannelsViewFilter(object item)
        {
            _filterProgressScope?.ReportNext();

            var c = (ChannelInfo)item;

            if (!FilterIncludeSystemChannels && c.ChannelSource.IsSystemChannel)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(FilterByName))
            {
                if (c.Name.IndexOf(FilterByName, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    return false;
                }
            }

            Parent.ExecuteGuarded(()=> c.RefreshInfo());

            if (FilterType.HasValue)
            {
                if (c.Type != FilterType.Value) { return false; }
            }

            if (FilterIndoubtChannels)
            {
                if (c.IndoubtStatus.HasValue == false || c.IndoubtStatus == false) { return false; }
            }

            if (FilterStatus.HasValue)
            {
                if (c.Status != FilterStatus.Value) { return false; }
            }

            Channels.Add(c);

            return true;
        }

        private async Task UpdateFilterResultAsync()
        {
            await Parent.ExecuteAsync(() =>
            {
                using (var ps = Progress.Start(_originalChannelList.Count, LongRunningState.No))
                {
                    _filterProgressScope = ps;

                    Channels.Clear();
                    ChannelsFilterView.Refresh();
                    _filterProgressScope = null;
                }
            });
        }

        public SelectableItemCollection<ChannelInfo> Channels { get; }

        private ICollectionView ChannelsFilterView { get; set; }

        private async Task RefreshChannelsInfoAsync(bool allChannels)
        {
            if (!Initialized) return;

            var count = allChannels ? _originalChannelList.Count : Channels.TotalCount;

            await Parent.ExecuteAsync(() =>
            {
                using (var ps = Progress.Start(count))
                {
                    ps.SetTitle("Refreshing...");
                    var channelsToRefreshInfo = allChannels ? ChannelsFilterView.SourceCollection : ChannelsFilterView;
                    foreach (ChannelInfo c in channelsToRefreshInfo)
                    {
                        c.RefreshInfo();
                        ps.ReportNext();
                    }
                }
                StatusInfoViewModel.LastUpdateTimestamp = DateTime.Now;
            });
        }

        private  void StartUpdateFilterResult()
        {
            var _ = UpdateFilterResultAsync();
        }

        private bool _filterIncludeSystemChannels;
        public bool FilterIncludeSystemChannels
        {
            get { return _filterIncludeSystemChannels; }
            set
            {
                if (SetPropertyAndNotify(ref _filterIncludeSystemChannels, value))
                {
                    StartUpdateFilterResult();
                }
            }
        }

        private bool _filterIndoubtChannels;
        public bool FilterIndoubtChannels
        {
            get { return _filterIndoubtChannels; }
            set
            {
                if (SetPropertyAndNotify(ref _filterIndoubtChannels, value))
                {
                    StartUpdateFilterResult();
                }
            }
        }

        private ChannelStatus? _filterStatus;
        public ChannelStatus? FilterStatus
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


        private ChannelInfo _highlightedChannel;
        public ChannelInfo HighlightedChannel
        {
            get { return _highlightedChannel; }
            set
            {
                SetPropertyAndNotify(ref _highlightedChannel, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }


        public StatusInfoViewModel StatusInfoViewModel { get; private set; }

    }

}
