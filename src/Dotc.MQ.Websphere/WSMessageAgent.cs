#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using Dotc.MQ.Websphere.PCF;
using IBM.WMQ;

namespace Dotc.MQ.Websphere
{
    internal sealed class WsMessageAgent : IDisposable
    {

        private readonly PcfMessageAgent _agent;
        private readonly int _platform;
        private readonly object _syncLock = new object();

        internal WsMessageAgent(MQQueueManager queueManager)
        {
            if (queueManager == null) throw new ArgumentNullException(nameof(queueManager));

            _agent = new PcfMessageAgent(queueManager);
            _platform = queueManager.Platform;
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            _agent?.Dispose();
        }

        internal PcfMessage NewRequest(int command)
        {
            return _platform == MQC.MQPL_ZOS 
                ? new PcfMessage( 0x10 /*IBM.WMQ.PCF.CMQCFC.MQCFT_COMMAND_XR*/, command, 1, true) 
                : new PcfMessage(command);
        }

        internal PcfMessage[] Send(PcfMessage request)
        {
            lock(_syncLock)
            {
                return _agent.Send(request);
            }
        }

        private bool _SkipBecauseFailure;
        internal bool TrySend(PcfMessage request, out PcfMessage[] result, bool skipNextOnFailure = false)
        {
            LastError = null;
            if (_SkipBecauseFailure)
            {
                result = null;
                return false;
            }

            try
            {
                result = Send(request);
                return true;
            }
            catch (IBM.WMQ.MQException ex)
            {
                if (ex.ReasonCode == MQC.MQRC_HCONN_ERROR)
                    throw;

                LastError = ex;
                _SkipBecauseFailure = skipNextOnFailure;
                result = null;
                return false;
            }
        }

        internal Exception LastError { get; private set; }

        internal bool TrySend(PcfMessage request, bool skipNextOnFailure = false)
        {
            LastError = null;
            if (_SkipBecauseFailure) return false;

            try
            {
                Send(request);
                return true;
            }
            catch (IBM.WMQ.MQException ex)
            {
                if (ex.ReasonCode == MQC.MQRC_HCONN_ERROR)
                    throw;

                LastError = ex;
                _SkipBecauseFailure = skipNextOnFailure;
                return false;
            }
        }
    }
}
