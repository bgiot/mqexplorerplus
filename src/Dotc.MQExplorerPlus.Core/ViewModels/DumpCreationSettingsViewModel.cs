#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Views;
using System.ComponentModel.Composition;
using Dotc.MQExplorerPlus.Core.Models;
using System.Windows.Input;
using Dotc.MQExplorerPlus.Core.Services;
using static System.FormattableString;
using System.ComponentModel;
using Dotc.MQ;
using System.ComponentModel.DataAnnotations;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(DumpCreationSettingsViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]

    public class DumpCreationSettingsViewModel : ModalViewModel
    {
        [ImportingConstructor]
        public DumpCreationSettingsViewModel(IDumpCreationSettingsView view, IApplicationController appController) : base(view, appController)
        {
            Title = "Create Dump";
            SelectFileCommand = CreateCommand(SelectFile);
            WriteHeader = true;
            WriteMessageIndex = false;
            _leaveMessages = true;
            _useTransaction = false;
            WriteMessageDescriptor = true;
        }

        public void Initialize(QueueInfo queue)
        {
            Queue = queue;
        }

        public ICommand SelectFileCommand { get; private set; }


        public QueueInfo Queue { get; private set; }

        private void SelectFile()
        {
            string defaultFileName = Invariant($"{Queue.QueueManager.Name}_{Queue.Name}_{DateTime.Now:yyyyMMddHHmmss}.qdump");
            FileType[] ft = { new FileType("Queue dump", ".qdump"), new FileType("All", ".*") };

            var result = App.FileDialogService.ShowSaveFileDialog(App.ShellService.ShellView, ft, ft[0], defaultFileName);
            if (result.IsValid)
            {
                Filename = result.FileName;
            }
        }

        public DumpCreationSettings Settings { get; private set; }

        protected override bool OkAllowed()
        {
            return !string.IsNullOrWhiteSpace(Filename);
        }

        protected override void OnOk(CancelEventArgs e)
        {
            Settings = new DumpCreationSettings()
            {
                LeaveMessages = LeaveMessages,
                WriteHeader = WriteHeader,
                WriteMessageIndex = WriteMessageIndex,
                WriteMessageDescriptor = WriteMessageDescriptor,
                AddAsciiColumn = AddAsciiColumn,
                AsciiFile = AsciiFile
            };
            if (UseTransaction)
            {
                Settings.UseTransaction = true;
                if (TransactionSize.HasValue && TransactionSize.Value >= 0)
                    Settings.TransactionSize = TransactionSize.Value;
            }

            if (UseConversion)
            {
                Settings.Converter = new Conversion();
                if (CCSID.HasValue)
                    Settings.Converter.CodedCharSetId = CCSID.Value;
                if (Encoding.HasValue)
                    Settings.Converter.Encoding = Encoding.Value;
            }
        }

        private string _filename;
        public string Filename
        {
            get
            {
                return _filename;
            }
            private set
            {
                SetPropertyAndNotify(ref _filename, value);
            }
        }

        private bool _leaveMessages;
        public bool LeaveMessages
        {
            get { return _leaveMessages; }
            set
            {
                if (SetPropertyAndNotify(ref _leaveMessages, value))
                {
                    OnPropertyChanged(nameof(SupportTransactions));
                    if (value == true)
                    {
                        UseTransaction = false;
                        TransactionSize = null;
                    }
                }
            }
        }

        public bool SupportTransactions
        {
            get { return !LeaveMessages; }
        }
        public bool WriteHeader { get; set; }
        public bool WriteMessageIndex { get; set; }
        public bool WriteMessageDescriptor { get; set; }

        private bool _useTransaction;
        public bool UseTransaction
        {
            get { return _useTransaction; }
            set { SetPropertyAndNotify(ref _useTransaction, value); }
        }

        private int? _transactionSize;

        [Range(1, 999999999)]
        public int? TransactionSize
        {
            get { return _transactionSize; }
            set { SetPropertyAndNotify(ref _transactionSize, value); }
        }

        public bool AddAsciiColumn { get; set; }
        public bool AsciiFile { get; set; }

        private bool _useConversion;
        public bool UseConversion
        {
            get { return _useConversion; }
            set { SetPropertyAndNotify(ref _useConversion, value); }
        }
        public int? CCSID { get; set; }
        public int? Encoding { get; set; }
    }
}
