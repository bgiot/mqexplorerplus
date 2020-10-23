#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Collections.Generic;

namespace Dotc.MQ
{
    public interface IObjectProvider
    {
        IEnumerable<string> GetChannelNames();
        IEnumerable<IChannel> GetChannels();
        IObjectNameFilter Filter { get; }

        IEnumerable<string> GetListenerNames();
        IEnumerable<IListener> GetListeners();

        IEnumerable<string> GetQueueNames();
        IEnumerable<IQueue> GetQueues();

        bool SupportChannels { get; }
        bool SupportListeners { get; }
    }
}
