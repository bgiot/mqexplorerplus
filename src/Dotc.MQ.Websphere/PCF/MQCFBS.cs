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
    internal class Mqcfbs : PcfParameter
    {
        private byte[] byteVal;
        private int stringLength;

        public Mqcfbs()
        {
            base.Type = Cmqcfc.MqcftByteString; // 9;
            base.StrucLength = Cmqcfc.MqcfbsStrucLengthFixed; // 0x10;
            base.Parameter = 0;
            this.stringLength = 0;
        }

        public Mqcfbs(MQMessage message)
        {
            this.Initialize(message);
        }

        public override object GetValue() =>
            this.byteVal;

        internal override sealed void Initialize(MQMessage message)
        {
            base.Type = message.ReadInt4();
            base.StrucLength = message.ReadInt4();
            base.Parameter = message.ReadInt4();
            this.stringLength = message.ReadInt4();
            this.byteVal = message.ReadBytes(this.stringLength);
            int padding = (4 - ((Cmqcfc.MqcfbsStrucLengthFixed + this.stringLength) % 4)) % 4;
            message.SkipBytes(padding);
        }

        public override void SetValue(object value)
        {
            this.byteVal = (byte[])value;
        }

        public override int Write(MQMessage message) =>
            Write(message, base.Parameter, this.byteVal);

        public static int Write(MQMessage message, int parameter, byte[] val)
        {
            int num = 0;
            if (val != null)
            {
                int num2 = (4 - ((Cmqcfc.MqcfbsStrucLengthFixed + val.Length) % 4)) % 4;
                message.WriteInt4(9);
                message.WriteInt4((Cmqcfc.MqcfbsStrucLengthFixed + val.Length) + num2);
                message.WriteInt4(parameter);
                message.WriteInt4(val.Length);
                message.Write(val);
                message.Write(new byte[num2]);
                num = (Cmqcfc.MqcfbsStrucLengthFixed + val.Length) + num2;
            }
            return num;
        }

        public override string GetStringValue()
        {
            var str = new StringBuilder();
            for (var i = 0; i < byteVal.Length; i++)
            {
                str.AppendFormat("{0:X2}", byteVal[i]);
            }
            return str.ToString();
        }
    }
}

