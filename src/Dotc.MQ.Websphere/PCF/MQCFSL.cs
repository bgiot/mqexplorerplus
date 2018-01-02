#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Text;
using IBM.WMQ;
using System.Globalization;

namespace Dotc.MQ.Websphere.PCF
{
    internal class Mqcfsl : PcfParameter
    {
        public  int CodedCharSetId { get; private set; }
        private int _count;
        private int _stringLength;
        private string[] _strings;

        public Mqcfsl()
        {
            Type = Cmqcfc.MqcftStringList;
            StrucLength = Cmqcfc.MqcfslStrucLengthFixed;
            Parameter = 0;
            CodedCharSetId = 0;
            _count = 0;
            _stringLength = 0;
            _strings = null;
        }

        public Mqcfsl(MQMessage message)
        {
            Initialize(message);
        }

        public Mqcfsl(int param, string[] strs)
        {
            _count = strs.Length;
            if (_count > 0)
            {
                _stringLength = strs[0].Length;
            }
            StrucLength = Cmqcfc.MqcfslStrucLengthFixed;
            StrucLength += _count * _stringLength;
            StrucLength += (4 - (StrucLength % 4)) % 4;
            _strings = (string[]) strs.Clone();
            Parameter = param;
        }

        public override object GetValue()
        {
            return _strings;
        }

        internal override sealed void Initialize(MQMessage message)
        {
            Type = message.ReadInt4();
            StrucLength = message.ReadInt4();
            Parameter = message.ReadInt4();
            CodedCharSetId = message.ReadInt4();
            _count = message.ReadInt4();
            _stringLength = message.ReadInt4();
            _strings = new string[_count];
            for (var i = 0; i < _count; i++)
            {
                _strings[i] = message.ReadString(_stringLength); // Encoding.ASCII.GetString(message.ReadBytes(_stringLength), 0, _stringLength);
            }
            var padding = (4 - (StrucLength % 4)) % 4;
            message.SkipBytes(padding);
        }

        public void SetStrings(string[] strs)
        {
            if (strs == null) return;
            _strings = (string[]) strs.Clone();
            _count = strs.Length;
            _stringLength = strs[0].Length;
            StrucLength = Cmqcfc.MqcfslStrucLengthFixed;
            StrucLength += _count * _stringLength;
            StrucLength += (4 - (StrucLength % 4)) % 4;
        }

        public override void SetValue(object val)
        {
            if (val == null) return;
            _strings = (string[]) ((string[]) val).Clone();
            _count = _strings.Length;
            _stringLength = _strings[0].Length;
            StrucLength = Cmqcfc.MqcfslStrucLengthFixed;
            StrucLength += _count * _stringLength;
            StrucLength += (4 - (StrucLength % 4)) % 4;
        }

        public override int Write(MQMessage message)
        {
            return Write(message, Parameter, _strings);
        }

        public static int Write(MQMessage message, int param, string[] strs)
        {
            if (strs == null)
            {
                return 0;
            }
            var padding = (Cmqcfc.MqcfslStrucLengthFixed + (strs.Length * strs[0].Length)) % 4;
            message.WriteInt4(Cmqcfc.MqcftStringList);
            message.WriteInt4((Cmqcfc.MqcfslStrucLengthFixed + (strs.Length * strs[0].Length)) + padding);
            message.WriteInt4(param);
            message.WriteInt4(0);
            message.WriteInt4(strs.Length);
            message.WriteInt4(strs[0].Length);
            foreach (var c in strs)
            {
                var bytes = new byte[strs[0].Length];
                Encoding.ASCII.GetBytes(c, 0, strs[0].Length, bytes, 0);
                message.Write(bytes);
            }
            message.Write(new byte[padding]);
            return ((Cmqcfc.MqcfslStrucLengthFixed + (strs.Length * strs[0].Length)) + padding);
        }

        public override string GetStringValue()
        {
            var str = "";
            for (var i = 0; i < _strings.Length; i++)
            {
                if (i > 0)
                {
                    str = str + "\n";
                }
                str = str + _strings[i].ToString(CultureInfo.InvariantCulture);
            }
            return str;
        }

        public object Value
        {
            get
            {
                return _strings;
            }
            set
            {
                _strings = (string[]) value;
            }
        }
    }
}
