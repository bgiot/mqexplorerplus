#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Text;
using IBM.WMQ;

namespace Dotc.MQ.Websphere.PCF
{
    internal class Mqcfst : PcfParameter
    {
        public  int CodedCharSetId { get; private set; }
        private int _stringLength;
        private string _stringval;

        public Mqcfst()
        {
            _stringval = "";
            Type = Cmqcfc.MqcftString;
            StrucLength = Cmqcfc.MqcfstStrucLengthFixed;
            Parameter = 0;
            CodedCharSetId = 0;
            _stringLength = 0;
            _stringval = "";
        }

        public Mqcfst(MQMessage message)
        {
            _stringval = "";
            Initialize(message);
        }

        public Mqcfst(int param, string str)
        {
            _stringval = "";
            Type = Cmqcfc.MqcftString;
            StrucLength = Cmqcfc.MqcfstStrucLengthFixed + str.Length;
            StrucLength += (4 - (StrucLength % 4)) % 4;
            Parameter = param;
            CodedCharSetId = 0;
            _stringLength = str.Length;
            _stringval = str;
        }

        public override object GetValue()
        {
            return _stringval;
        }

        internal override sealed void Initialize(MQMessage message)
        {
            Type = message.ReadInt4();
            StrucLength = message.ReadInt4();
            Parameter = message.ReadInt4();
            CodedCharSetId = message.ReadInt4();
            _stringLength = message.ReadInt4();
            _stringval = message.ReadString(_stringLength); //  Encoding.ASCII.GetString(message.ReadBytes(_stringLength), 0, _stringLength);
            var padding = (4 - ((Cmqcfc.MqcfstStrucLengthFixed + _stringLength) % 4)) % 4;
            message.SkipBytes(padding);
        }

        public void SetString(string str)
        {
            _stringval = str;
            _stringLength = _stringval.Length;
            StrucLength = Cmqcfc.MqcfstStrucLengthFixed + _stringLength;
            StrucLength += (4 - (StrucLength % 4)) % 4;
        }

        public override void SetValue(object val)
        {
            _stringval = (string) val;
            _stringLength = _stringval.Length;
            StrucLength = Cmqcfc.MqcfstStrucLengthFixed + _stringLength;
            StrucLength += (4 - (StrucLength % 4)) % 4;
        }

        public override int Write(MQMessage message)
        {
            return Write(message, Parameter, _stringval);
        }

        public static int Write(MQMessage message, int param, string val)
        {
            var totallen = 0;
            if (val == null) return totallen;
            var padding = (4 - ((Cmqcfc.MqcfstStrucLengthFixed + val.Length) % 4)) % 4;
            message.WriteInt4(Cmqcfc.MqcftString);
            message.WriteInt4((Cmqcfc.MqcfstStrucLengthFixed + val.Length) + padding);
            message.WriteInt4(param);
            message.WriteInt4(0);
            message.WriteInt4(val.Length);
            var bytes = new byte[val.Length];
            Encoding.ASCII.GetBytes(val, 0, val.Length, bytes, 0);
            message.Write(bytes);
            message.Write(new byte[padding]);
            totallen = (Cmqcfc.MqcfstStrucLengthFixed + val.Length) + padding;
            return totallen;
        }

        public override string GetStringValue()
        {
            return _stringval;
        }
    }
}
