#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Diagnostics;
using System.Dynamic;
using Dotc.Common;
using IBM.WMQ;
using System.Collections;

namespace Dotc.MQ.Websphere
{
    internal sealed class WsMessage : ObservableObject, IMessage
    {


        internal MQMessage IbmMessage { get; }

        internal WsMessage(MQMessage ibmM)
        {
            Debug.Assert(ibmM != null);
            IbmMessage = ibmM;
            LoadExtendedProperties();
        }
        internal WsMessage(MQMessage ibmM, WsQueue queueOwner) : this(ibmM)
        {
            Debug.Assert(queueOwner != null);
            Queue = queueOwner;
        }

        private void LoadExtendedProperties()
        {
            ExtendedProperties = new ExpandoObject();
            ExtendedProperties.Format = IbmMessage.Format;
            ExtendedProperties.Priority = IbmMessage.Priority;
            ExtendedProperties.CharacterSet = IbmMessage.CharacterSet;
            ExtendedProperties.Encoding = IbmMessage.Encoding;
            ExtendedProperties.GroupId = IbmMessage.GroupId;
            ExtendedProperties.LogicalSequenceNubmer = IbmMessage.MessageSequenceNumber;
            ExtendedProperties.CorrelationId = IbmMessage.CorrelationId;

        }

        public IQueue Queue { get; }

        private void AddExtraInfoToError(IDictionary data)
        {
            data.AddOrReplace("Queue", this.Queue.Name);
            data.AddOrReplace("Connection", this.Queue.QueueManager.ConnectionInfo);
        }

        public int? Index { get; internal set; }

        public byte[] MessageId
        {
            get
            {
                try
                {
                    return IbmMessage.MessageId;
                }
                catch (MQException ibmEx)
                {
                    throw ibmEx.ToMqException(AddExtraInfoToError);
                }
            }
        }

        public byte[] Bytes
        {
            get
            {
                try
                {
                    return IbmMessage.ReadBytesEx();
                }
                catch (MQException ibmEx)
                {
                    throw ibmEx.ToMqException(AddExtraInfoToError);
                }
            }
        }

        private string _text;
        private bool _textLoaded;

        public string Text
        {
            get
            {
                if (_textLoaded)
                {
                    return _text;
                }

                try
                {

                    if (IbmMessage.Format != MQC.MQFMT_STRING)
                    {
                        _text = null;
                    }
                    else
                    {
                        _text = IbmMessage.ReadStringEx();
                    }
                    _textLoaded = true;
                    return _text;
                }
                catch (MQException ibmEx)
                {
                    throw ibmEx.ToMqException(AddExtraInfoToError);
                }
            }
        }

        public DateTime PutTimestamp
        {
            get
            {
                try
                {
                    return IbmMessage.PutDateTime.ToLocalTime();

                }
                catch (MQException ibmEx)
                {
                    throw ibmEx.ToMqException(AddExtraInfoToError);
                }
            }
        }

        public int Length
        {
            get
            {
                try
                {
                    return IbmMessage.MessageLength;

                }
                catch (MQException ibmEx)
                {
                    throw ibmEx.ToMqException(AddExtraInfoToError);
                }
            }
        }


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

    }
}
