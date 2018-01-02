#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Runtime.Serialization;

namespace Dotc.MQ
{
    [Serializable]
    public class MqException : Exception
    {

        public MqException()
        { }

        public MqException(string message) : base(message)
        { }

        public MqException(string message, Exception innerException) : base (message, innerException)
        { }

        public MqException(string message, int reasonCode, Exception innerException) : base(message, innerException)
        {
            ReasonCode = reasonCode;
        }

        public int ReasonCode { get; }

        protected MqException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ReasonCode = info.GetInt32("ReasonCode");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ReasonCode", ReasonCode);
        }
    }
}
