#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using IBM.WMQ;
using System.Collections.Generic;

namespace Dotc.MQ.Websphere.PCF
{
    internal class PcfAgent : IDisposable
    {
        private MQQueueManager _qMgr;
        private readonly bool _tempQueueManager;
        private int _waitInterval;

        protected PcfAgent()
        {
            _waitInterval = 30 * 1000;
            _tempQueueManager = false;
        }

        protected PcfAgent(MQQueueManager qMgr) : this()
        {
            _qMgr = qMgr;
        }

        protected PcfAgent(string qManagerName) : this()
        {
            _qMgr = new MQQueueManager(qManagerName);
            _tempQueueManager = true;
        }

        public void Disconnect()
        {
            if (_tempQueueManager)
            {
                _qMgr.Disconnect();
            }
        }

        protected MQMessage[] Send(int command, PcfParameter[] parameters)
        {
            if (parameters == null)  throw new ArgumentNullException(nameof(parameters));

            var oo = MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_INQUIRE;
            var responseQueue = _qMgr.AccessQueue("SYSTEM.DEFAULT.MODEL.QUEUE", oo);

            var cmdMessage = new MQMessage {
                ReplyToQueueName = responseQueue.Name,
                MessageType = MQC.MQMT_REQUEST,
                Feedback = MQC.MQFB_NONE,
                Format = MQC.MQFMT_ADMIN,
                Report = MQC.MQRO_NONE
            };

            PCFAgentResponseTracker tracker;
            if (_qMgr.Platform == MQC.MQPL_ZOS)
            {
                tracker = new PCFAgentResponseTracker390();
                Mqcfh.Write(cmdMessage, command, parameters.Length, Cmqcfc.MqcftCommandXr, Cmqcfc.MqcfhVersion3);
            }
            else
            {
                tracker = new PCFAgentResponseTrackerNon390();
                Mqcfh.Write(cmdMessage, command, parameters.Length, Cmqcfc.MqcftCommand, Cmqcfc.MqcfhVersion1);
            }

            foreach (var p in parameters)
            {
                p.Write(cmdMessage);
            }

            var aqo = MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_OUTPUT | MQC.MQOO_INQUIRE;
            var cmdQueue = _qMgr.AccessQueue(_qMgr.CommandInputQueueName, aqo);

            var pmo = new MQPutMessageOptions {
                Options = MQC.MQPMO_NEW_MSG_ID
            };
            cmdQueue.Put(cmdMessage, pmo);

            var gmo = new MQGetMessageOptions {
                Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_WAIT,
                WaitInterval = _waitInterval,
                MatchOptions = MQC.MQMO_MATCH_CORREL_ID
            };
            var list = new List<MQMessage>();
            while(true)
            {
                var response = new MQMessage {
                    CorrelationId = cmdMessage.MessageId
                };
                responseQueue.Get(response, gmo);

                list.Add(response);
                if (tracker.IsLast(response))
                {
                    break;
                }
            }
            cmdQueue.Close();
            responseQueue.Close();
            return list.ToArray();
        }

        public void SetWaitInterval(int seconds)
        {
            _waitInterval = seconds * 1000;
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                ((IDisposable) _qMgr)?.Dispose();
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
