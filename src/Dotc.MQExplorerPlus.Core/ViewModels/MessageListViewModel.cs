#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using Dotc.Common;
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Models.Parser;
using Dotc.MQExplorerPlus.Core.Models.Parser.Configuration;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Mvvm;
using static System.FormattableString;
using System.ComponentModel.DataAnnotations;
using Nito.AsyncEx;
using Dotc.Wpf.Controls.HexViewer;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(MessageListViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class MessageListViewModel : DocumentViewModel, IKeyboardCommands, IStatusInfo
    {
        private int? _browseLimit;
        private MessageInfo _currentMessage;

        private object _syncLock = new object();

        [ImportingConstructor]
        public MessageListViewModel(IMessageListView view, IApplicationController appc)
            : base(view, appc)
        {

            Parser = new ParserEngine();

            _browseLimit = App.UserSettings.BrowseLimit;

            Messages = new SelectableItemCollection<MessageInfo>();
            BindingOperations.EnableCollectionSynchronization(Messages, _syncLock);

            Progress = new RangeProgress();

            Filter = new BrowseFilter();

            BuildCommands();


            AvailableConverters = new List<LabelValuePair<IByteCharConverter>>
            {
                new LabelValuePair<IByteCharConverter> { Label = "CP-1252", Value = new DefaultByteCharConverter() },
                new LabelValuePair<IByteCharConverter> { Label = "EBCDIC", Value = new EbcdicByteCharConverter() }
            };

            _currentConverter = AvailableConverters[0].Value;

            StatusInfoViewModel = new MessageListStatusInfo(this);

        }

        private IByteCharConverter _currentConverter;

        public List<LabelValuePair<IByteCharConverter>> AvailableConverters { get; private set; }

        public IByteCharConverter CurrentConverter
        {
            get { return _currentConverter; }
            set
            {
                if (SetPropertyAndNotify(ref _currentConverter, value))
                {
                    UpdateMessagesWithNewConverter();
                }
            }
        }

        private void UpdateMessagesWithNewConverter()
        {
            if (MemorySearchCache != null)
            {
                foreach (var msg in MemorySearchCache)
                {
                    msg.ChangeConverter(_currentConverter);
                }
            }
            else if (Messages != null)
            {
                foreach (var msg in Messages)
                {
                    msg.ChangeConverter(_currentConverter);
                }
            }
        }

        public ShellService ShellService { get { return App.ShellService; } }

        public ParserEngine Parser { get; }

        private QueueInfo _queue;
        public QueueInfo Queue
        {
            get { return _queue; }
            private set { SetPropertyAndNotify(ref _queue, value); }
        }

        public SelectableItemCollection<MessageInfo> Messages { get; }

        public RangeProgress Progress { get; }

        public MessageInfo CurrentMessage
        {
            get { return _currentMessage; }
            set
            {
                SetPropertyAndNotify(ref _currentMessage, value);
                if (Parser.Configuration != null)
                {
                    Parser.ParseMessage(_currentMessage?.Text);
                }
            }
        }

        [Range(1, int.MaxValue)]
        [Required(AllowEmptyStrings = false)]
        public int? BrowseLimit
        {
            get { return _browseLimit; }
            set
            {
                SetPropertyAndNotify(ref _browseLimit, value);
            }
        }

        public BrowseFilter Filter
        {
            get;
        }

        private bool _initialized;
        public bool Initialized
        {
            get { return _initialized; }
            private set
            {
                SetPropertyAndNotify(ref _initialized, value);
            }
        }

        public void StartInitialize(IQueue queue)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));
            var _ = InitializeAsync(queue);
        }
        private async Task InitializeAsync(IQueue queue)
        {

            Queue = new QueueInfo(queue.NewConnection(), App.UserSettings);

            ((MessageListStatusInfo)StatusInfoViewModel).ConnectionInformation = queue.QueueManager.ConnectionInfo;

            Initialized = true;

            await RefreshAsync(false);

        }

        private async Task SearchAsync()
        {
            await ApplyFilterAsync();
        }

        public ICommand F5Command => RefreshCommand;
        public ICommand CtlF5Command => new DisabledCommand();

        private void BuildCommands()
        {
            RefreshCommand = new AwaitableRelayCommand(
                async () => await RefreshAsync(false), () => Initialized && !HasErrors && LocalIdle);

            RefreshContinueCommand = new AwaitableRelayCommand(
                async () => await RefreshAsync(true), () => Initialized && !HasErrors && Messages.Count > 0 && LocalIdle);

            SearchCommand = new AwaitableRelayCommand(
                async () => await SearchAsync(), () => Initialized && !HasErrors && Messages.Count > 0 && LocalIdle);

            SelectAllCommand = CreateCommand(
            () =>
            {
                foreach (var m in Messages)
                {
                    m.Selected = true;
                }
            }, () => Initialized);

            SelectNoneCommand = CreateCommand(
                () =>
                {
                    foreach (var m in Messages)
                    {
                        m.Selected = false;
                    }
                }, () => Initialized);

            InvertSelectionCommand = CreateCommand(
                () =>
                {
                    foreach (var m in Messages)
                    {
                        m.Selected = !m.Selected;
                    }
                }, () => Initialized);

            DeleteCommand = new AwaitableRelayCommand(
                async () => await DeleteMessagesAsync(GetSelectedMessages())
                , () => Initialized && Messages.SelectedCount > 0
                );

            TruncateCommand = new AwaitableRelayCommand(
                async () => await EmptyQueueAsync(true)
                , () => Initialized && Queue != null && Queue.SupportTruncate && Messages.TotalCount > 0);

            EmptyCommand = new AwaitableRelayCommand(
                () => EmptyQueueAsync(false)
                , () => Initialized && Messages.TotalCount > 0);

            ForwardCommand = new AwaitableRelayCommand(
                async () => await ForwardMessagesAsync(GetSelectedMessages())
                , () => Initialized && Messages.SelectedCount > 0);

            ExportCommand = new AwaitableRelayCommand(
                () => AskExportMessagesAsync()
                , () => Initialized && Messages.SelectedCount > 0);

            SetGetAllowCommand = CreateCommand(
                () => Queue.SetGetStatus(GetPutStatus.Allowed),
                () => Initialized && Queue != null && Queue.GetStatus == GetPutStatus.Inhibited);

            SetGetInhibitCommand = CreateCommand(
                () => Queue.SetGetStatus(GetPutStatus.Inhibited),
                () => Initialized && Queue != null && Queue.GetStatus == GetPutStatus.Allowed);

            SetPutAllowCommand = CreateCommand(
                () => Queue.SetPutStatus(GetPutStatus.Allowed),
                () => Initialized && Queue != null && Queue.PutStatus == GetPutStatus.Inhibited);

            SetPutInhibitCommand = CreateCommand(
                () => Queue.SetPutStatus(GetPutStatus.Inhibited),
                () => Initialized && Queue != null && Queue.PutStatus == GetPutStatus.Allowed);

            CopyRawDataToClipboardCommand = CreateCommand(
                () => CopyRawDataToClipboard(CurrentMessage.Text),
                () => CurrentMessage != null);

            PutMessageCommand = CreateCommand(
                OpenPutMessageView,
                () => Initialized && Queue?.PutStatus != null && Queue.PutStatus.Value == GetPutStatus.Allowed);

            UnloadQueueCommand = new AwaitableRelayCommand(
                async () => await AskDumpQueueAsync(),
                () => Initialized && Queue?.GetStatus != null);

            LoadQueueCommand = new AwaitableRelayCommand(
                async () => await AskLoadDumpAsync(),
                () => Initialized && Queue?.PutStatus != null);

            LoadParserConfigurationCommand = new AwaitableRelayCommand(
                async () => await LoadParserConfigurationAsync()
                , () => Initialized);
        }

        private async Task LoadParserConfigurationAsync()
        {
            var result = App.FileDialogService.ShowOpenFileDialog(App.ShellService.ShellView,
                ParserConfiguration.FILE_EXTENSIONS, ParserConfiguration.FILE_EXTENSIONS[0], string.Empty);
            if (result.IsValid)
            {

                await ExecuteAsync(() =>
               {
                   Parser.Configuration = ParserConfiguration.Open(result.FileName);
                   var xmlDoc = new XmlDocument();
                   xmlDoc.LoadXml(Parser.Configuration.GetRawData());
                   ParserDefinitionDocument = xmlDoc;
                   ParserDefinitionFilename = Path.GetFileName(result.FileName);
                   if (CurrentMessage != null)
                   {
                       Parser.ParseMessage(CurrentMessage.Text);
                   }
               });
            }
        }

        private XmlDocument _parserDefinitionDocument;
        public XmlDocument ParserDefinitionDocument
        {
            get { return _parserDefinitionDocument; }
            set { SetPropertyAndNotify(ref _parserDefinitionDocument, value); }
        }

        private string _parserDefinitionFilename;
        public string ParserDefinitionFilename
        {
            get { return _parserDefinitionFilename; }
            set { SetPropertyAndNotify(ref _parserDefinitionFilename, value); }
        }

        private async Task AskDumpQueueAsync()
        {
            await Task.Yield();
            Queue.RefreshInfo();
            App.OpenDumpCreationSettingsView(Queue, async (s, p) =>
            {
                await DumpQueueAsync(s, p);
            });
        }

        private async Task DumpQueueAsync(string filename, DumpCreationSettings settings)
        {

            await ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(0, Queue.Depth ?? 0, ct, LongRunningState.Yes))
                {
                    ps.SetTitle("Generating queue dump...");

                    using (var context = new DumpCreationContext(filename, settings))
                    {
                        Queue.QueueSource.DumpEngine.CreateDump(context, ps.CancellationToken, ps.Progress);

                    }
                }
                App.MessageService.ShowMessage(App.ShellService.ShellView, Invariant($"File '{filename}' generated."));

            });

            if (settings.LeaveMessages)
            {
                Queue.RefreshInfo();
            }
            else
            {
                await RefreshAsync(false);
            }
        }

        private async Task AskLoadDumpAsync()
        {
            await Task.Yield();
            App.OpenDumpLoadSettingsView(Queue, async (s, p) =>
            {
                await LoadDumpAsync(s, p);
            });
        }
        private async Task LoadDumpAsync(string filename, DumpLoadSettings settings)
        {

            bool reload = false;
            await ExecuteAsync((ct) =>
            {
                var loadDone = false;
                var countMsgLoaded = 0;
                var dumpIsInvalid = false;
                string error;

                using (var ps = Progress.Start(ct, LongRunningState.Yes))
                {
                    ps.SetTitle("Checking dump...");
                    int previewCount = 0;

                    if (Queue.QueueSource.DumpEngine.CheckDumpIsValid(filename, ps.CancellationToken, out previewCount, out error))
                    {
                        ps.SetTitle("Loading dump...");
                        ps.SetRange(0, previewCount);

                        using (var context = new DumpLoadContext(filename, settings))
                        {
                            countMsgLoaded = Queue.QueueSource.DumpEngine.LoadDump(context, ps.CancellationToken, ps.Progress);
                        }

                        loadDone = true;
                    }
                    else
                    {
                        if (!ps.CancellationToken.IsCancellationRequested)
                        {
                            dumpIsInvalid = true;
                        }
                    }
                }

                if (loadDone)
                {
                    App.MessageService.ShowMessage(App.ShellService.ShellView,
                        Invariant($"{countMsgLoaded} message(s) loaded."));
                    reload = true;
                }
                else
                {
                    if (dumpIsInvalid)
                    {
                        ShowErrorMessage($"Invalid dump file!\n{error}");
                    }
                }

            });
            if (reload)
            {
                await RefreshAsync(false);
            }


        }

        private void OpenPutMessageView()
        {
            App.OpenPutMessageView(Queue);
        }

        private void CopyRawDataToClipboard(string text)
        {
            var r = ClipboardHelper.PushStringToClipboard(text.Replace(Convert.ToChar(0x0).ToString(), ""));
            if (!r.OK)
            {
                ShowErrorMessage(Invariant($"Failed to copy to clipboard.\n({r.ResultCode})"));
            }
        }

        private async Task EmptyQueueAsync(bool truncate)
        {
            var action = truncate ? "TRUNCATE command" : "DELETE command";
            var msg = Invariant($"You are going to empty the queue ({action}).\n\nAre you sure?");

            if (App.MessageService.ShowYesNoQuestion(App.ShellService.ShellView, msg))
            {
                await ExecuteAsync((ct) =>
                {
                    Queue.Empty(truncate);
                });
                await RefreshAsync(false);
            }
        }

        private async Task AskExportMessagesAsync()
        {
            await Task.Yield();
            var msgs = GetSelectedMessages();
            App.OpenExportMessagesSettingsView(Queue, msgs.Count, async (s, p) =>
            {
                await ExportMessagesAsync(msgs, s, p);
            });
        }



        private async Task ExportMessagesAsync(IList<MessageInfo> list, string filename, CsvExportSettings settings)
        {

            await ExecuteAsync((ct) =>
            {

                using (var ps = Progress.Start(list.Count, ct, LongRunningState.Yes))
                {

                    ps.SetTitle("Exporting selected messages...");
                    using (var ctx = new CsvExportContext(filename, settings))
                    {
                        Queue.QueueSource.DumpEngine.ExportToCsv(ctx, list.Select(x => x.MessageSource),
                            ps.CancellationToken, ps.Progress);
                    }
                }
                App.MessageService.ShowMessage(App.ShellService.ShellView,
                    Invariant($"File '{filename}' generated."));
            });

        }

        private async Task DeleteMessagesAsync(IList<MessageInfo> list)
        {

            var msg = Invariant($"You are going to delete {list.Count} message(s).\nAre you sure?");

            if (App.MessageService.ShowYesNoQuestion(App.ShellService.ShellView, msg))
            {
                await ExecuteAsync((ct) =>
                    {

                        using (var ps = Progress.Start(list.Count, ct, LongRunningState.Yes))
                        {
                            ps.SetTitle("Deleting messages...");

                            Queue.DeleteMessages(list, ps.CancellationToken, ps.Progress);
                        }
                    });
                await RefreshAsync(false);
            }

        }


        private async Task ForwardMessagesAsync(IList<MessageInfo> list)
        {
            await Task.Yield();
            App.SelectQueue(Queue.QueueManager, async (destQ) => await ForwardMessagesAsync(list, destQ));
        }

        private async Task ForwardMessagesAsync(IList<MessageInfo> list, IQueue destinationQ)
        {
            await ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(list.Count, ct, LongRunningState.Yes))
                {
                    ps.SetTitle("Forwarding messages...");

                    Queue.ForwardMessages(list, destinationQ, ps.CancellationToken, ps.Progress);

                }
            });
            await RefreshAsync(false);
        }

        private IList<MessageInfo> GetSelectedMessages()
        {
            return Messages.Where(m => m.Selected).ToList();
        }

        public ICommand RefreshCommand { get; private set; }
        public ICommand RefreshContinueCommand { get; private set; }

        public ICommand SearchCommand { get; private set; }
        public ICommand SelectAllCommand { get; private set; }
        public ICommand SelectNoneCommand { get; private set; }
        public ICommand InvertSelectionCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand EmptyCommand { get; private set; }
        public ICommand TruncateCommand { get; private set; }
        public ICommand ForwardCommand { get; private set; }
        public ICommand SetGetAllowCommand { get; private set; }
        public ICommand SetGetInhibitCommand { get; private set; }
        public ICommand SetPutAllowCommand { get; private set; }
        public ICommand SetPutInhibitCommand { get; private set; }
        public ICommand CopyRawDataToClipboardCommand { get; private set; }
        public ICommand PutMessageCommand { get; private set; }
        public ICommand UnloadQueueCommand { get; private set; }
        public ICommand LoadQueueCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand LoadParserConfigurationCommand { get; private set; }

        public StatusInfoViewModel StatusInfoViewModel { get; private set; }

        private MessageInfo[] _cacheForInmemorySearch;

        public bool ResultsetFromCache
        {
            get { return _cacheForInmemorySearch != null; }
        }
        public MessageInfo[] MemorySearchCache
        {
            get { return _cacheForInmemorySearch; }
            set
            {
                SetPropertyAndNotify(ref _cacheForInmemorySearch, value);
                OnPropertyChanged(nameof(ResultsetFromCache));
            }
        }
        private async Task RefreshAsync(bool browseFromLastFound)
        {
            await ExecuteAsync((ct) =>
            {
                Queue.RefreshInfo();

                if (BrowseLimit.HasValue && BrowseLimit.Value > 0)
                {

                    MessageInfo lastMessage = null;
                    if (browseFromLastFound && Messages.Any())
                    {
                        lastMessage = Messages.Last();
                    }
                    else
                    {
                        Messages.Clear();
                    }
                    MemorySearchCache = null;
                    using (var ps = Progress.Start(BrowseLimit.Value, ct))
                    {
                        using (var internalProgress = ObservableProgress<int>.CreateForUi((count) =>
                        {
                            ps.SetTitle($"Browsing messages... [{count}]");
                        }))
                        {
                            ps.SetTitle("Browsing messages...");

                            foreach (var m in Queue.Browse(BrowseLimit.Value, Filter, ps.CancellationToken, internalProgress, lastMessage, CurrentConverter))
                            {
                                Messages.Add(m);
                                ps.ReportNext();
                            }
                        }
                    }
                    StatusInfoViewModel.LastUpdateTimestamp = DateTime.Now;
                }
            });
        }

        private async Task ApplyFilterAsync()
        {
            await ExecuteAsync((ct) =>
            {
                using (var ps = Progress.Start(Messages.Count, ct))
                {
                    if (MemorySearchCache == null)
                    {
                        MemorySearchCache = new MessageInfo[Messages.Count];
                        Messages.CopyTo(MemorySearchCache, 0);
                    }
                    Messages.Clear();
                    int i = 0;
                    while (i < MemorySearchCache.Length)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }

                        if (Filter.IsMatch(MemorySearchCache[i].MessageSource))
                        {
                            Messages.Add(MemorySearchCache[i]);
                        }

                        i++;
                        ps.ReportNext();
                    }
                }
            });

        }
    }

    public class MessageListStatusInfo : StatusInfoViewModel
    {
        internal MessageListStatusInfo(MessageListViewModel mlvm) : base()
        {
            Owner = mlvm;
            WeakEventManager<SelectableItemCollection<MessageInfo>, PropertyChangedEventArgs>
                .AddHandler(Owner.Messages, "CountChanged", Messages_CountChanged);

        }

        public MessageListViewModel Owner { get; private set; }

        private void Messages_CountChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TotalCount")
            {
                OnPropertyChanged(nameof(TotalCount));
            }
            if (e.PropertyName == "SelectedCount")
            {
                OnPropertyChanged(nameof(SelectedCount));
            }
        }

        private string _connectionInformation;
        public string ConnectionInformation
        {
            get { return _connectionInformation; }
            set
            {
                SetPropertyAndNotify(ref _connectionInformation, value);
            }
        }

        public int SelectedCount { get { return Owner.Messages.SelectedCount; } }
        public int TotalCount { get { return Owner.Messages.TotalCount; } }

        public RangeProgress Progress { get { return Owner.Progress; } }

        public CountdownService Countdown
        {
            get { return Owner.ShellService.Countdown; }
        }
    }
}

