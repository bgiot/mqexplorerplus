#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Dotc.MQ
{

    public class IdMatching
    {
        public enum IdType
        {
            MessageId,
            CorrelationId,
            GroupId,
        }

        public IdMatching(IdType type, byte[] value)
        {
            Type = type;
            Value = value;
        }

        public IdType Type { get; }
        public byte[] Value { get; }
    }

    public class Conversion
    {

        public Conversion()
        {
            CodedCharSetId = 0; // MQCCSI_Q_MGR
            Encoding = 0x00000222; // MQENC_NATIVE
        }

        public int CodedCharSetId { get; set; }
        public int Encoding { get; set; }
    }


    public sealed class DumpCreationSettings
    {

        public DumpCreationSettings()
        {
            LeaveMessages = true;
            UseTransaction = false;
            WriteHeader = true;
            WriteMessageDescriptor = true;
            TransactionSize = 100;
            DataWidth = 25;
        }

        public IdMatching IdFilter { get; set; }
        public bool LeaveMessages { get; set; }
        public bool WriteHeader { get; set; }
        public bool WriteMessageIndex { get; set; }
        public bool WriteMessageDescriptor { get; set; }
        public bool UseTransaction { get; set; }
        public int TransactionSize { get; set; }

        public Conversion Converter { get; set; }

        public int DataWidth { get; set; }
        public bool AddAsciiColumn { get; set; }

        public bool AsciiFile { get; set; }
    }

    public sealed class DumpLoadSettings
    {
        public enum ContextMode
        {
            Default = 0,
            SetAll,
            SetIdentity,
            //PassAll,
            //PassIdentity,
            NoContext,
        }
        public DumpLoadSettings()
        {
            Context = ContextMode.SetAll;
            UseTransaction = true;
            TransactionSize = 100;
        }

        public ContextMode Context { get; set; }
        public bool UseTransaction { get; set; }

        public int TransactionSize { get; set; }

    }

    public interface IDumpCreationContext : IDisposable
    {
        DumpCreationSettings Settings { get; }
        StreamWriter Output { get; }
    }

    public interface IDumpLoadContext : IDisposable
    {
        DumpLoadSettings Settings { get; }
        StreamReader Input { get; }
    }

    public class CsvExportSettings
    {
        public bool IncludeHexData { get; set; }
    }

    public interface ICsvExportContext : IDisposable
    {
        CsvExportSettings Settings { get; }
        StreamWriter Output { get; }
    }

    public interface IDump
    {
        int CreateDump(IDumpCreationContext context, CancellationToken ct, IProgress<int> progress = null);
        bool CheckDumpIsValid(string filename, CancellationToken ct, out int messagesCount, out string error);
        int LoadDump(IDumpLoadContext context, CancellationToken ct, IProgress<int> progress = null);
        void ExportToCsv(ICsvExportContext context, IEnumerable<IMessage> messages, CancellationToken ct, IProgress<int> progress = null);
    }
}
