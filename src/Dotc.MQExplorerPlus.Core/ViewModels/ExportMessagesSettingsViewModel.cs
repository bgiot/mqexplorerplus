#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.FormattableString;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(ExportMessagesSettingsViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ExportMessagesSettingsViewModel : ModalViewModel
    {
        [ImportingConstructor]
        public ExportMessagesSettingsViewModel(IExportMessagesSettingsView view, IApplicationController appController) : base(view, appController)
        {
            Title = "Csv Export";
            SelectFileCommand = CreateCommand(SelectFile);
        }

        public void Initialize(QueueInfo queue, int messagesCount)
        {
            MessagesCount = messagesCount;
            Queue = queue;
        }

        public ICommand SelectFileCommand { get; private set; }

        public QueueInfo Queue { get; private set; }
        public int MessagesCount { get; private set; }

        private void SelectFile()
        {
            string defaultFileName = Invariant($"{Queue.QueueManager.Name}_{Queue.Name}_{DateTime.Now:yyyyMMddHHmmss}.csv");

            FileType[] ft = { new FileType("csv", ".csv"), new FileType("All", ".*") };

            var result = App.FileDialogService.ShowSaveFileDialog(App.ShellService.ShellView, ft, ft[0], defaultFileName);
            if (result.IsValid)
            {
                Filename = result.FileName;
            }
        }

        public CsvExportSettings Settings { get; private set; }

        protected override bool OkAllowed()
        {
            return !string.IsNullOrWhiteSpace(Filename);
        }

        protected override void OnOk(CancelEventArgs e)
        {
            Settings = new CsvExportSettings()
            {
                IncludeHexData = this.IncludeHexData
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
            }
        }

        public bool IncludeHexData { get; set; }

    }
}
