#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Runtime.Serialization;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    [Serializable]
    public class ParserException : MQExplorerPlusException
    {

        public ParsingResultNode Node { get; private set; }

        public ParserException()
        {
        }

        public ParserException(string message)
            : base(message)
        {
        }

        public ParserException(string message, ParsingResultNode node)
            : this(message)
        {
            Node = node;
        }

        public ParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ParserException(SerializationInfo si, StreamingContext context) : base(si, context)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
