#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Dotc.MQ
{

    public enum GetPutStatus
    {
        Allowed,
        Inhibited
    }

    public interface IQueue : INotifyPropertyChanged
    {
        string Name { get; }
        int Type { get; }
        int? Depth { get; }

        bool IsSystemQueue { get; }

        GetPutStatus? GetStatus { get; }
        GetPutStatus? PutStatus { get; }
        string UniqueId { get; }

        IQueue NewConnection();

        IQueueManager QueueManager { get; }
        dynamic ExtendedProperties { get; }

        void RefreshInfo();
        void SetGetStatus(GetPutStatus newStatus);
        void SetPutStatus(GetPutStatus newStatus);
        void DeleteMessages(IList<IMessage> messages, CancellationToken ct, IProgress<int> progress = null);
        IEnumerable<IMessage> GetMessages(int numberOfMessages, CancellationToken ct, IProgress<int> progress = null);
        IEnumerable<IMessage> BrowseMessages(int numberOfMessages, CancellationToken ct, byte[] startingPointMessageId = null, IBrowseFilter filter = null, IProgress<int> progress = null);
        void PutMessages(IList<IMessage> messages, CancellationToken ct, IProgress<int> progress = null);
        void ForwardMessages(IList<IMessage> messages, IQueue destinationQueue, CancellationToken ct, IProgress<int> progress = null);
        void ClearQueue(bool truncateMode);
        bool SupportTruncate {get; }
        IMessage NewMessage(string content, int? priority, int? characterSet, Dictionary<string, object> extendedProperties = null);

        IDump DumpEngine { get; }

    }

    public interface IBrowseFilter
    {
        bool IsMatch(IMessage message);
    }
}
