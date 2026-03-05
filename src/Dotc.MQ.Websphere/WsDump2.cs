#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using IBM.WMQ;
using System.IO;
using System.Collections;
using static Dotc.MQ.ExtensionMethods;

namespace Dotc.MQ.Websphere
{
    internal class WsDump2 : IDump
    {
        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;
        private readonly Encoding _encoding = Encoding.UTF8;

        private readonly WsQueue _qSource;

        internal WsDump2(WsQueue queue)
        {
            _qSource = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        private void AddExtraInfoToError(IDictionary data)
        {
            data.AddOrReplace("Queue", _qSource.Name);
            data.AddOrReplace("Connection", _qSource.QueueManager.ConnectionInfo);
        }

        #region Check Dump : Rely on Load Dump simulation mode
        public bool CheckDumpIsValid(string filename, CancellationToken ct, out int messagesCount, out string error)
        {
            messagesCount = 0;
            error = null;
            try
            {
                using (var sr = new StreamReader(filename))
                {
                    var dr = new DumpReader(sr);

                    MQMessage msg = null; // we simulate, so we don't need the mq message

                    while (ReadFileMessage(dr, out msg, Encoding.Default, HexStringConversion.None, true))
                    {
                        messagesCount++;
                    }
                }

                return true;
            }
            catch (DumpException ex)
            {
                error = ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                error = $"Unexpected error ({ex.Message})";
                return false;
            }

        }
        #endregion

        #region Create Dump
        public int CreateDump(IDumpCreationContext context, CancellationToken ct, IProgress<int> progress = null)
        {

            try
            {

                MQMessage message = null;
                bool transactionOpened = false;
                int filterIndex = 0;
                IdMatching idFilter = null;
                bool withFilter = context.Settings.IdFilters != null && context.Settings.IdFilters.Length > 0;

                using (var reader = new MQReader(_qSource.NewConnectionCore(), context.Settings.UseTransaction, context.Settings.Converter))
                {
                    var writer = new DumpWriter(context.Output, context.Settings, _qSource.QueueManager.Name, _qSource.Name, _culture, _encoding, context.EBCDICEncoding);

                    while (true)
                    {
                        idFilter = withFilter ? context.Settings.IdFilters[filterIndex++] : null;
                        bool msgFound = reader.Read(ref message, idFilter);
                        if (msgFound)
                        {
                        
                            if (ct.IsCancellationRequested)
                            break;

                            writer.WriteMessage(message);

                            if (!context.Settings.LeaveMessages)
                            {
                                // Force remove of the message
                                if (reader.Remove(ref message) && context.Settings.UseTransaction)
                                    transactionOpened = true;

                                if (context.Settings.UseTransaction && context.Settings.TransactionSize > 0 && writer.Counter % context.Settings.TransactionSize == 0)
                                {
                                    // Commit batch
                                    reader.Commit();
                                    transactionOpened = false;
                                }
                            }

                            progress?.Report(writer.Counter);

                            if (withFilter && filterIndex == context.Settings.IdFilters.Length)
                                break;

                        }
                        else
                        {
                            if (withFilter && filterIndex < context.Settings.IdFilters.Length)
                                continue;
                            else
                                break;
                        }
                    }

                    if (!context.Settings.LeaveMessages && context.Settings.UseTransaction && transactionOpened)
                    {
                        // Make sure to commit latest transaction
                        reader.Commit();
                    }

                    return writer.Counter;
                }

            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException(AddExtraInfoToError);
            }
        }

        #endregion

        #region Export Csv
        public void ExportToCsv(ICsvExportContext context, IEnumerable<IMessage> messages, CancellationToken ct, IProgress<int> progress = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            //using (var sw = File.CreateText(filename))
            //{
            //    var preamble = Encoding.UTF8.GetPreamble();
            //    sw.BaseStream.Write(preamble, 0, preamble.Length);

            var sw = context.Output;

            if (context.Settings.IncludeHexData)
            {
                WriteCsvRow(sw,
                    "PutTimestamp",
                    "Format",
                    "Priority",
                    "Length",
                    "Data",
                    "CCSID",
                    "Encoding",
                    "Hex");
            }
            else
            {
                WriteCsvRow(sw,
                    "PutTimestamp",
                    "Format",
                    "Priority",
                    "Length",
                    "Data");
            }

            int counter = 0;

            foreach (var msg in messages)
            {
                if (progress != null && ct.IsCancellationRequested)
                    break;

                var wsMsg = (WsMessage)msg;

                if (context.Settings.IncludeHexData)
                {
                    WriteCsvRow(sw,
                        wsMsg.PutTimestamp,
                        wsMsg.ExtendedProperties.Format,
                        wsMsg.ExtendedProperties.Priority,
                        wsMsg.Length,
                        wsMsg.Text,
                        wsMsg.ExtendedProperties.CharacterSet,
                        wsMsg.ExtendedProperties.Encoding,
                        wsMsg.Bytes.ToHexString()
                        );
                }
                else
                {
                    WriteCsvRow(sw,
                       wsMsg.PutTimestamp,
                       wsMsg.ExtendedProperties.Format,
                       wsMsg.ExtendedProperties.Priority,
                       wsMsg.Length,
                       wsMsg.Text
                       );
                }
                counter++;
                progress?.Report(counter);
            }
            //}
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
        #endregion

        #region Load dump
        public int LoadDump(IDumpLoadContext context, CancellationToken ct, IProgress<int> progress = null)
        {
            MQQueue ibmQ = null;

            var mqPutMsgOpts = new MQPutMessageOptions { Options = MQC.MQPMO_FAIL_IF_QUIESCING };

            if (context.Settings.UseTransaction)
                mqPutMsgOpts.Options |= MQC.MQPMO_SYNCPOINT;
            else
                mqPutMsgOpts.Options |= MQC.MQPMO_NO_SYNCPOINT;

            var openMode = OpenQueueMode.ForWrite;

            switch (context.Settings.Context)
            {
                case DumpLoadSettings.ContextMode.SetAll:
                    mqPutMsgOpts.Options |= MQC.MQPMO_SET_ALL_CONTEXT;
                    openMode = OpenQueueMode.ForWriteSetAllContext;
                    break;
                case DumpLoadSettings.ContextMode.SetIdentity:
                    mqPutMsgOpts.Options |= MQC.MQPMO_SET_IDENTITY_CONTEXT;
                    openMode = OpenQueueMode.ForWriteSetIdentityContext;
                    break;
                case DumpLoadSettings.ContextMode.NoContext:
                    mqPutMsgOpts.Options |= MQC.MQPMO_NO_CONTEXT;
                    break;
                case DumpLoadSettings.ContextMode.Default:
                    break;
            }

            try
            {
                int messagesCount = 0;
                var qs = _qSource.NewConnectionCore();
                ibmQ = qs.OpenQueueCore(openMode);

                var dr = new DumpReader(context.Input);

                MQMessage msg = null;

                HexStringConversion conversion = context.Settings.ConvertASCIIHexStringToEBCDIC ? HexStringConversion.ASCII_EBCDIC : HexStringConversion.None;

                while (ReadFileMessage(dr, out msg, context.EBCDICEncoding, conversion))
                {
                    messagesCount++;
                    ibmQ.Put(msg, mqPutMsgOpts);

                    if (context.Settings.UseTransaction
                        && context.Settings.TransactionSize > 0
                        && messagesCount % context.Settings.TransactionSize == 0)
                        qs.QueueManager.Commit();

                    progress?.Report(messagesCount);
                }


                if (context.Settings.UseTransaction &&
                    (context.Settings.TransactionSize <= 0 || messagesCount % context.Settings.TransactionSize != 0))
                {
                    qs.QueueManager.Commit();
                }

                ibmQ.Close();

                return messagesCount;
            }
            catch (DumpException)
            {
                throw;
            }
            catch (MQException ex)
            {
                throw new DumpException(ex.Message, ex.ReasonCode, ex);
            }
            catch (Exception ex)
            {
                throw new DumpException("Unexpected error!", ex);

            }
        }

        private bool ReadFileMessage(DumpReader reader, out MQMessage msg, Encoding ebcdicEncoding, HexStringConversion conversion, bool simulate = false)
        {
            msg = null;

            if (!reader.ReadLine(false, false))
            {
                return false;
            }

            if (reader.Line[0] != 'A' && reader.Line[0] != 'N')
                throw new DumpException($"Expected message attribute at line {reader.LineNo}");

            if (reader.Line[0] == 'N')
            {
                if (!reader.ReadLine(true, false))
                    throw new DumpException("Invalid end of file. Expected message data");
            }

            MQMessageDescriptor md = null;

            if (!simulate)
            {
                msg = new MQMessage();
                md = msg.GetDescriptor();
            }

            while (reader.Line[0] == 'A')
            {

                LoadMessageAttribute(reader, md, ebcdicEncoding, conversion);

                if (!reader.ReadLine(true, false))
                    throw new DumpException("Invalid end of file. Expected message data");
            }

            if (reader.Line[0] != 'X' && reader.Line[0] != 'T' && reader.Line[0] != 'S')
                throw new DumpException($"Expected message at line {reader.LineNo}");

            while (reader.Line[0] == 'X' || reader.Line[0] == 'T' || reader.Line[0] == 'S')
            {
                int skip = 1;
                if (reader.Line[0] == 'T')
                {
                    skip += 1;
                }
                else
                {
                    while (skip < reader.Line.Length && reader.Line[skip] == ' ')
                        skip += 1;
                }

                if (skip < reader.Line.Length)
                {

                    switch (reader.Line[0])
                    {
                        case 'X':
                            ReadMessageHex(reader, skip, msg);
                            break;
                        case 'S':
                            ReadMessageLine(reader, skip, msg);
                            break;
                        case 'T':
                            ReadMessageTextLine(reader, skip, msg);
                            break;

                    }
                }

                if (!reader.ReadLine(true, false))
                    break;
            }

            return true;

        }

        private void ReadMessageHex(DumpReader reader, int start, MQMessage msg)
        {
            var index = start;
            while (index < reader.Line.Length)
            {
                int ch = reader.Line[index];
                if (ch == ' ')
                    break;
                if (ch >= '0' && ch <= '9') ch = ch - '0';
                else if (ch >= 'A' && ch <= 'F') ch = ch - 'A' + 10;
                else if (ch >= 'a' && ch <= 'f') ch = ch - 'a' + 10;
                /* Oops                        */
                else
                    throw new DumpException($"Badly formed HEX string on line {reader.LineNo}");

                int val = ch * 16;
                index++;
                ch = reader.Line[index];
                if (ch >= '0' && ch <= '9') ch = ch - '0';
                else if (ch >= 'A' && ch <= 'F') ch = ch - 'A' + 10;
                else if (ch >= 'a' && ch <= 'f') ch = ch - 'a' + 10;
                /* Oops                        */
                else
                    throw new DumpException($"Badly formed HEX string on line {reader.LineNo}");

                msg?.WriteByte(val + ch);
                index++;
            }
        }

        private const char ESCAPE_CHAR = '~';

        private void ReadMessageLine(DumpReader reader, int start, MQMessage msg)
        {
            var line = reader.Line;
            var index = start;

            while (line[index] == ' ')
                index++;

            if (line[index] == '"')
            {
                index++;
                while (index < line.Length)
                {
                    if (line[index] == '"')
                        break;

                    if (line[index] == ESCAPE_CHAR)
                    {
                        index++;
                        switch (line[index])
                        {
                            case '"':
                            case ESCAPE_CHAR:
                                msg?.Write(line[index]);
                                break;
                            default:
                                throw new DumpException($"Unexpected escape sequence on line {reader.LineNo}");
                        }
                    }
                    else
                    {
                        msg?.Write(line[index]);
                        index++;
                    }
                }
            }
            else
            {
                int end = line.Length - 1;
                while (line[end] == ' ')
                    end--;

                while (index <= end)
                {
                    msg?.Write(line[index]);
                    index++;
                }
            }
        }

        private void ReadMessageTextLine(DumpReader reader, int start, MQMessage msg)
        {
            msg?.WriteString(reader.Line.Substring(start));
        }

        private void LoadMessageAttribute(DumpReader reader, MQMessageDescriptor md, Encoding ebcdicEncoding, HexStringConversion conversion)
        {
            if (reader.Line.Length < 5)
                throw new DumpException($"Invalid attribute line on line {reader.LineNo}");

            var code = reader.Line.Substring(2, 3);

            switch (code)
            {
                case "VER":
                case "RPT":
                case "MST":
                case "EXP":
                case "FDB":
                case "ENC":
                case "CCS":
                case "FMT":
                case "PRI":
                case "PER":
                case "MSI":
                case "COI":
                case "BOC":
                case "RTQ":
                case "RTM":
                case "USR":
                case "ACC":
                case "AID":
                case "AIX":
                case "PAT":
                case "PAN":
                case "PTD":
                case "PTT":
                case "AOD":
                case "AOX":
                case "GRP":
                case "MSQ":
                case "OFF":
                case "MSF":
                case "ORL":
                    break;
                default:
                    throw new DumpException($"Unrecognised attribute on line {reader.LineNo}");

            }

            if (reader.Line.Length < 7)
                return;


            var data = reader.Line.Substring(6);

            if (md != null)
            {

                try
                {
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
                            var format = new byte[8];
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
                            md.CorrelId = ConvertHexStringBytes(data, ebcdicEncoding, conversion);
                            break;
                        case "BOC":
                            md.BackoutCount = int.Parse(data, _culture);
                            break;
                        case "RTQ":
                            var rtq = new byte[48];
                            data.ToBytes(ref rtq, _encoding);
                            md.ReplyToQueue = rtq;
                            break;
                        case "RTM":
                            var rtm = new byte[48];
                            data.ToBytes(ref rtm, _encoding);
                            md.ReplyToQueueMgr = rtm;
                            break;
                        case "USR":
                            var usr = new byte[12];
                            data.ToBytes(ref usr, _encoding);
                            md.UserID = usr;
                            break;
                        case "ACC":
                            md.AccountingToken = data.HexStringToBytes();
                            break;
                        case "AID":
                            var aid = new byte[3];
                            data.ToBytes(ref aid, _encoding);
                            md.ApplIdentityData = aid;
                            break;
                        case "AIX":
                            md.ApplIdentityData = data.HexStringToBytes();
                            break;
                        case "PAT":
                            md.PutApplType = int.Parse(data, _culture);
                            break;
                        case "PAN":
                            var pan = new byte[28];
                            data.ToBytes(ref pan, _encoding);
                            md.PutApplName = pan;
                            break;
                        case "PTD":
                            var ptd = new byte[8];
                            data.ToBytes(ref ptd, _encoding);
                            md.PutDate = ptd;
                            break;
                        case "PTT":
                            var ptt = new byte[8];
                            data.ToBytes(ref ptt, _encoding);
                            md.PutTime = ptt;
                            break;
                        case "AOD":
                            var aod = new byte[4];
                            data.ToBytes(ref aod, _encoding);
                            md.ApplOriginData = aod;
                            break;
                        case "AOX":
                            md.ApplOriginData = data.HexStringToBytes();
                            break;
                        case "GRP":
                            md.GroupID = ConvertHexStringBytes(data, ebcdicEncoding, conversion); 
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
                catch (Exception ex)
                {
                    throw new DumpException($"Invalid message attribute value on line {reader.LineNo}", ex);
                }
            }

        }

        private byte[] ConvertHexStringBytes(string data, Encoding ebcdicEncoding, HexStringConversion conversion)
        {
            var dataAsBytes = data.HexStringToBytes();
            return dataAsBytes.ConvertHexString(ebcdicEncoding, conversion);
        }
        #endregion
    }


    internal sealed class DumpReader
    {
        private StreamReader _reader;
        internal DumpReader(StreamReader reader)
        {
            _reader = reader;
        }

        internal int LineNo { get; private set; }

        internal string Line { get; private set; }

        internal bool ReadLine(bool force, bool all)
        {
            if (!force && !string.IsNullOrEmpty(Line))
                return true;

            while ((Line = _reader.ReadLine()) != null)
            {
                LineNo++;

                if (all)
                    break;

                if (!string.IsNullOrWhiteSpace(Line)
                    && Line[0] != '*')
                    break;

            }

            return (Line != null);
        }
    }

    internal sealed class MQReader : IDisposable
    {
        private MQQueue _IbmQueue;
        private WsQueue _queue;

        internal bool UseTransaction { get; }
        internal Conversion Converter { get; }

        internal MQReader(WsQueue queue, bool useTransaction, Conversion converter)
        {
            _queue = queue;
            Converter = converter;
            UseTransaction = useTransaction;
            var oqm = OpenQueueMode.ForBrowseAndRead;
            _IbmQueue = queue.OpenQueueCore(oqm);
        }

        internal void Commit()
        {
            _queue.QueueManager.Commit();
        }

        public int CurrentIndex { get; private set; }

        internal bool Remove(ref MQMessage msg)
        {
            var gmo = new MQGetMessageOptions()
            {
                Options = MQC.MQGMO_NO_WAIT + MQC.MQGMO_PROPERTIES_AS_Q_DEF,
                Version = 1,
                MatchOptions = MQC.MQMO_MATCH_MSG_ID + MQC.MQMO_MATCH_CORREL_ID,
                GroupStatus = MQC.MQGS_NOT_IN_GROUP,
                SegmentStatus = MQC.MQSS_NOT_A_SEGMENT,
                Segmentation = MQC.MQSEG_INHIBITED,
            };
            gmo.Options |= MQC.MQGMO_MSG_UNDER_CURSOR;
            if (UseTransaction)
                gmo.Options |= MQC.MQGMO_SYNCPOINT;
            else gmo.Options |= MQC.MQGMO_NO_SYNCPOINT;

            try
            {
                _IbmQueue.Get(msg, gmo);
                return true;
            }
            catch (MQException exc)
            {
                if (exc.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE
                    || exc.ReasonCode == MQC.MQRC_NOT_CONVERTED)
                {
                    return false;
                }
                throw;
            }
        }

        internal bool Read(ref MQMessage msg, IdMatching filter )
        {
            var gmo = new MQGetMessageOptions()
            {
                Options = MQC.MQGMO_NO_WAIT + MQC.MQGMO_PROPERTIES_AS_Q_DEF,
                Version = 1,
                MatchOptions = MQC.MQMO_MATCH_MSG_ID + MQC.MQMO_MATCH_CORREL_ID,
                GroupStatus = MQC.MQGS_NOT_IN_GROUP,
                SegmentStatus = MQC.MQSS_NOT_A_SEGMENT,
                Segmentation = MQC.MQSEG_INHIBITED,
            };

            gmo.Options |= MQC.MQGMO_BROWSE_NEXT | MQC.MQGMO_NO_SYNCPOINT;

            msg = new MQMessage();

            if (filter != null)
            {
                if (filter.Type == IdMatching.IdType.MessageId)
                {
                    msg.MessageId = filter.Value;
                }
                if (filter.Type == IdMatching.IdType.CorrelationId)
                {
                    msg.CorrelationId = filter.Value;
                }
                if (filter.Type == IdMatching.IdType.GroupId)
                {
                    msg.GroupId = filter.Value;
                    gmo.MatchOptions |= MQC.MQMO_MATCH_GROUP_ID;
                    if (gmo.Version < 2) gmo.Version = 2;
                }
            }

            if (Converter != null)
            {
                gmo.Options |= MQC.MQGMO_CONVERT;
                msg.Encoding = Converter.Encoding;
                msg.CharacterSet = Converter.CodedCharSetId;
            }

            CurrentIndex++;

            try
            {
                _IbmQueue.Get(msg, gmo);
                return true;
            }
            catch (MQException exc)
            {
                if (exc.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE
                    || exc.ReasonCode == MQC.MQRC_NOT_CONVERTED)
                {
                    return false;
                }
                throw;
            }
        }

        public void Dispose()
        {
            _IbmQueue.Close();
        }
    }

    internal sealed class DumpWriter
    {
        private readonly CultureInfo _culture;
        private readonly Encoding _encoding;
        private readonly Encoding _ebcdicEncoding;

        private StreamWriter _sw;
        private DumpCreationSettings _settings;

        internal DumpWriter(StreamWriter output, DumpCreationSettings settings, string qmName, string qName, CultureInfo ci, Encoding encoding, Encoding ebcdicEncoding)
        {
            _culture = ci;
            _encoding = encoding;
            _ebcdicEncoding = ebcdicEncoding;
            _settings = settings;
            _sw = output;

            if (settings.WriteHeader)
                WriteDumpHeader(qmName, qName);

            Counter = 0;
        }


        public int Counter { get; private set; }

        private void WriteDumpHeader(string qmName, string qName)
        {
            _sw.WriteLine("* {0} Created:{1}", "QLOAD (Dotc.MQ.Dump)", DateTime.Now); // Need to put 'QLOAD' keyword to make the dump compatible
            _sw.WriteLine("* Qmgr  = {0}", qmName);
            _sw.WriteLine("* Queue = {0}", qName);
            _sw.WriteLine();
        }

        internal void WriteMessage(MQMessage message)
        {
            Counter++;

            if (_settings.WriteMessageIndex)
                _sw.WriteLine("* Index {0}", Counter);

            if (_settings.WriteMessageDescriptor)
                WriteMessageDescriptor(message.GetDescriptor());

            WriteMessageContent(message);

            _sw.WriteLine();
        }


        private void WriteMessageContent(MQMessage mQMessage)
        {
            var data = mQMessage.ReadBytes(mQMessage.DataLength);

            if (_settings.AsciiFile)
            {
                bool firstLine = true;
                int pStart = 0;
                int pEnd = data.Length;
                while (true)
                {
                    int len = 0;
                    int p = pStart;
                    while (p < pEnd && IsInvariant(data[p])) { p++; len++; }
                    /* As much as we can as string */
                    if (len > 0 || firstLine)
                    {
                        WriteString(ref data, pStart, len);
                    }

                    pStart += len;
                    len = 0;
                    p = pStart;
                    if (p >= pEnd) break;

                    while (p < pEnd && !IsInvariant(data[p])) { p++; len++; }
                    WriteHex(ref data, pStart, len);

                    pStart += len;
                    p = pStart;
                    if (p >= pEnd) break;
                    firstLine = false;
                }
            }
            else
            {
                WriteHex(ref data, 0, data.Length);
            }
        }

        static char[] HEX = "0123456789ABCDEF".ToCharArray();
        static Lazy<byte[]> ExtraInvariantChars = new Lazy<byte[]>(() => ASCIIEncoding.ASCII.GetBytes(" '()+,-./:=?_"));


        private bool IsPrintable(byte c) /* C equivalent */
        {
            return (c >= 32 && c <= 126);
        }

        private bool IsInvariant(byte c)
        {

            return (c >= 97 /* a */ && c <= 122 /* z */) ||
                (c >= 65 /* A */ && c <= 90 /* Z */) ||
                (c >= 48 /* 0 */ && c <= 57 /* 9 */) ||
                (ExtraInvariantChars.Value.Contains(c));


        }

        private void WriteHex(ref byte[] data, int pos, int length)
        {

            int startLine = pos;
            int i = 0;

            _sw.Write("X ");

            while (length > 0)
            {
                _sw.Write(HEX[data[pos] / 16]);
                _sw.Write(HEX[data[pos] % 16]);

                pos++; i++; length--;

                if (i >= _settings.DataWidth || length == 0)
                {
                    if (_settings.AddAsciiColumn)
                    {
                        int LineLength = i;
                        while (i < (_settings.DataWidth + 1))
                        {
                            _sw.Write("  ");
                            i++;
                        }
                        _sw.Write("<");
                        for (i = 0; i < LineLength; i++)
                        {
                            byte c = data[startLine + i];
                            if (IsPrintable(c))
                                _sw.Write(Convert.ToChar(c));
                            else
                                _sw.Write(".");
                        }
                        _sw.Write(">");
                    }

                    if (length == 0) break;

                    _sw.WriteLine();
                    _sw.Write("X ");
                    startLine = pos;
                    i = 0;
                }
            }
            _sw.WriteLine();
        }


        private void WriteString(ref byte[] data, int pos, int length)
        {
            while (true)
            {
                _sw.Write("S ");
                int lineLength = Math.Min(_settings.DataWidth * 2 - 2, length);

                if (length == 0 || lineLength == 0)
                {
                    _sw.WriteLine();
                    break;
                }
                _sw.Write("\"");
                for (int i = 0; i < lineLength; i++)
                {
                    char c = Convert.ToChar(data[i + pos]);
                    switch (c)
                    {
                        case '"':
                            _sw.Write("~\"");
                            break;
                        case '~':
                            _sw.Write("~~");
                            break;
                        default:
                            _sw.Write(c);
                            break;
                    }
                }
                length -= lineLength;
                pos += lineLength;
                _sw.WriteLine("\"");
                if (length == 0) break;
            }
        }

        private string ToHexStringConverted(byte[] data)
        {
            return data.ToHexString(
                ebcdicEncoding: _ebcdicEncoding, 
                conversion: _settings.ConvertEBCDICHexStringToASCII ? HexStringConversion.EBCDIC_ASCII : HexStringConversion.None
                );
        }

        private string ToHexStringNotConverted(byte[] data)
        {
            return data.ToHexString(
                ebcdicEncoding: _ebcdicEncoding,
                conversion: HexStringConversion.None
                );
        }

        private void WriteMessageDescriptor(MQMessageDescriptor md)
        {
            var ver = md.Version > 2 ? md.Version : 2;
            _sw.WriteLine("A VER {0}", ver.ToString(_culture));
            _sw.WriteLine("A RPT {0}", md.Report.ToString(_culture));
            _sw.WriteLine("A MST {0}", md.MsgType.ToString(_culture));
            _sw.WriteLine("A EXP {0}", md.Expiry.ToString(_culture));
            _sw.WriteLine("A FDB {0}", md.Feedback.ToString(_culture));
            _sw.WriteLine("A ENC {0}", md.Encoding.ToString(_culture));
            _sw.WriteLine("A CCS {0}", md.Ccsid.ToString(_culture));
            _sw.WriteLine("A FMT {0}", md.Format.ToString(_encoding));
            _sw.WriteLine("A PRI {0}", md.Priority.ToString(_culture));
            _sw.WriteLine("A PER {0}", md.Persistence.ToString(_culture));
            _sw.WriteLine("A MSI {0}", ToHexStringNotConverted(md.MsgId));
            _sw.WriteLine("A COI {0}", ToHexStringConverted(md.CorrelId));
            _sw.WriteLine("A BOC {0}", md.BackoutCount.ToString(_culture));
            _sw.WriteLine("A RTQ {0}", md.ReplyToQueue.ToString(_encoding));
            _sw.WriteLine("A RTM {0}", md.ReplyToQueueMgr.ToString(_encoding));
            _sw.WriteLine("A USR {0}", md.UserID.ToString(_encoding));
            _sw.WriteLine("A ACC {0}", ToHexStringNotConverted(md.AccountingToken));
            _sw.WriteLine("A AIX {0}", ToHexStringNotConverted(md.ApplIdentityData));
            _sw.WriteLine("A PAT {0}", md.PutApplType.ToString(_culture));
            _sw.WriteLine("A PAN {0}", md.PutApplName.ToString(_encoding));
            _sw.WriteLine("A PTD {0}", md.PutDate.ToString(_encoding));
            _sw.WriteLine("A PTT {0}", md.PutTime.ToString(_encoding));
            _sw.WriteLine("A AOX {0}", ToHexStringNotConverted(md.ApplOriginData));
            _sw.WriteLine("A GRP {0}", ToHexStringConverted(md.GroupID));
            _sw.WriteLine("A MSQ {0}", md.MsgSequenceNumber.ToString(_culture));
            _sw.WriteLine("A OFF {0}", md.Offset.ToString(_culture));
            _sw.WriteLine("A MSF {0}", md.MsgFlags.ToString(_culture));
            _sw.WriteLine("A ORL {0}", md.OriginalLength.ToString(_culture));
        }
    }

}

