#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using IBM.WMQ;

namespace Dotc.MQ.Websphere.PCF
{
    internal class PcfMessage : PcfHeader
    {
        internal enum ParameterErrorType
        {
            NotFound,
            WrongType,
        }

        private List<PcfParameter> _list;
        private Mqcfh _mqCfh;

        public PcfMessage(MQMessage message)
        {
            _list = new List<PcfParameter>();
            _mqCfh = new Mqcfh(message);
            for (var i = 0; i < _mqCfh.ParameterCount; i++)
            {
                AddParameter(PcfParameter.NextParameter(message));
            }
        }

        public PcfMessage(int command)
        {
            Initialize(command);
        }

        public PcfMessage(int type, int command, int msgSeqNumber, bool last)
        {
            Initialize(type, command, msgSeqNumber, last);
        }

        public void AddParameter(PcfParameter parameter)
        {
            _list.Add(parameter);
        }

        public void AddParameter(int parameter, int value)
        {
            var mqcfin = new Mqcfin(parameter, value);
            _list.Add(mqcfin);
        }

        public void AddParameter(int parameter, int[] values)
        {
            var mqcfil = new Mqcfil(parameter, values);
            _list.Add(mqcfil);
        }

        public void AddParameter(int parameter, string value)
        {
            var mqcfst = new Mqcfst(parameter, value);
            _list.Add(mqcfst);
        }

        public void AddParameter(int parameter, string[] values)
        {
            var mqcfsl = new Mqcfsl(parameter, values);
            _list.Add(mqcfsl);
        }

        public int GetCommand()
        {
            return _mqCfh.Command;
        }

        public int GetCompCode()
        {
            return _mqCfh.CompCode;
        }

        public int GetControl()
        {
            return _mqCfh.Control;
        }

        private PcfException BuildParameterError(int parameter, ParameterErrorType errorType )
        {
            return PcfException.Build(2, PcfException.MqrccfCfilParmIdError, (data) =>
            {
                data.AddOrReplace("Parameter", parameter);
                data.AddOrReplace("Error",errorType.ToString());
            });
        }

        public int[] GetIntListParameterValue(int parameter)
        {       
            foreach (var p in _list.Where(p => p.Parameter == parameter))
            {
                if (p.Type != Cmqcfc.MqcftIntegerList)
                {
                    throw BuildParameterError(parameter, ParameterErrorType.WrongType);// new PcfException(2, PcfException.MqrccfCfilParmIdError);
                }
                return (int[]) p.GetValue();
            }
            throw BuildParameterError(parameter, ParameterErrorType.NotFound);// throw new PcfException(2, PcfException.MqrccfCfilParmIdError);
        }

        public int GetIntParameterValue(int parameter)
        {
            foreach (var p in _list.Where(p => p.Parameter == parameter))
            {
                if (p.Type != Cmqcfc.MqcftInteger)
                {
                    throw BuildParameterError(parameter, ParameterErrorType.WrongType);// throw new PcfException(2, PcfException.MqrccfCfinParmIdError);
                }
                return (int) p.GetValue();
            }
            throw BuildParameterError(parameter, ParameterErrorType.NotFound);// throw new PcfException(2, PcfException.MqrccfCfinParmIdError);
        }

        public int GetMsgSeqNumber()
        {
            return _mqCfh.MsgSeqNumber;
        }

        public int GetParameterCount()
        {
            return _list.Count;
        }

        public PcfParameter[] GetParameters()
        {
            return _list.Count == 0 ? null : _list.ToArray();
        }

        public object GetParameterValue(int parameter)
        {
            throw new NotImplementedException();
        }

        //private int GetReason()
        //{
        //    return _mqCfh.Reason;
        //}

        public IEnumerable<string> GetStringListParameterValue(int parameter)
        {
            foreach (var p in _list.Where(p => p.Parameter == parameter))
            {
                if (p.Type != Cmqcfc.MqcftStringList)
                {
                    throw BuildParameterError(parameter, ParameterErrorType.WrongType);// throw new PcfException(2, PcfException.MqrccfCfslParmIdError);
                }
                return (string[]) p.GetValue();
            }
            throw BuildParameterError(parameter, ParameterErrorType.NotFound);// throw new PcfException(2, PcfException.MqrccfCfslParmIdError);
        }

        public string GetStringParameterValue(int parameter)
        {
            foreach (var p in _list.Where(p => p.Parameter == parameter))
            {
                if (p.Type != Cmqcfc.MqcftString)
                {
                    throw BuildParameterError(parameter, ParameterErrorType.WrongType);// throw new PcfException(2, PcfException.MqrccfCfstParmIdError);
                }
                return (string) p.GetValue();
            }
            throw BuildParameterError(parameter, ParameterErrorType.NotFound);// throw new PcfException(2, PcfException.MqrccfCfstParmIdError);
        }

        internal override void Initialize(MQMessage message)
        {
            throw new NotImplementedException();
        }

        private void Initialize(int command)
        {
            Initialize(1, command, 1, true);
        }

        private void Initialize(int type, int command, int msgSeqNumber, bool last)
        {
            _list = new List<PcfParameter>();
            _mqCfh = new Mqcfh
            {
                CompCode = 0,
                Reason = 0,
                Command = command,
                Type = type,
                MsgSeqNumber = msgSeqNumber
            };
            Cmdtype = type;
            _mqCfh.Control = last ? 1 : 0;
        }

        public new int Size()
        {
            return _mqCfh.Size + _list.Sum(t => t.Size);
        }

        public override int Write(MQMessage message)
        {
            var num = 0;
            message.ClearMessage();
            message.MessageType = 1;
            message.Expiry = 0x3e8;
            message.Format = MQC.MQFMT_ADMIN;
            message.Feedback = 0;
            _mqCfh.ParameterCount = _list.Count;
            num += _mqCfh.Write(message);
            num += _list.Sum(parameter => parameter.Write(message));
            return num;
        }
    }
}
