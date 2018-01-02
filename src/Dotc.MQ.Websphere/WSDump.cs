using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using IBM.WMQ;
using System.Threading;
using System.Collections;

namespace Dotc.MQ.Websphere
{
    [Obsolete("Use new WsDump2 engine!", true)]
    internal sealed class WsDump : IDump
    {
        private readonly WsQueue _qSource;
        private readonly FieldInfo _fiMqmd;
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

        internal WsDump(WsQueue queue)
        {
            _qSource = queue ?? throw new ArgumentNullException(nameof(queue));
            _fiMqmd = typeof(MQMessage).GetField("md", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void AddExtraInfoToError(IDictionary data)
        {
            data.AddOrReplace("Queue", _qSource.Name);
            data.AddOrReplace("Connection", _qSource.QueueManager.ConnectionInfo);
        }

        public int CreateDump(DumpCreationSettings settings, CancellationToken ct, IProgress<int> progress = null)
        {

            try
            {
                using (var sw = File.CreateText(settings.FileName))
                {
                    if (settings.WriteHeader)
                    {
                        CreateDumpHeader(sw);
                    }

                    var counter = 0;

                    var enumerator = _qSource.NewConnectionCore().DumpAllMessagesCore(settings.LeaveMessages);

                    while (enumerator.MoveNext())
                    {
                        if (progress != null && ct.IsCancellationRequested)
                            break;

                        counter++;
                        if (settings.WriteMessageIndex)
                            sw.WriteLine("* Index {0}", counter);

                        DumpMessage(enumerator.Current, sw);
                        if (counter % 50 == 0)
                        {
                            progress?.Report(counter);
                        }
                    }

                    progress?.Report(counter);

                    return counter;
                }
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        public bool CheckDumpIsValid(string filename, CancellationToken ct, out int messagesCount, out string error)
        {
            using (var sr = File.OpenText(filename))
            {
                var counter = 0;
                var previousFirstChar = char.MinValue;
                messagesCount = 0;
                error = null;

                while (!sr.EndOfStream)
                {
                    if (ct.IsCancellationRequested)
                    {
                        return false;
                    }
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        if (previousFirstChar != 'X') continue;
                        counter++;
                        previousFirstChar = char.MinValue;
                    }
                    else
                    {
                        switch (line[0])
                        {
                            case '*': // comment
                                break;
                            case 'A': // attribute
                                break;
                            case 'X': // data
                                break;
                            default:
                                return false;
                        }
                        previousFirstChar = line[0];
                    }
                }
                messagesCount = counter;
                return true;
            }
        }

        public int LoadDump(DumpLoadSettings settings, CancellationToken ct, IProgress<int> progress = null)
        {
            MQQueue ibmQ = null;

            var mqPutMsgOpts = new MQPutMessageOptions { Options = MQC.MQPMO_FAIL_IF_QUIESCING | MQC.MQPMO_ASYNC_RESPONSE };

            if (settings.UseTransaction)
                mqPutMsgOpts.Options |= MQC.MQPMO_SYNCPOINT;
            else
                mqPutMsgOpts.Options |= MQC.MQPMO_NO_SYNCPOINT;

            switch (settings.Context)
            {
                case DumpLoadSettings.ContextMode.SetAll:
                    mqPutMsgOpts.Options |= MQC.MQPMO_SET_ALL_CONTEXT;
                    break;
                case DumpLoadSettings.ContextMode.SetIdentity:
                    mqPutMsgOpts.Options |= MQC.MQPMO_SET_IDENTITY_CONTEXT;
                    break;
                case DumpLoadSettings.ContextMode.NoContext:
                    mqPutMsgOpts.Options |= MQC.MQPMO_NO_CONTEXT;
                    break;
                case DumpLoadSettings.ContextMode.Default:
                    break;
            }

            try
            {
                ibmQ = _qSource.NewConnectionCore().OpenQueueCore(OpenQueueMode.ForWrite);
                using (var sr = File.OpenText(settings.FileName))
                {
                    MQMessage msg = null;
                    MQMessageDescriptor md = null;

                    var counter = 0;

                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();

                        if (string.IsNullOrEmpty(line)) // a blank line between each message
                        {
                            if (msg != null)
                            {
                                if (progress != null && ct.IsCancellationRequested)
                                    break;

                                ibmQ.Put(msg, mqPutMsgOpts);
                                counter++;

                                if (counter % 100 == 0)
                                {
                                    progress?.Report(counter);
                                }
                            }
                            msg = new MQMessage();
                            md = GetDescriptor(msg);
                        }
                        else
                        {
                            switch (line[0])
                            {
                                case 'A':
                                    LoadMessageAttribute(line, md);
                                    break;
                                case 'X':
                                    LoadMessageContent(line, msg);
                                    break;
                            }
                        }
                    }
                    progress?.Report(counter);
                    return counter;
                }
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
            finally
            {
                ibmQ?.Close();
            }
        }


        public void ExportToCsv(string filename, IEnumerable<IMessage> messages, CancellationToken ct, IProgress<int> progress = null)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            using (var sw = File.CreateText(filename))
            {
                WriteCsvRow(sw,
                    "PutTimestamp",
                    "Format",
                    "Priority",
                    "Length",
                    "Data");

                int counter = 0;

                foreach (var msg in messages)
                {
                    if (progress != null && ct.IsCancellationRequested)
                        break;

                    WriteCsvRow(sw,
                        msg.PutTimestamp,
                        msg.ExtendedProperties.Format,
                        msg.ExtendedProperties.Priority,
                        msg.Length,
                        msg.Text
                        );

                    counter++;
                    if (counter % 50 == 0)
                    {
                        progress?.Report(counter);
                    }
                }
                progress?.Report(counter);
            }
        }

        private static void WriteCsvRow(TextWriter sw, params object[] data)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                var value = data[i];
                if (i > 0)
                {
                    sb.Append(",");
                }
                var s = value as string;
                if (s != null)
                {
                    sb.AppendFormat("\"{0}\"", s.Replace("\"", "\"\""));
                }
                else if (value is DateTime)
                {
                    sb.AppendFormat("\"{0}\"", ((DateTime)value).ToString("yyyy-MM-dd-HH.mm.ss.ffffff", CultureInfo.InvariantCulture));
                }
                else
                {
                    sb.AppendFormat("\"{0}\"", Convert.ToString(value, CultureInfo.InvariantCulture));
                }
            }
            sw.WriteLine(sb.ToString());
        }



        private void CreateDumpHeader(TextWriter sw)
        {
            sw.WriteLine("* {0} Created:{1}", "QLOAD (Dotc.MQ.Dump)", DateTime.Now);
            sw.WriteLine("* Qmgr  = {0}", _qSource.QueueManager.Name);
            sw.WriteLine("* Queue = {0}", _qSource.Name);
            sw.WriteLine();
        }



        private static void LoadMessageContent(string line, MQMessage msg)
        {
            var data = line.Substring(2).HexStringToBytes();
            msg.Write(data);
        }


        private void LoadMessageAttribute(string line, MQMessageDescriptor md)
        {
            var code = line.Substring(2, 3);
            var data = line.Substring(6);
            //var md = GetDescriptor(msg);

            switch (code)
            {
                case "VER":
                    md.Version = int.Parse(data, _culture);
                    break;
                case "RPT":
                    md.Report = int.Parse(data, _culture);
                    break;
                case "MST":
                    md.MsgType = int.Parse(data, _culture);
                    break;
                case "EXP":
                    md.Expiry = int.Parse(data, _culture);
                    break;
                case "FDB":
                    md.Feedback = int.Parse(data, _culture);
                    break;
                case "ENC":
                    md.Encoding = int.Parse(data, _culture);
                    break;
                case "CCS":
                    md.Ccsid = int.Parse(data, _culture);
                    break;
                case "FMT":
                    var format = md.Format;
                    data.ToBytes(ref format, _encoding);
                    md.Format = format;
                    break;
                case "PRI":
                    md.Priority = int.Parse(data, _culture);
                    break;
                case "PER":
                    md.Persistence = int.Parse(data, _culture);
                    break;
                case "MSI":
                    md.MsgId = data.HexStringToBytes();
                    break;
                case "COI":
                    md.CorrelId = data.HexStringToBytes();
                    break;
                case "BOC":
                    md.BackoutCount = int.Parse(data, _culture);
                    break;
                case "RTQ":
                    var rtq = md.ReplyToQueue;
                    data.ToBytes(ref rtq, _encoding);
                    md.ReplyToQueue = rtq;
                    break;
                case "RTM":
                    var rtm = md.ReplyToQueue;
                    data.ToBytes(ref rtm, _encoding);
                    md.ReplyToQueueMgr = rtm;
                    break;
                case "USR":
                    var usr = md.UserID;
                    data.ToBytes(ref usr, _encoding);
                    md.UserID = usr;
                    break;
                case "ACC":
                    md.AccountingToken = data.HexStringToBytes();
                    break;
                case "AIX":
                    md.ApplIdentityData = data.HexStringToBytes();
                    break;
                case "PAT":
                    md.PutApplType = int.Parse(data, _culture);
                    break;
                case "PAN":
                    var pan = md.PutApplName;
                    data.ToBytes(ref pan, _encoding);
                    md.PutApplName = pan;
                    break;
                case "PTD":
                    var ptd = md.PutDate;
                    data.ToBytes(ref ptd, _encoding);
                    md.PutDate = ptd;
                    break;
                case "PTT":
                    var ptt = md.PutTime;
                    data.ToBytes(ref ptt, _encoding);
                    md.PutTime = ptt;
                    break;
                case "AOX":
                    md.ApplOriginData = data.HexStringToBytes();
                    break;
                case "GRP":
                    md.GroupID = data.HexStringToBytes();
                    break;
                case "MSQ":
                    md.MsgSequenceNumber = int.Parse(data, _culture);
                    break;
                case "OFF":
                    md.Offset = int.Parse(data, _culture);
                    break;
                case "MSF":
                    md.MsgFlags = int.Parse(data, _culture);
                    break;
                case "ORL":
                    md.OriginalLength = int.Parse(data, _culture);
                    break;

            }
        }

        private void DumpMessage(MQMessage mQMessage, TextWriter sw)
        {
            DumpMessageAttributes(GetDescriptor(mQMessage), sw);
            DumpMessageContent(mQMessage, sw);
            sw.WriteLine();
        }

        private MQMessageDescriptor GetDescriptor(MQMessage msg)
        {
            return (MQMessageDescriptor)_fiMqmd.GetValue(msg);
            //return msg.MQMD;
        }


        private static void DumpMessageContent(MQMessage mQMessage, TextWriter sw)
        {
            var data = mQMessage.ReadBytes(mQMessage.DataLength);
            var i = 0;
            while (true)
            {
                var len = Math.Min(25, data.Length - i);
                var dataline = new byte[len];
                Array.Copy(data, i, dataline, 0, len);
                sw.WriteLine("X {0}", dataline.ToHexString());
                i += len;
                if (i >= data.Length)
                    break;
            }

        }

        private void DumpMessageAttributes(MQMessageDescriptor md, TextWriter sw)
        {
            var ver = md.Version > 2 ? md.Version : 2;
            sw.WriteLine("A VER {0}", ver.ToString(_culture));
            sw.WriteLine("A RPT {0}", md.Report.ToString(_culture));
            sw.WriteLine("A MST {0}", md.MsgType.ToString(_culture));
            sw.WriteLine("A EXP {0}", md.Expiry.ToString(_culture));
            sw.WriteLine("A FDB {0}", md.Feedback.ToString(_culture));
            sw.WriteLine("A ENC {0}", md.Encoding.ToString(_culture));
            sw.WriteLine("A CCS {0}", md.Ccsid.ToString(_culture));
            sw.WriteLine("A FMT {0}", md.Format.ToString(_encoding));
            sw.WriteLine("A PRI {0}", md.Priority.ToString(_culture));
            sw.WriteLine("A PER {0}", md.Persistence.ToString(_culture));
            sw.WriteLine("A MSI {0}", md.MsgId.ToHexString());
            sw.WriteLine("A COI {0}", md.CorrelId.ToHexString());
            sw.WriteLine("A BOC {0}", md.BackoutCount.ToString(_culture));
            sw.WriteLine("A RTQ {0}", md.ReplyToQueue.ToString(_encoding));
            sw.WriteLine("A RTM {0}", md.ReplyToQueueMgr.ToString(_encoding));
            sw.WriteLine("A USR {0}", md.UserID.ToString(_encoding));
            sw.WriteLine("A ACC {0}", md.AccountingToken.ToHexString());
            sw.WriteLine("A AIX {0}", md.ApplIdentityData.ToHexString());
            sw.WriteLine("A PAT {0}", md.PutApplType.ToString(_culture));
            sw.WriteLine("A PAN {0}", md.PutApplName.ToString(_encoding));
            sw.WriteLine("A PTD {0}", md.PutDate.ToString(_encoding));
            sw.WriteLine("A PTT {0}", md.PutTime.ToString(_encoding));
            sw.WriteLine("A AOX {0}", md.ApplOriginData.ToHexString());
            sw.WriteLine("A GRP {0}", md.GroupID.ToHexString());
            sw.WriteLine("A MSQ {0}", md.MsgSequenceNumber.ToString(_culture));
            sw.WriteLine("A OFF {0}", md.Offset.ToString(_culture));
            sw.WriteLine("A MSF {0}", md.MsgFlags.ToString(_culture));
            sw.WriteLine("A ORL {0}", md.OriginalLength.ToString(_culture));
        }

    }
}
