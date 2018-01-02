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
    internal class Mqcfil : PcfParameter
    {
        private int _count;
        private int[] _values;

        public Mqcfil()
        {
            Type = Cmqcfc.MqcftIntegerList;
            StrucLength = Cmqcfc.MqcfilStrucLengthFixed;
            Parameter = 0;
        }

        public Mqcfil(MQMessage message)
        {
            Initialize(message);
        }

        public Mqcfil(int param, int[] vals)
        {
            Type = Cmqcfc.MqcftIntegerList;
            StrucLength = Cmqcfc.MqcfilStrucLengthFixed + (vals.Length * 4);
            StrucLength += (4 - (StrucLength % 4)) % 4;
            Parameter = param;
            _values = (int[]) vals.Clone();
        }

        public override object GetValue()
        {
            return _values;
        }

        internal override sealed void Initialize(MQMessage message)
        {
            Type = message.ReadInt4();
            StrucLength = message.ReadInt4();
            Parameter = message.ReadInt4();
            _count = message.ReadInt4();
            _values = new int[_count];
            for (var i = 0; i < _count; i++)
            {
                _values[i] = message.ReadInt4();
            }
            var n = (4 - (StrucLength % 4)) % 4;
            message.SkipBytes(n);
        }

        public override void SetValue(object val)
        {
            _values = (int[]) ((int[]) val).Clone();
            StrucLength = Cmqcfc.MqcfilStrucLengthFixed;
            StrucLength += _values.Length * 4;
            StrucLength += (4 - (StrucLength % 4)) % 4;
            _count = _values.Length;
        }

        public void SetValues(int[] vals)
        {
            StrucLength = Cmqcfc.MqcfilStrucLengthFixed + (vals.Length * 4);
            _count = vals.Length;
            _values = (int[]) vals.Clone();
        }

        public override int Write(MQMessage message)
        {
            return Write(message, Parameter, _values);
        }

        public static int Write(MQMessage message, int param, int[] vals)
        {
            var num = (0x10 + (vals.Length * 4)) % 4;
            message.WriteInt4(Cmqcfc.MqcftIntegerList);
            message.WriteInt4((Cmqcfc.MqcfilStrucLengthFixed + (vals.Length * 4)) + num);
            message.WriteInt4(param);
            message.WriteInt4(vals.Length);
            foreach (var v in vals)
            {
                message.WriteInt4(v);
            }
            message.Write(new byte[num]);
            return ((Cmqcfc.MqcfilStrucLengthFixed + (vals.Length * 4)) + num);
        }

        public override string GetStringValue()
        {
            return StringValue;
        }

        public string StringValue
        {
            get
            {
                var str = "";
                for (var i = 0; i < _values.Length; i++)
                {
                    if (i > 0)
                    {
                        str = str + " ";
                    }
                    str = str + _values[i].ToString(CultureInfo.InvariantCulture);
                }
                return str;
            }
        }
    }
}
