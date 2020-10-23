#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Dotc.MQ.Websphere
{
    internal static class ExtensionMethods
    {
        public static MqException ToMqException(this IBM.WMQ.MQException ibmException, Action<IDictionary> extraDataLoad = null)
        {
            if (ibmException == null) throw new ArgumentNullException(nameof(ibmException));
            var ex = new MqException(ibmException.Message, ibmException.Reason, ibmException);
            if (extraDataLoad != null)
            {
                extraDataLoad.Invoke(ex.Data);
            }
            return ex;
        }


        public static bool TryGetIntParameterValue(this PCF.PcfMessage pcfMsg, int parameter, out int value)
        {
            try
            {
                value = pcfMsg.GetIntParameterValue(parameter);
                return true;
            }
            catch(PCF.PcfException)
            {
                value = 0;
                return false;
            }
        }

        public static bool TryGetStringParameterValue(this PCF.PcfMessage pcfMsg, int parameter, out string value)
        {
            try
            {
                value = pcfMsg.GetStringParameterValue(parameter);
                return true;
            }
            catch (PCF.PcfException)
            {
                value = null;
                return false;
            }
        }

        private static FieldInfo _fiMqmd;

        public static IBM.WMQ.MQMessageDescriptor GetDescriptor(this IBM.WMQ.MQMessage message)
        {
            if (_fiMqmd == null)
                _fiMqmd = typeof(IBM.WMQ.MQMessage).GetField("md", BindingFlags.NonPublic | BindingFlags.Instance);

            return (IBM.WMQ.MQMessageDescriptor)_fiMqmd.GetValue(message);

        }

        public static string ReadStringEx(this IBM.WMQ.MQMessage message)
        {
            try
            {
                var srcEncoding = WsUtils.GetEncoding(message.CharacterSet);
                var bytes = message.ReadBytes(message.MessageLength);
                message.Seek(0);
                var str = Encoding.Convert(srcEncoding, Encoding.UTF8, bytes);
                return Encoding.UTF8.GetString(str);
            }
            catch (Exception)
            {
                // fallback option...
                var result = message.ReadString(message.MessageLength);
                message.Seek(0);
                return result;
            }
        }

        public static byte[] ReadBytesEx(this IBM.WMQ.MQMessage message)
        {
            var result = message.ReadBytes(message.MessageLength);
            message.Seek(0);
            return result;
        }


    }
}
