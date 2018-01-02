#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using IBM.WMQ;

namespace Dotc.MQ.Websphere.PCF
{
    internal abstract class PcfParameter : PcfHeader
    {
        public static PcfParameter NextParameter(MQMessage message)
        {
            var num = message.ReadInt4();
            message.DataOffset -= 4;
            switch (num)
            {
                case Cmqcfc.MqcftInteger:
                    return new Mqcfin(message);

                case Cmqcfc.MqcftString:
                    return new Mqcfst(message);

                case Cmqcfc.MqcftIntegerList:
                    return new Mqcfil(message);

                case Cmqcfc.MqcftStringList:
                    return new Mqcfsl(message);

                case Cmqcfc.MqcftByteString:
                    return new Mqcfbs(message);
            }
            throw new ArgumentException("Unknown type");
        }

        public abstract object GetValue();

        public abstract String GetStringValue();
        public abstract void SetValue(object value);
    }
}
