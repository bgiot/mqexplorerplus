#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.MQ;
using System.Text.RegularExpressions;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{

    public enum PutMessageMode
    {
        Single,
        FromFile,
    }

    public class PutMessageViewModel : ModalViewModel
    {

        public PutMessageViewModel(IPutMessageView view, IApplicationController appc)
            : base(view, appc)
        {
            Title = "Put Message";
            Mode = PutMessageMode.Single;
            ResetCommand = CreateCommand(Reset);
            SelectFileCommand = CreateCommand(SelectFile);
            GenerateCorrelationIdCommand = CreateCommand(GenerateCorrelationId, () => SetCorrelationId);
            Progress = new RangeProgress();
        }

        public void Initialize(QueueInfo queue)
        {
            Queue = queue;
            Reset();
        }

        public override string OkLabel => "Put";

        public override string CancelLabel => "Close";

        public QueueInfo Queue { get; private set; }

        public ICommand ResetCommand { get; private set; }

        public ICommand SelectFileCommand { get; private set; }
        public ICommand GenerateCorrelationIdCommand { get; private set; }

        private void Reset()
        {
            MessageContent = null;
            Priority = App.UserSettings.PutPriority;
            CharacterSet = Queue.QueueManager.DefaultCharacterSet;
            Filename = null;
        }

        private void SelectFile()
        {
            FileType[] ft = { new FileType("Text File", ".txt"), new FileType("All", ".*") };

            var result = App.FileDialogService.ShowOpenFileDialog(ft, ft[0], string.Empty);
            if (result.IsValid)
            {
                Filename = result.FileName;
            }
        }

        private async Task EvaluateFileContentAsync()
        {
            LinesCountInFile = 0;

            if (!string.IsNullOrEmpty(Filename) && File.Exists(Filename))
            {

                FileIsValid = true;
                FileIsValid = await ExecuteGuardedAsync(() =>
                {
                    foreach (var line in ReadFile())
                    {
                        LinesCountInFile++;
                    }
                });
            }
            else
            {
                FileIsValid = null;
            }
        }

        private IEnumerable<string> ReadFile()
        {
            using (var sr = File.OpenText(Filename))
            {
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    if (SkipBlankLines && string.IsNullOrEmpty(line))
                        continue;
                    else
                        yield return line;
                }
            }
        }

        protected override bool OkAllowed()
        {

            if (Mode == PutMessageMode.Single)
                return HasErrors == false;

            if (Mode == PutMessageMode.FromFile)
                return FileIsValid.HasValue && FileIsValid.Value == true && LinesCountInFile > 0;

            return false;
        }

        private string _filename;
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                if (SetPropertyAndNotify(ref _filename, value))
                    StartEvaluateFileContent();
            }
        }

        private bool _skipBlankLines = true;

        public bool SkipBlankLines
        {
            get
            {
                return _skipBlankLines;
            }
            set
            {
                if (SetPropertyAndNotify(ref _skipBlankLines, value))
                    StartEvaluateFileContent();
            }
        }

        private void StartEvaluateFileContent()
        {
            var _ = EvaluateFileContentAsync();
        }
        private bool? _fileIsValid;

        public bool? FileIsValid
        {
            get { return _fileIsValid; }
            set { SetPropertyAndNotify(ref _fileIsValid, value); }
        }

        private int _linesCount;
        public int LinesCountInFile
        {
            get { return _linesCount; }
            set { SetPropertyAndNotify(ref _linesCount, value); }
        }

        private int _priority;
        [Required]
        [Range(1, 9)]
        public int Priority
        {
            get { return _priority; }
            set { SetPropertyAndNotify(ref _priority, value); }
        }

        private int _characterSet;
        [Required]
        public int CharacterSet
        {
            get { return _characterSet; }
            set { SetPropertyAndNotify(ref _characterSet, value); }
        }

        private string _messageContent;
        [Required(AllowEmptyStrings = false)]
        public string MessageContent
        {
            get { return _messageContent; }
            set { SetPropertyAndNotify(ref _messageContent, value); }
        }

        private string _messageInfo;
        public string MessageInfo
        {
            get { return _messageInfo; }
            private set { SetPropertyAndNotify(ref _messageInfo, value); }
        }
        public bool CloseAfterPut { get; set; }

        public PutMessageMode Mode { get; set; }

        public RangeProgress Progress { get; }

        string _correlationId;

        [CustomValidation(typeof(PutMessageViewModel), "IsCorrelationIdValid")]
        public string CorrelationId
        {
            get { return _correlationId; }
            set { SetPropertyAndNotify(ref _correlationId, value); }
        }


        public static ValidationResult IsCorrelationIdValid(string value, ValidationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var obj = (PutMessageViewModel)context.ObjectInstance;
            if (!obj.SetCorrelationId)
                return ValidationResult.Success;

            var isvalid = new Regex("^[a-fA-F0-9]{48}$").IsMatch(value);
            if (isvalid)
                return ValidationResult.Success;

            return new ValidationResult("Invalid correlation id");
        }

        bool _setCorrelationId;

        public bool SetCorrelationId
        {
            get { return _setCorrelationId; }
            set
            {
                if (SetPropertyAndNotify(ref _setCorrelationId, value))
                {
                    if (_setCorrelationId && string.IsNullOrEmpty(_correlationId))
                    {
                        GenerateCorrelationId();
                    }
                    ValidateProperties();
                }
            }
        }

        private void GenerateCorrelationId()
        {
            var uuid = Guid.NewGuid().ToByteArray(); // 16 bytes
            var result = new byte[24];
            for (int i = 0; i < 16; i++)
            {
                result[i] = uuid[i];
                if (i < 8)
                    result[i + 16] = uuid[i];
            }
            CorrelationId = result.ToHexString();
        }

        protected override void OnOk(CancelEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            e.Cancel = true;

            var _ = SendMessagesAsync();

        }

        private async Task SendMessagesAsync()
        {
            long msgSentCount = 0;
            MessageInfo = string.Empty;

            var result = await ExecuteGuardedAsync(() =>
            {
                if (Mode == PutMessageMode.Single)
                {
                    Queue.PutMessage(MessageContent, Priority, CharacterSet, SetCorrelationId ? CorrelationId : null, null, null);
                    msgSentCount = 1;
                }
                if (Mode == PutMessageMode.FromFile)
                {
                    using (var ps = Progress.Start(LinesCountInFile))
                    {
                        ps.SetTitle("Putting messages...");

                        Queue.PutMessages(ReadFile(), Priority, CharacterSet, SetCorrelationId ? CorrelationId : null, null, null, ps.CancellationToken, ps.Progress);
                    }
                    msgSentCount = LinesCountInFile;
                }

            });

            if (result)
            {
                if (!CloseAfterPut)
                {
                    MessageInfo = string.Format(CultureInfo.InvariantCulture, "{0} message put @{1:H:mm:ss}",
                        msgSentCount, DateTime.Now);
                }
                else
                {
                    Close();
                }
            }

        }


    }
}
