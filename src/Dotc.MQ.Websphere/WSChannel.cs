#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotc.Common;
using static System.FormattableString;
using Dotc.MQ.Websphere.Configuration;
using IBM.WMQ;
using System.Collections;

namespace Dotc.MQ.Websphere
{
    public class WsChannel : ObservableObject, IChannel
    {
        internal WsChannel(WsQueueManager qmOwner, string name)
        {
            Debug.Assert(name != null);
            Debug.Assert(qmOwner != null);

            QueueManager = qmOwner;
            Name = name.Trim();
            IsSystemChannel = WsSystemObjectNameFilter.IsSystemChannel(name);
            _type = WsChannelType.Unknown;

        }

        public bool IsSystemChannel { get; }

        public string Name { get; }

        public IQueueManager QueueManager { get; }

        private string _connName;
        public string ConnectionName
        {
            get { return _connName; }
            internal set
            {
                SetPropertyAndNotify(ref _connName, value);
            }
        }

        private string _transQueue;

        public string TransmissionQueue
        {
            get { return _transQueue; }
            internal set
            {
                SetPropertyAndNotify(ref _transQueue, value);
            }
        }

        private ChannelStatus? _status;
        public ChannelStatus? Status
        {
            get { return _status; }
            internal set
            {
                SetPropertyAndNotify(ref _status, value);
            }
        }

        private bool? _indoubtStatus;
        public bool? IndoubtStatus
        {
            get { return _indoubtStatus; }
            internal set
            {
                SetPropertyAndNotify(ref _indoubtStatus, value);
            }
        }

        private int _type;
        public int Type
        {
            get { return _type; }
            internal set { SetPropertyAndNotify(ref _type, value); }
        }

        public string UniqueId => Invariant($"{Name}@{QueueManager.UniqueId}");

        internal int ChannelTypeCore { get; set; }

        public bool SupportReset
        {
            get
            {
                return (Type != WsChannelType.Amqp && Type != WsChannelType.ServerConnection );
            }
        }

        public bool SupportResolve
        {
            get
            {
                return (Type == WsChannelType.Sender || Type == WsChannelType.ClusterSender || Type == WsChannelType.Server);
            }
        }

        private void AddExtraInfoToError(IDictionary data)
        {
            data.AddOrReplace("Channel", this.Name);
            data.AddOrReplace("Connection", QueueManager.ConnectionInfo );
        }
        public void RefreshInfo()
        {
            try
            {
                ((WsQueueManager)QueueManager).RefreshChannelInfosCore(this);
            }
            catch (MQException ibmEx)
            {
                if (ibmEx.ReasonCode == MQC.MQRC_UNKNOWN_OBJECT_NAME)
                    return;

                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void Start()
        {
            try
            {
                ((WsQueueManager)QueueManager).StartChannelCore(this);
                RefreshInfo();
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void Stop(ChannelStopMode action = ChannelStopMode.Normal, bool setInactive = false)
        {
            try
            {
                ((WsQueueManager)QueueManager).StopChannelCore(this, action, setInactive);
                RefreshInfo();
            }
            catch (MQException ibmEx)
            {
                if (ibmEx.ReasonCode == MQC.MQRCCF_CHANNEL_NOT_ACTIVE)
                    return;
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void Reset(int msgSequenceNumber = 1)
        {
            if (msgSequenceNumber < 1 || msgSequenceNumber > 999999999) throw new ArgumentOutOfRangeException(nameof(msgSequenceNumber), msgSequenceNumber, "Message sequence number must between 1 and 999999999");
            try
            {
                ((WsQueueManager)QueueManager).ResetChannelCore(this, msgSequenceNumber);
                RefreshInfo();
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void Resolve(bool commit = false)
        {
            try
            {
                ((WsQueueManager)QueueManager).ResolveChannelCore(this, commit);
                RefreshInfo();
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }
    }
}
