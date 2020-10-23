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
    public class DumpException : MqException
    {

        public DumpException()
        { }

        public DumpException(string message) : base(message)
        { }

        public DumpException(string message, Exception innerException) : base (message, innerException)
        { }

        public DumpException(string message, int reasonCode, Exception innerException) : base(message, reasonCode, innerException)
        {
        }

        protected DumpException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
