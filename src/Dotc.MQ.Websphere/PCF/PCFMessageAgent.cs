#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using IBM.WMQ;

namespace Dotc.MQ.Websphere.PCF
{
    internal class PcfMessageAgent : PcfAgent
    {

        public PcfMessageAgent()
        {
        }

        public PcfMessageAgent(MQQueueManager qManager) : base(qManager)
        {
        }

        public PcfMessageAgent(string qManagerName) : base(qManagerName)
        {
        }

        public PcfMessage[] Send(PcfMessage request)
        {
            var messageArray = base.Send(request.GetCommand(), request.GetParameters());
            var messageArray2 = new PcfMessage[messageArray.Length];
            for (var i = 0; i < messageArray2.Length; i++)
            {
                messageArray2[i] = new PcfMessage(messageArray[i]);
            }
            return messageArray2;
        }

    }
}
