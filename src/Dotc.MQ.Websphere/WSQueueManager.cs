#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using IBM.WMQ;
using IBM.WMQ.PCF;
using static System.FormattableString;
using Dotc.MQ.Websphere.Configuration;
using System.Reflection;

namespace Dotc.MQ.Websphere
{


    internal enum OpenQueueMode
    {
        ForSet,
        ForBrowse,
        ForBrowseCo,
        ForRead,
        ForWrite,
        ForWriteSetAllContext,
        ForWriteSetIdentityContext,
        ForWritePassAllContext,
        ForWritePassIdentityContext,
        ForQuery,
        ForBrowseAndRead,
    }


    internal class WsConnectionProperties : IConnectionProperties
    {
        internal Hashtable CoreData { get; private set; }

        internal static WsConnectionProperties Default
        {
            get
            {
                return new WsConnectionProperties();
            }
        }
        private WsConnectionProperties()
        {
            Reset();
        }

        public void Set(string hostName, int port, string channel, string userId = null, SecureString password = null)
        {

            if (string.IsNullOrEmpty(hostName)) throw new ArgumentNullException(nameof(hostName));
            if (string.IsNullOrEmpty(channel)) throw new ArgumentNullException(nameof(channel));
            if (port <= 0) throw new ArgumentException("port must be positive number", nameof(port));

            Reset();

            IsLocal = false;
            HostName = hostName;
            Port = port;
            Channel = channel;
            Transport = WSConfiguration.Current.RemoteConnectionTransport;

            if (userId == null) return;
            UserId = userId;

            if (password?.Length > 0)
                Password = SecureStringHelper.ConvertToUnsecureString(password);
            else
                Password = String.Empty;

            string exitTypename = typeof(MCASecurityExit).FullName;
            string exitLocation = Assembly.GetExecutingAssembly().Location;
            SecurityExit = string.Format("{0}({1})", exitLocation, exitTypename);

        }

        public void Reset()
        {
            CoreData = new Hashtable();
            IsLocal = true;
        }

        private void SetValue(object value, string propName)
        {
            CoreData[propName] = value;
        }

        private T GetValue<T>(string propName)
        {
            var obj = CoreData[propName];
            return obj != null ? (T)obj : default(T);
        }

        private string Transport
        {
            get { return GetValue<string>(MQC.TRANSPORT_PROPERTY); }
            set { SetValue(value, MQC.TRANSPORT_PROPERTY); }
        }

        private string SecurityExit
        {
            get { return GetValue<string>(MQC.SECURITY_EXIT_PROPERTY); }
            set { SetValue(value, MQC.SECURITY_EXIT_PROPERTY); }
        }

        public string HostName
        {
            get { return GetValue<string>(MQC.HOST_NAME_PROPERTY); }
            private set { SetValue(value, MQC.HOST_NAME_PROPERTY); }
        }

        public int Port
        {
            get { return GetValue<int>(MQC.PORT_PROPERTY); }
            private set { SetValue(value, MQC.PORT_PROPERTY); }
        }

        public string Channel
        {
            get { return GetValue<string>(MQC.CHANNEL_PROPERTY); }
            private set { SetValue(value, MQC.CHANNEL_PROPERTY); }
        }

        public string UserId
        {
            get { return GetValue<string>(MQC.USER_ID_PROPERTY); }
            private set { SetValue(value, MQC.USER_ID_PROPERTY); }
        }

        public string Password
        {
            get { return GetValue<string>(MQC.PASSWORD_PROPERTY); }
            private set { SetValue(value, MQC.PASSWORD_PROPERTY); }
        }

        public bool IsLocal { get; private set; }
    }

    internal sealed class WsQueueManager : IQueueManager, IDisposable
    {

        private readonly WsMessageAgent _messageAgent;

        private MQQueueManager IbmQueueManager { get; }

        public IConnectionProperties ConnectionProperties { get; }


        internal WsQueueManager(MQQueueManager ibmQm, WsConnectionProperties properties)
        {
            Debug.Assert(ibmQm != null);
            IbmQueueManager = ibmQm;
            ConnectionProperties = properties;
            _messageAgent = new WsMessageAgent(ibmQm);
            GC.SuppressFinalize(this);
        }


        public string Name => IbmQueueManager.Name.TrimEnd();

        public string ConnectionInfo
        {
            get
            {
                if (ConnectionProperties.IsLocal)
                {
                    return string.Concat(Name, " (local)");
                }
                return Invariant(
                    $"{Name} ({ConnectionProperties.UserId ?? "<nobody>"}@{ConnectionProperties.HostName}:{ConnectionProperties.Port}/{ConnectionProperties.Channel})"
                );
            }
        }


        public string UniqueId
        {
            get
            {
                if (ConnectionProperties.IsLocal)
                {
                    return string.Concat(Name, "/localhost");
                }
                return Invariant(
                    $"{Name}/{ConnectionProperties.HostName}/{ConnectionProperties.Port}/{ConnectionProperties.Channel}/{ConnectionProperties.UserId}"
                );
            }
        }

        public int DefaultCharacterSet => IbmQueueManager.CharacterSet;

        const int MaxRetryOnHConnError = 2;

        private RT RetryOnHCONNError<RT>(Func<RT> action)
        {
            int tries = MaxRetryOnHConnError;
            while (true)
            {
                try
                {
                    if (tries < MaxRetryOnHConnError)
                        IbmQueueManager.Connect();

                    return action();
                }
                catch (MQException ex)
                {
                    if (ex.ReasonCode == MQC.MQRC_HCONN_ERROR
                        || ex.ReasonCode == MQC.MQRC_CONNECTION_BROKEN)
                    {
                        tries--;
                        if (tries == 0)
                            throw;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private void RetryOnHCONNError(Action action)
        {

            int tries = MaxRetryOnHConnError;
            while (true)
            {
                try
                {
                    if (tries < MaxRetryOnHConnError)
                        IbmQueueManager.Connect();

                    action();
                    return;
                }
                catch (MQException ex)
                {
                    if (ex.ReasonCode == MQC.MQRC_HCONN_ERROR
                        || ex.ReasonCode == MQC.MQRC_CONNECTION_BROKEN)
                    {
                        tries--;
                        if (tries == 0)
                            throw;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        internal IList<string> GetChannelNames(WsObjectNameFilter filter)
        {

            return RetryOnHCONNError(() =>
            {
                var result = new List<string>();

                var pcfMsg = _messageAgent.NewRequest(MQC.MQCMD_INQUIRE_CHANNEL_NAMES);

                if (filter != null)
                {
                    pcfMsg.AddParameter(MQC.MQCACH_CHANNEL_NAME,
                        string.IsNullOrEmpty(filter.NamePrefix[0]) ? "*" : string.Concat(filter.NamePrefix[0], "*"));
                }
                else
                {
                    pcfMsg.AddParameter(MQC.MQCACH_CHANNEL_NAME, "*");
                }

                pcfMsg.AddParameter(MQC.MQIACH_CHANNEL_TYPE, MQC.MQCHT_ALL);

                var pcfResponse = _messageAgent.Send(pcfMsg);

                result.AddRange(pcfResponse[0].GetStringListParameterValue(MQC.MQCACH_CHANNEL_NAMES));

                return result.Select(x => x.Trim()).Where(n => filter == null || filter.IsMatch(n)).ToList();
            });


        }

        private void AddExtraInfoToError(IDictionary data)
        {
            data.AddOrReplace("Connection", this.ConnectionInfo);
        }

        public void Disconnect()
        {
            try
            {
                IbmQueueManager.Disconnect();

            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public IChannel OpenChannel(string channelName, bool autoLoadInfo = false)
        {
            if (string.IsNullOrEmpty(channelName)) throw new ArgumentNullException(nameof(channelName));

            try
            {
                var c = new WsChannel(this, channelName);
                if (autoLoadInfo)
                    c.RefreshInfo();

                return c;
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        internal IList<string> GetQueueNames(WsObjectNameFilter filter)
        {
            return RetryOnHCONNError(() =>
            {
                var result = new List<string>();

                var pcfMsg = _messageAgent.NewRequest(MQC.MQCMD_INQUIRE_Q_NAMES);

                if (filter != null)
                {
                    pcfMsg.AddParameter(MQC.MQCA_Q_NAME,
                        string.IsNullOrEmpty(filter.NamePrefix[0]) ? "*" : string.Concat(filter.NamePrefix[0], "*"));
                }
                else
                {
                    pcfMsg.AddParameter(MQC.MQCA_Q_NAME, "*");
                }

                pcfMsg.AddParameter(MQC.MQIA_Q_TYPE, MQC.MQQT_ALL);

                var pcfResponse = _messageAgent.Send(pcfMsg);

                result.AddRange(pcfResponse[0].GetStringListParameterValue(MQC.MQCACF_Q_NAMES));

                return result.Select(x => x.Trim()).Where(n => filter == null || filter.IsMatch(n)).ToList();
            });

        }

        public IObjectNameFilter NewObjectNameFilter(string namePrefix = null)
        {
            return new WsObjectNameFilter(namePrefix);
        }

        public IObjectNameFilter NewSystemObjectNameFilter()
        {
            return new WsSystemObjectNameFilter();
        }

        public IQueue OpenQueue(string queueName, bool autoLoadInfo = false)
        {

            if (string.IsNullOrEmpty(queueName)) throw new ArgumentNullException(nameof(queueName));

            try
            {
                var q = new WsQueue(this, queueName);
                if (autoLoadInfo)
                    q.RefreshInfo();
                return q;

            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        internal MQQueue OpenQueueCore(string queueName, OpenQueueMode openMode)
        {

            Debug.Assert(!string.IsNullOrEmpty(queueName));

            var openFlags = MQC.MQOO_FAIL_IF_QUIESCING;

            switch (openMode)
            {
                case OpenQueueMode.ForBrowse:
                    openFlags = openFlags | MQC.MQOO_BROWSE;
                    break;
                case OpenQueueMode.ForBrowseCo:
                    openFlags = openFlags | MQC.MQOO_CO_OP | MQC.MQOO_BROWSE;
                    break;
                case OpenQueueMode.ForSet:
                    openFlags = openFlags | MQC.MQOO_SET;
                    break;
                case OpenQueueMode.ForRead:
                    openFlags = openFlags | MQC.MQOO_INPUT_SHARED;
                    break;
                case OpenQueueMode.ForWrite:
                    openFlags = openFlags | MQC.MQOO_OUTPUT;
                    break;
                case OpenQueueMode.ForWriteSetAllContext:
                    openFlags = openFlags | MQC.MQOO_OUTPUT | MQC.MQOO_SET_ALL_CONTEXT;
                    break;
                case OpenQueueMode.ForWriteSetIdentityContext:
                    openFlags = openFlags | MQC.MQOO_OUTPUT | MQC.MQOO_SET_IDENTITY_CONTEXT;
                    break;
                case OpenQueueMode.ForWritePassAllContext:
                    openFlags = openFlags | MQC.MQOO_OUTPUT | MQC.MQOO_PASS_ALL_CONTEXT;
                    break;
                case OpenQueueMode.ForWritePassIdentityContext:
                    openFlags = openFlags | MQC.MQOO_OUTPUT | MQC.MQOO_PASS_IDENTITY_CONTEXT;
                    break;
                case OpenQueueMode.ForQuery:
                    openFlags = openFlags | MQC.MQOO_INQUIRE;
                    break;
                case OpenQueueMode.ForBrowseAndRead:
                    openFlags = openFlags | MQC.MQOO_BROWSE | MQC.MQOO_INPUT_SHARED;
                    break;
            }

            return RetryOnHCONNError(() =>
            {
                return IbmQueueManager.AccessQueue(queueName, openFlags);
            });
        }

        public void Commit()
        {
            try
            {
                IbmQueueManager.Commit();

            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void Rollback()
        {
            try
            {
                IbmQueueManager.Backout();

            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }


        #region internal methods used by Queue object

        internal void RefreshQueueInfosCore(WsQueue theQueue)
        {
            RetryOnHCONNError(() =>
            {
                Exception pcfError = null;

                if (WsSoftwareInfo.AvoidPCFWhenPossible == false)
                {
                    var inquireQueueInfo = _messageAgent.NewRequest(MQC.MQCMD_INQUIRE_Q);
                    inquireQueueInfo.AddParameter(MQC.MQCA_Q_NAME, theQueue.Name);
                    inquireQueueInfo.AddParameter(MQC.MQIACF_Q_ATTRS, new[]
                    {
                        MQC.MQIA_Q_TYPE,
                        MQC.MQIA_USAGE,
                        MQC.MQCA_BASE_Q_NAME,
                        MQC.MQIA_MAX_Q_DEPTH,
                        MQC.MQIA_INHIBIT_GET,
                        MQC.MQIA_INHIBIT_PUT,
                        MQC.MQIA_DEF_PRIORITY,
                        MQC.MQCA_REMOTE_Q_MGR_NAME,
                        MQC.MQCA_REMOTE_Q_NAME,
                        MQC.MQCA_XMIT_Q_NAME
                    });

                    PCF.PcfMessage[] response;
                    if (_messageAgent.TrySend(inquireQueueInfo, out response))
                    {

                        var qType = response[0].GetIntParameterValue(MQC.MQIA_Q_TYPE);
                        theQueue.QueueTypeCore = qType;

                        var defPriority = response[0].GetIntParameterValue(MQC.MQIA_DEF_PRIORITY);
                        theQueue.DefaultPriority = defPriority;

                        theQueue.Type = WsQueueType.Unknown;

                        // Make sure all necessary properties have a value as null in case of not set by code under
                        theQueue.ExtendedProperties.UnderlyingName = null;
                        theQueue.ExtendedProperties.MaxDepth = null;
                        theQueue.ExtendedProperties.OpenReadCount = null;
                        theQueue.ExtendedProperties.OpenWriteCount = null;
                        theQueue.ExtendedProperties.UncommittedCount = null;

                        switch (qType)
                        {
                            case MQC.MQQT_ALIAS:
                                theQueue.Type = WsQueueType.Alias;
                                theQueue.ExtendedProperties.UnderlyingName =
                                    response[0].GetStringParameterValue(MQC.MQCA_BASE_Q_NAME);
                                break;
                            case MQC.MQQT_MODEL:
                                theQueue.Type = WsQueueType.Model;
                                theQueue.ExtendedProperties.MaxDepth = response[0].GetIntParameterValue(MQC.MQIA_MAX_Q_DEPTH);
                                break;
                            case MQC.MQQT_LOCAL:
                                theQueue.Type = response[0].GetIntParameterValue(MQC.MQIA_USAGE) == MQC.MQUS_TRANSMISSION
                                    ? WsQueueType.Transmission
                                    : WsQueueType.Local;
                                theQueue.ExtendedProperties.MaxDepth = response[0].GetIntParameterValue(MQC.MQIA_MAX_Q_DEPTH);
                                break;
                            case MQC.MQQT_REMOTE:
                                theQueue.Type = WsQueueType.Remote;
                                theQueue.ExtendedProperties.RemoteQueueName = response[0].GetStringParameterValue(MQC.MQCA_REMOTE_Q_NAME);
                                theQueue.ExtendedProperties.RemoteQueueManagerName = response[0].GetStringParameterValue(MQC.MQCA_REMOTE_Q_MGR_NAME);
                                theQueue.ExtendedProperties.TransmissionQueueName = response[0].GetStringParameterValue(MQC.MQCA_XMIT_Q_NAME);
                                break;
                        }

                        theQueue.PutStatus = response[0].GetIntParameterValue(MQC.MQIA_INHIBIT_PUT) ==
                                             MQC.MQQA_PUT_INHIBITED
                            ? GetPutStatus.Inhibited
                            : GetPutStatus.Allowed;

                        if (theQueue.Type != WsQueueType.Remote)
                        {
                            theQueue.GetStatus = response[0].GetIntParameterValue(MQC.MQIA_INHIBIT_GET) ==
                                                 MQC.MQQA_GET_INHIBITED
                                ? GetPutStatus.Inhibited
                                : GetPutStatus.Allowed;
                        }

                        if (theQueue.Type == WsQueueType.Local || theQueue.Type == WsQueueType.Transmission)
                        {

                            var inquireQueueStatus = _messageAgent.NewRequest(MQC.MQCMD_INQUIRE_Q_STATUS);
                            inquireQueueStatus.AddParameter(MQC.MQCA_Q_NAME, theQueue.Name);
                            inquireQueueStatus.AddParameter(MQC.MQIACF_Q_STATUS_ATTRS, new[]
                            {
                                MQC.MQIA_CURRENT_Q_DEPTH,
                                MQC.MQIA_OPEN_INPUT_COUNT,
                                MQC.MQIA_OPEN_OUTPUT_COUNT,
                                MQC.MQIACF_UNCOMMITTED_MSGS
                            });

                            response = _messageAgent.Send(inquireQueueStatus);

                            theQueue.Depth = response[0].GetIntParameterValue(MQC.MQIA_CURRENT_Q_DEPTH);
                            theQueue.ExtendedProperties.OpenReadCount =
                                response[0].GetIntParameterValue(MQC.MQIA_OPEN_INPUT_COUNT);
                            theQueue.ExtendedProperties.OpenWriteCount =
                                response[0].GetIntParameterValue(MQC.MQIA_OPEN_OUTPUT_COUNT);
                            theQueue.ExtendedProperties.UncommittedCount =
                                response[0].GetIntParameterValue(MQC.MQIACF_UNCOMMITTED_MSGS);
                        }
                        return; // use of PCF message is successfull
                    }

                    pcfError = _messageAgent.LastError;
                }

                // Fallback option

                try
                {
                    var mqQ = OpenQueueCore(theQueue.Name, OpenQueueMode.ForQuery);

                    theQueue.QueueTypeCore = mqQ.QueueType;

                    theQueue.Type = WsQueueType.Unknown;

                    // Make sure all necessary properties have a value as null in case of not set by code under
                    theQueue.ExtendedProperties.UnderlyingName = null;
                    theQueue.ExtendedProperties.MaxDepth = null;
                    theQueue.ExtendedProperties.OpenReadCount = null;
                    theQueue.ExtendedProperties.OpenWriteCount = null;
                    theQueue.ExtendedProperties.UncommittedCount = null;

                    switch (mqQ.QueueType)
                    {
                        case MQC.MQQT_ALIAS:
                            theQueue.Type = WsQueueType.Alias;
                            theQueue.ExtendedProperties.UnderlyingName = mqQ.BaseQueueName;
                            break;
                        case MQC.MQQT_MODEL:
                            theQueue.Type = WsQueueType.Model;
                            theQueue.ExtendedProperties.MaxDepth = mqQ.MaximumDepth;
                            break;
                        case MQC.MQQT_LOCAL:
                            theQueue.Type = mqQ.Usage == MQC.MQUS_TRANSMISSION
                                ? WsQueueType.Transmission
                                : WsQueueType.Local;
                            theQueue.ExtendedProperties.MaxDepth = mqQ.MaximumDepth;
                            break;
                        case MQC.MQQT_REMOTE:
                            theQueue.Type = WsQueueType.Remote;
                            break;
                    }

                    theQueue.PutStatus = mqQ.InhibitPut == MQC.MQQA_PUT_INHIBITED
                        ? GetPutStatus.Inhibited
                        : GetPutStatus.Allowed;

                    if (theQueue.Type != WsQueueType.Remote)
                    {
                        theQueue.GetStatus = mqQ.InhibitGet == MQC.MQQA_GET_INHIBITED
                            ? GetPutStatus.Inhibited
                            : GetPutStatus.Allowed;
                    }

                    if (theQueue.Type == WsQueueType.Local || theQueue.Type == WsQueueType.Transmission)
                    {

                        theQueue.Depth = mqQ.CurrentDepth;
                        theQueue.ExtendedProperties.OpenReadCount =
                            mqQ.OpenInputCount;
                        theQueue.ExtendedProperties.OpenWriteCount =
                            mqQ.OpenOutputCount;
                        theQueue.ExtendedProperties.UncommittedCount = "?"; // not possible without pcf message
                    }
                    mqQ.Close();
                }
                catch (MQException ex)
                {
                    if (pcfError != null)
                        ex.Data.Add("PCFError", pcfError.ToString());
                    throw;
                }
            });

        }

        internal void TruncateQueueCore(WsQueue theQueue)
        {
            RetryOnHCONNError(() =>
           {
               var pcfMsg = _messageAgent.NewRequest(MQC.MQCMD_CLEAR_Q);
               pcfMsg.AddParameter(MQC.MQCA_Q_NAME, theQueue.Name);
               _messageAgent.Send(pcfMsg);
           });
        }

        internal void SetQueueGetInhibitCore(WsQueue theQueue, GetPutStatus newStatus)
        {
            RetryOnHCONNError(() =>
            {
                Exception pcfError = null;

                if (WsSoftwareInfo.AvoidPCFWhenPossible == false)
                {
                    var pcfMsg = _messageAgent.NewRequest(MQC.MQCMD_CHANGE_Q);
                    pcfMsg.AddParameter(MQC.MQCA_Q_NAME, theQueue.Name);
                    pcfMsg.AddParameter(MQC.MQIA_Q_TYPE, theQueue.QueueTypeCore);
                    pcfMsg.AddParameter(MQC.MQIA_INHIBIT_GET,
                        newStatus == GetPutStatus.Inhibited ? MQC.MQQA_GET_INHIBITED : MQC.MQQA_GET_ALLOWED);

                    if (_messageAgent.TrySend(pcfMsg))
                        return;

                    pcfError = _messageAgent.LastError;
                }

                // fallback option

                try
                {
                    MQQueue mqQ = OpenQueueCore(theQueue.Name, OpenQueueMode.ForSet);
                    mqQ.InhibitGet = newStatus == GetPutStatus.Inhibited ? MQC.MQQA_GET_INHIBITED : MQC.MQQA_GET_ALLOWED;
                    mqQ.Close();
                }
                catch (MQException ex)
                {
                    if (pcfError != null)
                        ex.Data.Add("PCFError", pcfError.ToString());
                    throw;
                }
            });
        }

        internal void SetQueuePutInhibitCore(WsQueue theQueue, GetPutStatus newStatus)
        {
            RetryOnHCONNError(() =>
            {
                Exception pcfError = null;

                if (WsSoftwareInfo.AvoidPCFWhenPossible == false)
                {
                    var pcfMsg = _messageAgent.NewRequest(MQC.MQCMD_CHANGE_Q);
                    pcfMsg.AddParameter(MQC.MQCA_Q_NAME, theQueue.Name);
                    pcfMsg.AddParameter(MQC.MQIA_Q_TYPE, theQueue.QueueTypeCore);
                    pcfMsg.AddParameter(MQC.MQIA_INHIBIT_PUT,
                        newStatus == GetPutStatus.Inhibited ? MQC.MQQA_PUT_INHIBITED : MQC.MQQA_PUT_ALLOWED);

                    if (_messageAgent.TrySend(pcfMsg))
                        return;

                    pcfError = _messageAgent.LastError;
                }

                // fallback option

                try
                {
                    MQQueue mqQ = OpenQueueCore(theQueue.Name, OpenQueueMode.ForSet);
                    mqQ.InhibitPut = newStatus == GetPutStatus.Inhibited ? MQC.MQQA_PUT_INHIBITED : MQC.MQQA_PUT_ALLOWED;
                    mqQ.Close();
                }
                catch (MQException ex)
                {
                    if (pcfError != null)
                        ex.Data.Add("PCFError", pcfError.ToString());
                    throw;
                }
            });
        }

        #endregion

        public void Dispose()
        {
            _messageAgent.Dispose();
        }


        public IObjectProvider NewObjectProvider(IObjectNameFilter filter)
        {
            return new WsObjectProvider(this, filter);
        }

        internal void RefreshChannelInfosCore(WsChannel wsChannel)
        {

            wsChannel.Type = WsChannelType.Unknown;
            wsChannel.ConnectionName = null;
            wsChannel.TransmissionQueue = null;
            wsChannel.Status = null;
            wsChannel.IndoubtStatus = null;

            RetryOnHCONNError(() =>
            {
                if (WsSoftwareInfo.AvoidPCFWhenPossible == false)
                {
                    var msg = _messageAgent.NewRequest(MQC.MQCMD_INQUIRE_CHANNEL);
                    msg.AddParameter(MQC.MQCACH_CHANNEL_NAME, wsChannel.Name);
                    msg.AddParameter(CMQCFC.MQIACF_CHANNEL_ATTRS, new[]
                    {
                    CMQCFC.MQCACH_CHANNEL_NAME,
                    MQC.MQIACH_CHANNEL_TYPE,
                    CMQCFC.MQCACH_CONNECTION_NAME,
                    CMQCFC.MQCACH_XMIT_Q_NAME
                });

                    PCF.PcfMessage[] response;
                    if (_messageAgent.TrySend(msg, out response))
                    {
                        bool readXmitQ = false;
                        bool readConnName = false;

                        var qType = response[0].GetIntParameterValue(MQC.MQIACH_CHANNEL_TYPE);
                        wsChannel.ChannelTypeCore = qType;
                        switch (qType)
                        {
                            case MQC.MQCHT_AMQP:
                                wsChannel.Type = WsChannelType.Amqp;
                                break;
                            case MQC.MQCHT_CLNTCONN:
                                wsChannel.Type = WsChannelType.ClientConnection;
                                break;
                            case MQC.MQCHT_CLUSRCVR:
                                wsChannel.Type = WsChannelType.ClusterReceiver;
                                readConnName = true;
                                break;
                            case MQC.MQCHT_CLUSSDR:
                                wsChannel.Type = WsChannelType.ClusterSender;
                                readConnName = true;
                                break;
                            case MQC.MQCHT_REQUESTER:
                                wsChannel.Type = WsChannelType.Requester;
                                readConnName = true;
                                break;
                            case MQC.MQCHT_RECEIVER:
                                wsChannel.Type = WsChannelType.Receiver;
                                break;
                            case MQC.MQCHT_SENDER:
                                wsChannel.Type = WsChannelType.Sender;
                                readConnName = true;
                                readXmitQ = true;
                                break;
                            case MQC.MQCHT_SERVER:
                                wsChannel.Type = WsChannelType.Server;
                                readConnName = true;
                                readXmitQ = true;
                                break;
                            case MQC.MQCHT_SVRCONN:
                                wsChannel.Type = WsChannelType.ServerConnection;
                                break;
                            default:
                                wsChannel.Type = WsChannelType.Unknown;
                                break;
                        }

                        if (readConnName)
                            wsChannel.ConnectionName = response[0].GetStringParameterValue(CMQCFC.MQCACH_CONNECTION_NAME);
                        if (readXmitQ)
                            wsChannel.TransmissionQueue = response[0].GetStringParameterValue(CMQCFC.MQCACH_XMIT_Q_NAME);
                    }
                    else
                    {
                        return;
                    }

                    if (wsChannel.Type != WsChannelType.Unknown)
                    {

                        var msg2 = _messageAgent.NewRequest(MQC.MQCMD_INQUIRE_CHANNEL_STATUS);
                        msg2.AddParameter(MQC.MQCACH_CHANNEL_NAME, wsChannel.Name);
                        msg2.AddParameter(CMQCFC.MQIACH_CHANNEL_INSTANCE_TYPE, MQC.MQOT_CURRENT_CHANNEL);
                        msg2.AddParameter(CMQCFC.MQIACH_CHANNEL_INSTANCE_ATTRS, new[]
                        {
                        CMQCFC.MQCACH_CHANNEL_NAME,
                        CMQCFC.MQIACH_MSGS,
                        CMQCFC.MQCACH_LAST_MSG_DATE,
                        CMQCFC.MQCACH_LAST_MSG_TIME,
                        CMQCFC.MQIACH_CHANNEL_STATUS,
                        CMQCFC.MQIACH_INDOUBT_STATUS
                    });

                        PCF.PcfMessage[] response2;
                        if (_messageAgent.TrySend(msg2, out response2))
                        {
                            int chlStatus;
                            if (response2[0].TryGetIntParameterValue(CMQCFC.MQIACH_CHANNEL_STATUS, out chlStatus))
                            {
                                switch (chlStatus)
                                {
                                    case MQC.MQCHS_BINDING:
                                        wsChannel.Status = ChannelStatus.Binding;
                                        break;
                                    case MQC.MQCHS_DISCONNECTED:
                                        wsChannel.Status = ChannelStatus.Disconnected;
                                        break;
                                    case MQC.MQCHS_INACTIVE:
                                        wsChannel.Status = ChannelStatus.Inactive;
                                        break;
                                    case MQC.MQCHS_INITIALIZING:
                                        wsChannel.Status = ChannelStatus.Initializing;
                                        break;
                                    case MQC.MQCHS_PAUSED:
                                        wsChannel.Status = ChannelStatus.Paused;
                                        break;
                                    case MQC.MQCHS_REQUESTING:
                                        wsChannel.Status = ChannelStatus.Requesting;
                                        break;
                                    case MQC.MQCHS_RETRYING:
                                        wsChannel.Status = ChannelStatus.Retrying;
                                        break;
                                    case MQC.MQCHS_RUNNING:
                                        wsChannel.Status = ChannelStatus.Running;
                                        break;
                                    case MQC.MQCHS_STARTING:
                                        wsChannel.Status = ChannelStatus.Starting;
                                        break;
                                    case MQC.MQCHS_STOPPED:
                                        wsChannel.Status = ChannelStatus.Stopped;
                                        break;
                                    case MQC.MQCHS_STOPPING:
                                        wsChannel.Status = ChannelStatus.Stopping;
                                        break;
                                    case MQC.MQCHS_SWITCHING:
                                        wsChannel.Status = ChannelStatus.Switching;
                                        break;
                                }
                            }

                            int indoubtStatus;
                            if (response2[0].TryGetIntParameterValue(CMQCFC.MQIACH_INDOUBT_STATUS, out indoubtStatus))
                            {
                                wsChannel.IndoubtStatus = indoubtStatus == 0 ? false : true;
                            }
                        }
                        else
                        {
                            wsChannel.Status = ChannelStatus.Inactive;
                        }
                    }
                }
            });
        }

        internal void StartChannelCore(WsChannel wsChannel)
        {
            RetryOnHCONNError(() =>
            {
                var request = _messageAgent.NewRequest(MQC.MQCMD_START_CHANNEL);
                request.AddParameter(MQC.MQCACH_CHANNEL_NAME, wsChannel.Name);
                _messageAgent.Send(request);
            });
        }


        internal void StopChannelCore(WsChannel wsChannel, ChannelStopMode action, bool setInactive)
        {
            RetryOnHCONNError(() =>
            {
                var request = _messageAgent.NewRequest(MQC.MQCMD_STOP_CHANNEL);
                request.AddParameter(MQC.MQCACH_CHANNEL_NAME, wsChannel.Name);
                int mode = MQC.MQMODE_QUIESCE;
                switch (action)
                {
                    case ChannelStopMode.Force:
                        mode = MQC.MQMODE_FORCE;
                        break;
                    case ChannelStopMode.Terminate:
                        mode = MQC.MQMODE_TERMINATE;
                        break;
                    default:
                        mode = MQC.MQMODE_QUIESCE;
                        break;
                }
                request.AddParameter(MQC.MQIACF_MODE, mode);
                if (setInactive)
                {
                    request.AddParameter(MQC.MQIACH_CHANNEL_STATUS, MQC.MQCHS_INACTIVE);
                }
                _messageAgent.Send(request);
            });
        }


        internal void ResetChannelCore(WsChannel wsChannel, int msgSequenceNumber)
        {
            RetryOnHCONNError(() =>
            {
                var request = _messageAgent.NewRequest(MQC.MQCMD_RESET_CHANNEL);
                request.AddParameter(MQC.MQCACH_CHANNEL_NAME, wsChannel.Name);
                request.AddParameter(MQC.MQIACH_MSG_SEQUENCE_NUMBER, msgSequenceNumber);
                _messageAgent.Send(request);
            });
        }

        internal void ResolveChannelCore(WsChannel wsChannel, bool commit)
        {
            RetryOnHCONNError(() =>
            {
                var request = _messageAgent.NewRequest(MQC.MQCMD_RESOLVE_CHANNEL);
                request.AddParameter(MQC.MQCACH_CHANNEL_NAME, wsChannel.Name);
                request.AddParameter(MQC.MQIACH_IN_DOUBT, commit ? MQC.MQIDO_COMMIT : MQC.MQIDO_BACKOUT);
                _messageAgent.Send(request);
            });
        }

        internal IEnumerable<string> GetListenerNames(WsObjectNameFilter filter)
        {
            try
            {
                return RetryOnHCONNError(() =>
                {
                    var pcfMsg = _messageAgent.NewRequest(MQC.MQCMD_INQUIRE_LISTENER);

                    if (filter != null)
                    {
                        pcfMsg.AddParameter(MQC.MQCACH_LISTENER_NAME,
                            string.IsNullOrEmpty(filter.NamePrefix[0]) ? "*" : string.Concat(filter.NamePrefix[0], "*"));
                    }
                    else
                    {
                        pcfMsg.AddParameter(MQC.MQCACH_LISTENER_NAME, "*");
                    }

                    var pcfResponses = _messageAgent.Send(pcfMsg);

                    var result = new List<string>();

                    foreach (var r in pcfResponses)
                    {
                        string value;
                        if (r.TryGetStringParameterValue(MQC.MQCACH_LISTENER_NAME, out value))
                        {
                            result.Add(value);
                        }
                    }

                    return result.Select(x => x.Trim()).Where(n => filter == null || filter.IsMatch(n)).ToList();
                });
            }
            catch (MQException ex)
            {
                if (ex.ReasonCode == MQC.MQRCCF_CFH_COMMAND_ERROR)
                {
                    return new string[0];
                }
                throw;
            }

        }

        internal IListener OpenListener(string name, bool autoLoadInfo = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            try
            {
                var l = new WsListener(this, name);
                if (autoLoadInfo)
                    l.RefreshInfo();

                return l;
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }


        internal void RefreshListenerInfosCore(WsListener wsListener)
        {
            wsListener.Ip = null;
            wsListener.Port = null;
            wsListener.Protocol = null;
            wsListener.Status = null;

            RetryOnHCONNError(() =>
            {

                var pcfMsg = _messageAgent.NewRequest(MQC.MQCMD_INQUIRE_LISTENER);

                pcfMsg.AddParameter(MQC.MQCACH_LISTENER_NAME, wsListener.Name);

                var pcfResponses = _messageAgent.Send(pcfMsg);

                var protocol = pcfResponses[0].GetIntParameterValue(MQC.MQIACH_XMIT_PROTOCOL_TYPE);

                switch (protocol)
                {
                    case MQC.MQXPT_TCP:
                        wsListener.Protocol = "TCP";
                        break;
                    case MQC.MQXPT_LU62:
                        wsListener.Protocol = "LU62";
                        break;
                    case MQC.MQXPT_NETBIOS:
                        wsListener.Protocol = "NETBIOS";
                        break;
                    case MQC.MQXPT_SPX:
                        wsListener.Protocol = "SPX";
                        break;
                }

                if (protocol == MQC.MQXPT_TCP)
                {
                    var ip = pcfResponses[0].GetStringParameterValue(MQC.MQCACH_IP_ADDRESS);
                    wsListener.Ip = ip;
                    var port = pcfResponses[0].GetIntParameterValue(MQC.MQIACH_PORT);
                    if (port > 0)
                    {
                        wsListener.Port = port;
                    }
                }

                var pcfMsg2 = _messageAgent.NewRequest(MQC.MQCMD_INQUIRE_LISTENER_STATUS);
                pcfMsg2.AddParameter(MQC.MQCACH_LISTENER_NAME, wsListener.Name);
                PCF.PcfMessage[] statusMsg;
                if (_messageAgent.TrySend(pcfMsg2, out statusMsg))
                {
                    var status = statusMsg[0].GetIntParameterValue(MQC.MQIACH_LISTENER_STATUS);

                    switch (status)
                    {
                        case MQC.MQSVC_STATUS_STARTING:
                            wsListener.Status = ListenerStatus.Starting;
                            break;
                        case MQC.MQSVC_STATUS_RUNNING:
                            wsListener.Status = ListenerStatus.Running;
                            break;
                        case MQC.MQSVC_STATUS_STOPPING:
                            wsListener.Status = ListenerStatus.Stopping;
                            break;
                        case MQC.MQSVC_STATUS_STOPPED:
                            wsListener.Status = ListenerStatus.Stopped;
                            break;
                        case MQC.MQSVC_STATUS_RETRYING:
                            wsListener.Status = ListenerStatus.Retrying;
                            break;
                    }
                }
                else
                {
                    wsListener.Status = ListenerStatus.Stopped;
                }
            });

        }

        internal void StartListenerCore(WsListener wsListener)
        {
            RetryOnHCONNError(() =>
            {
                var pcfMsg = _messageAgent.NewRequest(MQC.MQCMD_START_CHANNEL_LISTENER);
                pcfMsg.AddParameter(MQC.MQCACH_LISTENER_NAME, wsListener.Name);
                var pcfResponses = _messageAgent.Send(pcfMsg);
            });

        }

        internal void StopListenerCore(WsListener wsListener)
        {
            RetryOnHCONNError(() =>
            {
                var pcfMsg = _messageAgent.NewRequest(MQC.MQCMD_STOP_CHANNEL_LISTENER);
                pcfMsg.AddParameter(MQC.MQCACH_LISTENER_NAME, wsListener.Name);
                var pcfResponses = _messageAgent.Send(pcfMsg);
            });

        }

        internal WsQueueManager Clone()
        {
            return WsQueueManagerFactory.Clone(this);
        }
    }
}
