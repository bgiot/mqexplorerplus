#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQ.Websphere
{
    public class WsObjectProvider : IObjectProvider
    {

        internal WsObjectProvider(WsQueueManager qm, IObjectNameFilter filter)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            QueueManager = qm ?? throw new ArgumentNullException(nameof(qm));

            if (filter is StaticQueueList)
            {
                SupportChannels = false;
                SupportListeners = false;
            }
            else
            {
                SupportChannels = true;
                SupportListeners = true;
            }
        }

        internal WsQueueManager QueueManager { get; }
        public IObjectNameFilter Filter { get; }

        public bool SupportChannels  {get;}

        public bool SupportListeners { get; }

        public IEnumerable<string> GetChannelNames()
        {
            try
            {
                return QueueManager.GetChannelNames((WsObjectNameFilter)Filter);
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException((data) => data.AddOrReplace("Connection", QueueManager.ConnectionInfo));
            }
        }

        public IEnumerable<IChannel> GetChannels()
        {
            var names = GetChannelNames();

            foreach (var name in names)
            {
                yield return QueueManager.OpenChannel(name);
            }
        }

        public IEnumerable<string> GetListenerNames()
        {
            try
            {
                return QueueManager.GetListenerNames((WsObjectNameFilter)Filter);
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException((data) => data.AddOrReplace("Connection", QueueManager.ConnectionInfo));
            }
        }

        public IEnumerable<IListener> GetListeners()
        {
            var names = GetListenerNames();

            foreach (var name in names)
            {
                yield return QueueManager.OpenListener(name);
            }
        }

        public IEnumerable<string> GetQueueNames()
        {
            if (Filter is StaticQueueList)
            {
                return ((StaticQueueList)Filter).Names;
            }

            try
            {
                return QueueManager.GetQueueNames((WsObjectNameFilter)Filter);
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException((data) => data.AddOrReplace("Connection", QueueManager.ConnectionInfo));
            }
        }

        public IEnumerable<IQueue> GetQueues()
        {
            var queueNames = GetQueueNames();

            foreach (var name in queueNames)
            {
                yield return QueueManager.OpenQueue(name);
            }
        }
    }
}
