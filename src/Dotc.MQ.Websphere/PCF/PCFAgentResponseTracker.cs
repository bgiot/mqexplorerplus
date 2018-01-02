#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQ.Websphere.PCF
{
    internal abstract class PCFAgentResponseTracker
    {
        internal abstract bool IsLast(MQMessage message);
    }

    internal class PCFAgentResponseTrackerNon390 : PCFAgentResponseTracker
    {

        private Mqcfh cfh = new Mqcfh();
        internal override bool IsLast(MQMessage message)
        {
            message.SkipBytes(20);
            var end = message.ReadInt4() != 0;
            var compCode = message.ReadInt4();
            var reason = message.ReadInt4();
            message.Seek(0);
            if (compCode != 0)
            {
                throw new PcfException(compCode, reason);
            }
            return end;
        }
    }

    internal class PCFAgentResponseTracker390 : PCFAgentResponseTracker
    {
        private Mqcfh cfh = new Mqcfh();
        private HashSet<string> set = new HashSet<string>();

        internal override bool IsLast(MQMessage message)
        {
            cfh.Initialize(message);

            String current = null;
            int count = this.cfh.ParameterCount;
            while (count > 0)
            {
                PcfParameter p = PcfParameter.NextParameter(message);
                int id = p.Parameter;
                if (id == MQC.MQBACF_RESPONSE_SET)
                {
                    this.set.Add(p.GetStringValue());
                }
                else if (id == MQC.MQBACF_RESPONSE_ID)
                {
                    current = p.GetStringValue();
                    this.set.Add(current);
                }
                count--;
            }

            message.Seek(0);

            if ((this.cfh.Control == 1) && (current != null))
            {
                this.set.Remove(current);
            }
            return (this.set.Count == 0);
        }
    }
}
