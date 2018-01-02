#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel;
using System.Windows;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{

    public sealed class ChannelListStatusInfo : StatusInfoViewModel
    {
        internal ChannelListStatusInfo(ChannelListViewModel clvm) : base()
        {
            Owner = clvm;
            WeakEventManager<SelectableItemCollection<ChannelInfo>, PropertyChangedEventArgs>
                .AddHandler(Owner.Channels, "CountChanged", Queues_CountChanged);
        }

        private void Queues_CountChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TotalCount")
            {
                OnPropertyChanged(nameof(TotalCount));
            }
            if (e.PropertyName == "SelectedCount")
            {
                OnPropertyChanged(nameof(SelectedCount));
            }
        }

        private ChannelListViewModel Owner { get; set; }

        private string _connectionInformation;
        public string ConnectionInformation
        {
            get { return _connectionInformation; }
            set
            {
                SetPropertyAndNotify(ref _connectionInformation, value);
            }
        }

        public int SelectedCount { get { return Owner.Channels.SelectedCount; } }
        public int TotalCount { get { return Owner.Channels.TotalCount; } }

        public RangeProgress Progress { get { return Owner.Progress; } }

        public CountdownService Countdown
        {
            get { return Owner.ShellService.Countdown; }
        }

        public QueueManagerViewModel Parent { get { return Owner.Parent; } }
    }

}
