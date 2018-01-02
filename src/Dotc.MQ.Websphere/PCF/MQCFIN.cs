#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Globalization;
using IBM.WMQ;

namespace Dotc.MQ.Websphere.PCF
{
    internal class Mqcfin : PcfParameter
    {
        private int _value;

        public Mqcfin()
        {
            Type = Cmqcfc.MqcftInteger;
            StrucLength = Cmqcfc.MqcfinStrucLength;
            Parameter = 0;
            _value = 0;
        }

        public Mqcfin(MQMessage message)
        {
            Initialize(message);
        }

        public Mqcfin(int param, int val)
        {
            Type = Cmqcfc.MqcftInteger;
            StrucLength = Cmqcfc.MqcfinStrucLength;
            Parameter = param;
            _value = val;
        }

        public override object GetValue()
        {
            return _value;
        }

        internal override sealed void Initialize(MQMessage message)
        {
            Type = message.ReadInt4();
            StrucLength = message.ReadInt4();
            Parameter = message.ReadInt4();
            _value = message.ReadInt4();
        }

        public void SetValue(int val)
        {
            _value = val;
        }

        public override void SetValue(object val)
        {
            _value = (int) val;
        }

        public override int Write(MQMessage message)
        {
            return Write(message, Parameter, _value);
        }

        public static int Write(MQMessage message, int parameter, int val)
        {
            message.WriteInt4(Cmqcfc.MqcftInteger);
            message.WriteInt4(Cmqcfc.MqcfinStrucLength);
            message.WriteInt4(parameter);
            message.WriteInt4(val);
            return 0x10;
        }

        public override string GetStringValue()
        {
            return StringValue;
        }

        public string StringValue => _value.ToString(CultureInfo.InvariantCulture);
    }
}
