﻿#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using Dotc.Common;
using IBM.WMQ;
using static System.FormattableString;
using System.Threading;
using System.Collections;

namespace Dotc.MQ.Websphere
{

    internal sealed class WsQueue : ObservableObject, IQueue
    {

        internal WsQueue(WsQueueManager qmOwner, string queueName)
        {
            Debug.Assert(queueName != null);
            Debug.Assert(qmOwner != null);

            QueueManager = qmOwner;
            Name = queueName.Trim();
            ExtendedProperties = new ExpandoObject();
            DumpEngine = new WsDump2(this);
            IsSystemQueue = WsSystemObjectNameFilter.IsSystemQueue(queueName);
        }

        internal int QueueTypeCore { get; set; }
        internal int DefaultPriority { get; set; }

        public bool IsSystemQueue { get; }

        internal MQQueue OpenQueueCore(OpenQueueMode openMode)
        {
            return ((WsQueueManager)QueueManager).OpenQueueCore(Name, openMode);
        }

        private int? _depth;
        public int? Depth
        {
            get { return _depth; }
            internal set
            {
                SetPropertyAndNotify(ref _depth, value);
            }
        }

        private GetPutStatus? _getStatus;
        public GetPutStatus? GetStatus
        {
            get { return _getStatus; }
            internal set
            {
                SetPropertyAndNotify(ref _getStatus, value);
            }
        }

        private GetPutStatus? _putStatus;
        public GetPutStatus? PutStatus
        {
            get { return _putStatus; }
            internal set
            {
                SetPropertyAndNotify(ref _putStatus, value);
            }
        }


        public string Name { get; }

        public IQueueManager QueueManager { get; private set; }

        private int _type;
        public int Type
        {
            get { return _type; }
            internal set { SetPropertyAndNotify(ref _type, value); }
        }


        private void AddExtraInfoToError(IDictionary data)
        {
            data.AddOrReplace("Queue", this.Name);
            data.AddOrReplace("Connection", this.QueueManager.ConnectionInfo);
        }

        public void ClearQueue(bool truncateMode)
        {

            try
            {
                if (truncateMode)
                {
                    TruncateQueue();
                }
                else
                {
                    var ibmQueue = OpenQueueCore(OpenQueueMode.ForRead);

                    try
                    {
                        var mqGetMsgOpts = new MQGetMessageOptions { Options = MQC.MQGMO_FAIL_IF_QUIESCING };

                        while (true)
                        {
                            try
                            {
                                var msg = new MQMessage();
                                ibmQueue.Get(msg, mqGetMsgOpts);
                            }
                            catch (MQException ex)
                            {
                                if (ex.ReasonCode == 2033 /* MQRC_NO_MSG_AVAILABLE */)
                                {
                                    break;
                                }
                                throw;
                            }
                        }
                    }
                    finally
                    {
                        ibmQueue.Close();
                        RefreshInfo();
                    }
                }

            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        private void TruncateQueue()
        {
            (QueueManager as WsQueueManager)?.TruncateQueueCore(this);

            RefreshInfo();
        }

        public bool SupportTruncate => Type == WsQueueType.Local || Type == WsQueueType.Transmission;

        internal IEnumerator<MQMessage> DumpAllMessagesCore(bool leaveMessages)
        {
            var ibmQueue = OpenQueueCore(leaveMessages ? OpenQueueMode.ForBrowse : OpenQueueMode.ForRead);
            var mqGetMsgOpts = new MQGetMessageOptions();
            if (leaveMessages)
                mqGetMsgOpts.Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_BROWSE_FIRST;
            else
                mqGetMsgOpts.Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_SYNCPOINT;

            while (true)
            {
                var msg = new MQMessage();
                try
                {
                    ibmQueue.Get(msg, mqGetMsgOpts);
                    if (leaveMessages)
                    {
                        mqGetMsgOpts.Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_BROWSE_NEXT;
                    }


                }
                catch (MQException ex)
                {
                    if (ex.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE)
                    {
                        break;
                    }
                    ibmQueue.Close();
                    throw;
                }

                yield return msg;

            }
            if (!leaveMessages)
                QueueManager.Commit();

            ibmQueue.Close();
        }


        public IEnumerable<IMessage> GetMessages(int numberOfMessages, CancellationToken ct, IProgress<int> progress = null)
        {
            MQQueue ibmQueue;
            try
            {
                ibmQueue = OpenQueueCore(OpenQueueMode.ForRead);
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }

            var mqGetMsgOpts = new MQGetMessageOptions { Options = MQC.MQGMO_FAIL_IF_QUIESCING };

            int count = 0;

            while (numberOfMessages > 0)
            {
                if (ct != CancellationToken.None && ct.IsCancellationRequested)
                    break;

                var msg = new MQMessage();
                try
                {
                    ibmQueue.Get(msg, mqGetMsgOpts);
                    count++;
                    progress?.Report(count);
                }
                catch (MQException ex)
                {
                    if (ex.ReasonCode == 2033 /* MQRC_NO_MSG_AVAILABLE */)
                    {
                        break;
                    }
                    throw ex.ToMqException(AddExtraInfoToError);
                }

                yield return new WsMessage(msg, this);
                numberOfMessages--;

            }

            RefreshInfo();
        }


        public IEnumerable<IMessage> BrowseMessages(int numberOfMessages, CancellationToken ct, byte[] startingPointMessageId = null, IBrowseFilter filter = null, IProgress<int> progress = null)
        {

            MQQueue ibmQueue;

            var browseOption = MQC.MQGMO_BROWSE_FIRST;

            try
            {
                ibmQueue = OpenQueueCore(OpenQueueMode.ForBrowse);

                if (startingPointMessageId != null)
                {
                    if (SetBrowseCursorAtMessageId(ibmQueue, startingPointMessageId))
                    {
                        browseOption = MQC.MQGMO_BROWSE_NEXT;
                    }
                }
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }

            int count = 0;

            var getMsgOpts = new MQGetMessageOptions()
            {
                Options = MQC.MQGMO_FAIL_IF_QUIESCING | browseOption
            };

            try
            {

                while (!ct.IsCancellationRequested)
                {
                    var msg = new MQMessage();
                    try
                    {
                        ibmQueue.Get(msg, getMsgOpts);
                    }
                    catch (MQException ibmEx)
                    {
                        if (ibmEx.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE)
                        {
                            break;
                        }

                        throw ibmEx.ToMqException(AddExtraInfoToError);
                    }
                    count++;
                    var localMsg = new WsMessage(msg, this);
                    if (filter == null || filter.IsMatch(localMsg))
                    {

                        progress?.Report(count);

                        localMsg.Index = count;
                        yield return localMsg;

                    }

                    numberOfMessages--;
                    if (numberOfMessages == 0)
                    {
                        break;
                    }

                    getMsgOpts.Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_BROWSE_NEXT;

                }
            }
            finally
            {
                ibmQueue?.Close();
            }
        }

        private bool SetBrowseCursorAtMessageId(MQQueue queue, byte[] msgId)
        {

            var getMsgOpts = new MQGetMessageOptions()
            {
                Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_BROWSE_FIRST
            };

            try
            {
                getMsgOpts.MatchOptions = MQC.MQMO_MATCH_MSG_ID;
                var msg = new MQMessage
                {
                    MessageId = msgId
                };
                queue.Get(msg, getMsgOpts);
                return true;

            }
            catch (MQException ex)
            {
                if (ex.ReasonCode != MQC.MQRC_NO_MSG_AVAILABLE)
                    throw;
                return false;
            }
        }

        public void RefreshInfo()
        {
            try
            {
                ((WsQueueManager)QueueManager).RefreshQueueInfosCore(this);
            }
            catch (MQException ibmEx)
            {
                if (ibmEx.ReasonCode == MQC.MQRC_UNKNOWN_OBJECT_NAME)
                    return;

                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void PutMessages(IList<IMessage> messages, CancellationToken ct, IProgress<int> progress = null)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            try
            {

                var ibmQueue = OpenQueueCore(OpenQueueMode.ForWrite);

                try
                {

                    var mqPutMsgOpts = new MQPutMessageOptions { Options = MQC.MQPMO_FAIL_IF_QUIESCING };

                    int count = 0;

                    foreach (var msg in messages)
                    {
                        if (progress != null && ct.IsCancellationRequested)
                            break;

                        ibmQueue.Put(((WsMessage)msg).IbmMessage, mqPutMsgOpts);
                        count++;
                        progress?.Report(count);
                    }

                }
                finally
                {
                    ibmQueue.Close();
                    RefreshInfo();
                }

            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void SetGetStatus(GetPutStatus newStatus)
        {

            try
            {
                (QueueManager as WsQueueManager)?.SetQueueGetInhibitCore(this, newStatus);
                RefreshInfo();
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void SetPutStatus(GetPutStatus newStatus)
        {

            try
            {
                (QueueManager as WsQueueManager)?.SetQueuePutInhibitCore(this, newStatus);
                RefreshInfo();
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public void ForwardMessages(IList<IMessage> messagesToForward, IQueue destinationQueue, CancellationToken ct, IProgress<int> progress = null)
        {
            if (messagesToForward == null) throw new ArgumentNullException(nameof(messagesToForward));
            if (destinationQueue == null) throw new ArgumentNullException(nameof(destinationQueue));

            try
            {
                var ibmQueue = OpenQueueCore(OpenQueueMode.ForRead);
                var destinationQ = (WsQueue)destinationQueue;
                var ibmQueueDest = destinationQ.OpenQueueCore(OpenQueueMode.ForWrite);

                var mqGetMsgOpts = new MQGetMessageOptions
                {
                    Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_SYNCPOINT,
                    MatchOptions = MQC.MQMO_MATCH_MSG_ID
                };

                var mqPutMsgOpts = new MQPutMessageOptions { Options = MQC.MQPMO_FAIL_IF_QUIESCING | MQC.MQPMO_SYNCPOINT };


                int count = 0;

                try
                {
                    foreach (var message in messagesToForward)
                    {
                        var retrievedMessage = new MQMessage { MessageId = message.MessageId };
                        try
                        {
                            if (progress != null && ct.IsCancellationRequested)
                                break;


                            ibmQueue.Get(retrievedMessage, mqGetMsgOpts);
                            ibmQueueDest.Put(retrievedMessage, mqPutMsgOpts);
                            count++;
                            progress?.Report(count);
                        }
                        catch (MQException ex)
                        {
                            if (ex.ReasonCode == 2033 /* MQRC_NO_MSG_AVAILABLE */)
                            {
                                continue;
                            }
                            throw;
                        }
                    }
                    if (QueueManager != destinationQ.QueueManager)
                    {
                        destinationQ.QueueManager.Commit();
                    }
                    QueueManager.Commit();

                }
                catch (Exception)
                {
                    if (QueueManager != destinationQ.QueueManager)
                    {
                        destinationQ.QueueManager.Rollback();
                    }
                    QueueManager.Rollback();
                    throw;
                }
                finally
                {
                    ibmQueue.Close();
                    ibmQueueDest.Close();
                    RefreshInfo();
                    destinationQ.RefreshInfo();
                }
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public IMessage NewMessage(string content, int? priority, int? characterSet, Dictionary<string, object> extendedProperties = null)
        {
            var ibmMsg = new MQMessage
            {
                Priority = priority ?? DefaultPriority,
                CharacterSet = characterSet ?? QueueManager.DefaultCharacterSet,
                Format = MQC.MQFMT_STRING
            };
            if (extendedProperties != null)
            {
                if (extendedProperties.ContainsKey("CorrelationId"))
                {
                    if (extendedProperties["CorrelationId"] is byte[] corrId && corrId.Length == 24)
                    {
                        // Fine we got a valid correlationid; set it
                        ibmMsg.CorrelationId = corrId;
                    }

                }
                if (extendedProperties.ContainsKey("GroupId"))
                {
                    if (extendedProperties["GroupId"] is byte[] grpId && grpId.Length == 24)
                    {
                        // Fine we got a valid groupid; set it
                        ibmMsg.GroupId = grpId;
                        if (extendedProperties.ContainsKey("LogicalSequenceNumber") && extendedProperties["LogicalSequenceNumber"] is int lsn)
                        {
                            ibmMsg.MessageSequenceNumber = lsn;
                        }

                    }
                }
            }
            ibmMsg.WriteString(content);
            return new WsMessage(ibmMsg);
        }

        public string UniqueId => Invariant($"{Name}@{QueueManager.UniqueId}");

        private ExpandoObject _extendedProperties;
        public dynamic ExtendedProperties
        {
            get
            {
                return _extendedProperties;
            }
            private set
            {
                _extendedProperties = value;
            }
        }

        public IDump DumpEngine { get; }

        public void DeleteMessages(IList<IMessage> messages, CancellationToken ct, IProgress<int> progress = null)
        {

            if (messages == null) throw new ArgumentNullException(nameof(messages));

            try
            {
                var ibmQueue = OpenQueueCore(OpenQueueMode.ForRead);

                try
                {
                    var mqGetMsgOpts = new MQGetMessageOptions
                    {
                        Options = MQC.MQGMO_FAIL_IF_QUIESCING,
                        MatchOptions = MQC.MQMO_MATCH_MSG_ID
                    };

                    int count = 0;
                    foreach (var m in messages)
                    {
                        if (progress != null && ct.IsCancellationRequested)
                            break;

                        try
                        {
                            var msg = ((WsMessage)m).IbmMessage;
                            ibmQueue.Get(msg, mqGetMsgOpts);

                            count++;
                            progress?.Report(count);

                        }
                        catch (MQException ex)
                        {
                            if (ex.ReasonCode == 2033 /* MQRC_NO_MSG_AVAILABLE */)
                            {
                            }
                            else throw;
                        }
                    }
                }
                finally
                {
                    ibmQueue.Close();
                }


            }

            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public IQueue NewConnection()
        {
            return NewConnectionCore();
        }

        internal WsQueue NewConnectionCore()
        {
            var qm = WsQueueManagerFactory.Clone((WsQueueManager)QueueManager);
            return new WsQueue(qm, this.Name);
        }
    }
}
