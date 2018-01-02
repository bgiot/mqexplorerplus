#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public sealed class ListenerListStatusInfo : StatusInfoViewModel
    {
        internal ListenerListStatusInfo(ListenerListViewModel owner) : base()
        {
            Owner = owner;
            WeakEventManager<SelectableItemCollection<ListenerInfo>, PropertyChangedEventArgs>
                .AddHandler(Owner.Listeners, "CountChanged", Queues_CountChanged);
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

        private ListenerListViewModel Owner { get; set; }

        private string _connectionInformation;
        public string ConnectionInformation
        {
            get { return _connectionInformation; }
            set
            {
                SetPropertyAndNotify(ref _connectionInformation, value);
            }
        }

        public int SelectedCount { get { return Owner.Listeners.SelectedCount; } }
        public int TotalCount { get { return Owner.Listeners.TotalCount; } }

        public RangeProgress Progress { get { return Owner.Progress; } }

        public CountdownService Countdown
        {
            get { return Owner.ShellService.Countdown; }
        }

        public QueueManagerViewModel Parent { get { return Owner.Parent; } }
    }
}
