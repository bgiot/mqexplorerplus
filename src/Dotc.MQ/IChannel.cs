#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel;

namespace Dotc.MQ
{

    public enum ChannelStatus
    {
        Disconnected,
        Inactive,
        Binding,
        Starting,
        Running,
        Stopping,
        Retrying,
        Stopped,
        Requesting,
        Paused,
        Initializing,
        Switching,
    }

    public enum ChannelStopMode
    {
        Normal,
        Force,
        Terminate
    }

    public interface IChannel : INotifyPropertyChanged
    {
        string Name { get; }
        int Type { get; }
        string ConnectionName { get; }
        string TransmissionQueue { get; }
        bool IsSystemChannel { get; }
        ChannelStatus? Status { get; }
        bool? IndoubtStatus { get; }
        bool SupportReset { get; }
        bool SupportResolve { get; }
        string UniqueId { get; }
        IQueueManager QueueManager { get; }
        void RefreshInfo();
        void Start();
        void Stop(ChannelStopMode mode = ChannelStopMode.Normal, bool setInactive = false);
        void Reset(int msgSequenceNumber = 1);
        void Resolve(bool commit = false);
    }
}
