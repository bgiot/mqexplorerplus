#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using IBM.WMQ;

namespace Dotc.MQ.Websphere.PCF
{
    internal abstract class PcfHeader
    {
        protected static int Cmdtype = 1;

        protected int StrucLength { get; set; }

        internal abstract void Initialize(MQMessage message);
        public abstract int Write(MQMessage message);

        public int Parameter { get; protected set; }

        public int Size => StrucLength;

        public int Type { get; set; }
    }
}
