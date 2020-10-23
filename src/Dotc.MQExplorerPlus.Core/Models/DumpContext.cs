#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQ;
using System;
using System.IO;
using System.Text;

namespace Dotc.MQExplorerPlus.Core.Models
{

    public sealed class DumpCreationContext : IDumpCreationContext
    {
        public DumpCreationContext(string filename, DumpCreationSettings settings)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            Output = File.CreateText(filename);
        }
        public DumpCreationSettings Settings { get; private set; }

        public StreamWriter Output { get; private set; }

        public void Dispose()
        {
            Output?.Dispose();
        }
    }

    public sealed class DumpLoadContext : IDumpLoadContext
    {

        public DumpLoadContext(string filename, DumpLoadSettings settings)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            Input = new StreamReader(filename);
        }
        public DumpLoadSettings Settings { get; private set; }

        public StreamReader Input { get; private set; }

        public void Dispose()
        {
            Input?.Dispose();
        }
    }

    public sealed class CsvExportContext : ICsvExportContext
    {
        public CsvExportContext(string filename, CsvExportSettings settings)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            Output = File.CreateText(filename);
            var preamble = Encoding.UTF8.GetPreamble();
            Output.BaseStream.Write(preamble, 0, preamble.Length);

        }
        public CsvExportSettings Settings { get; private set; }

        public StreamWriter Output { get; private set; }

        public void Dispose()
        {
            Output?.Dispose();
        }
    }

}
