#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Views;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Dotc.MQExplorerPlus.Core.Models.Parser;
using Dotc.MQExplorerPlus.Core.Models.Parser.Configuration;
using ICSharpCode.AvalonEdit.Document;
using Dotc.MQExplorerPlus.Core.Controllers;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(ParsingEditorViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ParsingEditorViewModel : DocumentViewModel
    {

        public const string ID = "ParsingEditor";

        [ImportingConstructor]
        public ParsingEditorViewModel(IParsingEditorView view, IApplicationController appc) : base(view, appc)
        {

            BuildCommand();

            Title = "Parsing Editor";
            UniqueId = ParsingEditorViewModel.ID;

            ParserDefinitionDocument = new TextDocument();
            ParserDefinitionDocument.Text = "<parser>\n\t<parts/>\n\t<message>\n\t\t<field label=\"sample\" length=\"15\" />\n\t</message>\n</parser>";

            Engine = new ParserEngine();
        }

        public ICommand ParseCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public TextDocument ParserDefinitionDocument { get; private set; }

        public ParserEngine Engine { get; }

        private ParsingResult _parsingResult;
        public ParsingResult ParsingResult
        {
            get { return _parsingResult; }
            set
            {
                SetPropertyAndNotify(ref _parsingResult, value);
            }
        }

        private void BuildCommand()
        {
            ParseCommand = CreateCommand(
                () =>
                {
                    ParseSampleMessage();
                },
                () => CanTestParsing());

            LoadCommand = CreateCommand(
                () => LoadParserDefinition()
                );

            SaveCommand = CreateCommand(
                () => SaverParserDefinition()
                );

        }

        private void SaverParserDefinition()
        {
            try
            {
                var conf = ParserConfiguration.LoadFromString(ParserDefinitionDocument.Text);
                var result = App.FileDialogService.ShowSaveFileDialog(App.ShellService.ShellView,
                    ParserConfiguration.FILE_EXTENSIONS, ParserConfiguration.FILE_EXTENSIONS[0], string.Empty);
                if (result.IsValid)
                {
                    Execute(() =>
                    {
                        conf.SaveTo(result.FileName);
                    });
                }
            }
            catch (ParserException ex)
            {
                ShowErrorMessage(ex.Message);
            }

        }

        private void LoadParserDefinition()
        {
            var result = App.FileDialogService.ShowOpenFileDialog(App.ShellService.ShellView,
                ParserConfiguration.FILE_EXTENSIONS, ParserConfiguration.FILE_EXTENSIONS[0], string.Empty);
            if (result.IsValid)
            {
                Execute(() =>
                {
                    var conf = ParserConfiguration.Open(result.FileName);
                    ParserDefinitionDocument.Text = conf.GetRawData();
                });
            }
        }

        private void ParseSampleMessage()
        {
            Execute(() =>
            {
                Engine.Configuration = ParserConfiguration.LoadFromString(ParserDefinitionDocument.Text);
                Engine.ParseMessage(SampleMessage);
                ParsingResult = Engine.Result;
            });
        }

        private bool CanTestParsing()
        {
            return !string.IsNullOrWhiteSpace(SampleMessage);
        }

        private string _sampleMessage;
        public string SampleMessage
        {
            get { return _sampleMessage; }
            set
            {
                SetPropertyAndNotify(ref _sampleMessage, value);
            }
        }

    }
}
