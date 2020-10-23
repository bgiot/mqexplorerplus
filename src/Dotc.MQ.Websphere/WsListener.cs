#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.Common;
using IBM.WMQ;
using System.Collections;
using System.Diagnostics;
using static System.FormattableString;


namespace Dotc.MQ.Websphere
{
    public class WsListener : ObservableObject, IListener
    {

        internal WsListener(WsQueueManager qmOwner, string name)
        {
            Debug.Assert(name != null);
            Debug.Assert(qmOwner != null);

            QueueManager = qmOwner;
            Name = name.Trim();
            IsSystemListener = WsSystemObjectNameFilter.IsSystemListener(name);

        }


        public bool IsSystemListener
        {
            get;
        }

        public string Name
        {
            get;
        }

        public IQueueManager QueueManager
        {
            get;
        }

        private string _ip;
        public string Ip
        {
            get { return _ip; }
            internal set { SetPropertyAndNotify(ref _ip, value); }
        }

        private int? _port;
        public int? Port
        {
            get { return _port; }
            internal set { SetPropertyAndNotify(ref _port, value); }
        }

        private string _protocol;
        public string Protocol
        {
            get { return _protocol; }
            internal set { SetPropertyAndNotify(ref _protocol, value); }
        }


        private ListenerStatus? _status;
        public ListenerStatus? Status
        {
            get { return _status; }
            internal set { SetPropertyAndNotify(ref _status, value); }
        }

        public string UniqueId => Invariant($"{Name}@{QueueManager.UniqueId}");

        private void AddExtraInfoToError(IDictionary data)
        {
            data.AddOrReplace("Listener", this.Name);
            data.AddOrReplace("Connection", QueueManager.ConnectionInfo);
        }
        public void RefreshInfo()
        {
            try
            {
                ((WsQueueManager)QueueManager).RefreshListenerInfosCore(this);
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
                ((WsQueueManager)QueueManager).StartListenerCore(this);
                RefreshInfo();
            }
            catch (MQException ibmEx)
            {
                if (ibmEx.ReasonCode == MQC.MQRCCF_LISTENER_RUNNING)
                    return;
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void Stop()
        {
            try
            {
                ((WsQueueManager)QueueManager).StopListenerCore(this);
                RefreshInfo();
            }
            catch (MQException ibmEx)
            {
                if (ibmEx.ReasonCode == MQC.MQRCCF_LISTENER_STOPPED)
                    return;
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }
    }
}
