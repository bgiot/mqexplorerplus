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
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.Models;
using static System.FormattableString;
using System.Windows.Input;
using System.ComponentModel;
using Dotc.MQ;
using System.Threading;
using System.ComponentModel.DataAnnotations;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(DumpLoadSettingsViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class DumpLoadSettingsViewModel : ModalViewModel
    {
        [ImportingConstructor]
        public DumpLoadSettingsViewModel(IDumpLoadSettingsView view, IApplicationController appController) : base(view, appController)
        {
            Title = "Load Dump";
            SelectFileCommand = CreateCommand(SelectFile);
            _selectedContext = DumpLoadSettings.ContextMode.SetAll;
        }

        public void Initialize(QueueInfo queue)
        {
            Queue = queue;
        }

        public ICommand SelectFileCommand { get; private set; }


        public QueueInfo Queue { get; private set; }

        private void SelectFile()
        {
            FileType[] ft = { new FileType("Queue dump", ".qdump"), new FileType("All", ".*") };

            var result = App.FileDialogService.ShowOpenFileDialog(App.ShellService.ShellView, ft, ft[0], string.Empty);
            if (result.IsValid)
            {
                Filename = result.FileName;
            }

        }

        public DumpLoadSettings Settings { get; private set; }

        private bool _dumpIsValid;
        public bool DumpIsValid
        {
            get { return _dumpIsValid; }
            set { SetPropertyAndNotify(ref _dumpIsValid, value); }
        }

        private string _dumpStatus;
        public string DumpStatus
        {
            get { return _dumpStatus; }
            set { SetPropertyAndNotify(ref _dumpStatus, value); }
        }
        protected override bool OkAllowed()
        {
            return DumpIsValid;
        }

        private DumpLoadSettings.ContextMode _selectedContext;
        public DumpLoadSettings.ContextMode SelectedContext
        {
            get { return _selectedContext; }
            set
            {
                SetPropertyAndNotify(ref _selectedContext, value);
            }
        }

        protected override void OnOk(CancelEventArgs e)
        {
            Settings = new DumpLoadSettings()
            {
                Context = SelectedContext,
                UseTransaction = UseTransaction,
                TransactionSize = TransactionSize.HasValue ? TransactionSize.Value : 100
            };
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
                CheckDumpIsValid();
            }
        }

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
        private void CheckDumpIsValid()
        {
            DumpIsValid = false;
            DumpStatus = null;
            if (!string.IsNullOrEmpty(Filename))
            {
                int msgCount = 0;
                string error = null;
                DumpIsValid = Queue.DumpEngine.CheckDumpIsValid(Filename, CancellationToken.None, out msgCount, out error);
                if (DumpIsValid)
                {
                    DumpStatus = Invariant($"Dump file OK. It contains {msgCount} message(s).");
                }
                else
                {
                    DumpStatus = Invariant($"Dump file Invalid! {error}.");
                }
            }
        }
    }
}
