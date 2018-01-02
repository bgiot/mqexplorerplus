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
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQ
{
    public enum ListenerStatus
    {
        Starting,
        Running,
        Stopping,
        Stopped,
        Retrying,
    }
    public interface IListener : INotifyPropertyChanged
    {
        string Name { get; }
        bool IsSystemListener { get; }
        int? Port { get; }
        string Ip { get; }
        string Protocol { get; }
        ListenerStatus? Status { get; }
        string UniqueId { get; }
        IQueueManager QueueManager { get; }
        void RefreshInfo();
        void Start();
        void Stop();

    }
}
