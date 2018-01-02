#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using IBM.WMQ;

namespace Dotc.MQ.Websphere.PCF
{
    internal class Mqcfh : PcfHeader
    {
        public int Version { get; private set; }

        public Mqcfh()
        {
            Version = 1;
            MsgSeqNumber = 1;
            Control = 1;
            Type = 1;
            StrucLength = Cmqcfc.MqcfhStrucLength;
            Command = 0;
            ParameterCount = 0;
        }

        public Mqcfh(MQMessage message)
        {
            Version = 1;
            MsgSeqNumber = 1;
            Control = 1;
            Initialize(message);
        }

        public Mqcfh(int cmd, int paramCount)
        {
            Version = 1;
            MsgSeqNumber = 1;
            Control = 1;
            Type = 1;
            StrucLength = Cmqcfc.MqcfhStrucLength;
            Command = cmd;
            CompCode = 0;
            Reason = 0;
            ParameterCount = paramCount;
        }

        internal override sealed void Initialize(MQMessage message)
        {
            Type = message.ReadInt4();
            StrucLength = message.ReadInt4();
            Version = message.ReadInt4();
            Command = message.ReadInt4();
            MsgSeqNumber = message.ReadInt4();
            Control = message.ReadInt4();
            CompCode = message.ReadInt4();
            Reason = message.ReadInt4();
            ParameterCount = message.ReadInt4();
        }

        public override int Write(MQMessage message)
        {
            return Write(message, Command, ParameterCount);
        }

        public static int Write(MQMessage message, int command, int paramCount)
        {
            return Cmdtype == Cmqcfc.MqcftCommandXr 
                ? Write(message, command, paramCount, Cmqcfc.MqcftCommandXr, Cmqcfc.MqcfhVersion3) 
                : Write(message, command, paramCount, Cmqcfc.MqcftCommand, Cmqcfc.MqcfhVersion1);
        }

        public static int Write(MQMessage message, int command, int paramCount, int type, int version)
        {
            message.WriteInt4(type);
            message.WriteInt4(Cmqcfc.MqcfhStrucLength);
            message.WriteInt4(version);
            message.WriteInt4(command);
            message.WriteInt4(1); // MsgSeqNumber
            message.WriteInt4(1); // Control
            message.WriteInt4(0); // CompCode
            message.WriteInt4(0); // Reason
            message.WriteInt4(paramCount);
            return 0x24;
        }

        public int Command { get; set; }

        public int CompCode { get; set; }

        public int Control { get; set; }

        public int MsgSeqNumber { get; set; }

        public int Reason { get; set; }

        public int ParameterCount { get; set; }
    }
}
