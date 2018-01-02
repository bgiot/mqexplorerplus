#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Runtime.Serialization;

namespace Dotc.MQExplorerPlus.Core
{
    [Serializable]
    public class MQExplorerPlusException : Exception
    {

        public MQExplorerPlusException()
        { }

        public MQExplorerPlusException(string message) : base(message)
        { }

        public MQExplorerPlusException(string message, Exception innerException) : base(message, innerException)
        { }

        protected MQExplorerPlusException(SerializationInfo si, StreamingContext context) : base(si, context)
        { }
    }
}
