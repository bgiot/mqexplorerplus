using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Schema;
using Dotc.Wpf.Controls.XmlEditor.CodeCompletion;
using Dotc.Wpf.Controls.XmlEditor.Folding;
using Dotc.Wpf.Controls.XmlEditor.Formatting;

namespace Dotc.Wpf.Controls.XmlEditor
{
    /// <summary>
    /// Interaction logic for XmlEditor.xaml
    /// </summary>
    public partial class XmlEditor : TextEditor, IDisposable
    {

        private XmlFoldingManager _foldingManager;
        private readonly XmlFormattingStrategy _formattingStrategy;
        private readonly TextMarkerService _textMarkerService;
        private ToolTip _toolTip;

        public XmlCodeCompletionBinding CodeCompletion { get; set; }

        public XmlEditor()
            : base()
        {
            InitializeComponent();

            SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            ShowLineNumbers = true;

            WeakEventManager<ICSharpCode.AvalonEdit.Editing.TextArea, TextCompositionEventArgs>
                .AddHandler(TextArea, "TextEntering", TextArea_TextEntering);
            WeakEventManager<ICSharpCode.AvalonEdit.Editing.TextArea, TextCompositionEventArgs>
                .AddHandler(TextArea, "TextEntered", TextArea_TextEntered);
            WeakEventManager<XmlEditor, EventArgs>
                .AddHandler(this, "DocumentChanged", XmlEditor_DocumentChanged);

            TextArea.DefaultInputHandler.CommandBindings.Add(
                new CommandBinding(CustomCommands.CtrlSpaceCompletion, OnCodeCompletion));

            _formattingStrategy = new XmlFormattingStrategy();
            TextArea.IndentationStrategy = new IndentationStrategyAdapter(this, _formattingStrategy);

            _textMarkerService = new TextMarkerService(this);
            var textView = TextArea.TextView;
            textView.BackgroundRenderers.Add(_textMarkerService);
            textView.LineTransformers.Add(_textMarkerService);
            textView.Services.AddService(typeof(TextMarkerService), _textMarkerService);

            WeakEventManager<ICSharpCode.AvalonEdit.Rendering.TextView, MouseEventArgs>
                .AddHandler(textView, "MouseHover", MouseHover);
            WeakEventManager<ICSharpCode.AvalonEdit.Rendering.TextView, MouseEventArgs>
                .AddHandler(textView, "MouseHoverStopped", TextEditorMouseHoverStopped);
            WeakEventManager<ICSharpCode.AvalonEdit.Rendering.TextView, EventArgs>
                .AddHandler(textView, "VisualLinesChanged", VisualLinesChanged);

        }

        void XmlEditor_DocumentChanged(object sender, EventArgs e)
        {
            _foldingManager?.Dispose();
            if (Document != null)
            {
                Indent();
                _foldingManager = new XmlFoldingManager(this);
                _foldingManager.UpdateFolds();
                _foldingManager.Start();
            }

        }



        void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {

            if (e.Text.Length > 0 && !e.Handled)
            {

                if (e.Text.Length == 1)
                {
                    var c = e.Text[0];
                    if (c == '\r') c = '\n';

                    _formattingStrategy.FormatLine(this, c);

                    CodeCompletion?.HandleKeyPressed(this, c);
                }

            }
        }

        private SharpDevelopCompletionWindow CompletionWindow
        {
            get
            {
                return CodeCompletion?.CompletionWindow;
            }
        }

        void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (CompletionWindow != null || CodeCompletion == null)
                return;

            if (e.Handled)
                return;

            foreach (var c in e.Text)
            {

                var result = CodeCompletion.HandleKeyPress(this, c);
                if (result == CodeCompletionKeyPressResult.Completed)
                {
                    if (CompletionWindow != null)
                    {
                        // a new CompletionWindow was shown, but does not eat the input
                        // tell it to expect the text insertion
                        CompletionWindow.ExpectInsertionBeforeStart = true;
                    }
                    return;
                }
                else if (result == CodeCompletionKeyPressResult.CompletedIncludeKeyInCompletion)
                {
                    if (CompletionWindow != null && CompletionWindow?.StartOffset == CompletionWindow.EndOffset)
                    {
                        CompletionWindow.CloseWhenCaretAtBeginning = true;
                    }
                    return;
                }
                else if (result == CodeCompletionKeyPressResult.EatKey)
                {
                    e.Handled = true;
                    return;
                }

            }
        }

        void OnCodeCompletion(object sender, ExecutedRoutedEventArgs e)
        {
            CompletionWindow?.Close();


            if (CodeCompletion!= null && CodeCompletion.CtrlSpace(this))
            {
                e.Handled = true;
            }

        }


        private new void MouseHover(object sender, MouseEventArgs e)
        {
            if (Document == null)
            {
                return;
            }
            var pos = TextArea.TextView.GetPositionFloor(e.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);
            var inDocument = pos.HasValue;
            if (inDocument)
            {
                var logicalPosition = pos.Value.Location;
                var offset = Document.GetOffset(logicalPosition);

                var markersAtOffset = _textMarkerService.GetMarkersAtOffset(offset);
                var markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);

                if (markerWithToolTip != null)
                {
                    if (_toolTip == null)
                    {
                        _toolTip = new ToolTip();
                        WeakEventManager<ToolTip, RoutedEventArgs>
                            .AddHandler(_toolTip, "Closed", ToolTipClosed);
                        _toolTip.PlacementTarget = this;
                        _toolTip.Content = new TextBlock
                        {
                            Text = markerWithToolTip.ToolTip,
                            TextWrapping = TextWrapping.Wrap
                        };
                        _toolTip.IsOpen = true;
                        e.Handled = true;
                    }
                }
            }
        }

        void ToolTipClosed(object sender, RoutedEventArgs e)
        {
            _toolTip = null;
        }

        void TextEditorMouseHoverStopped(object sender, MouseEventArgs e)
        {
            if (_toolTip != null)
            {
                _toolTip.IsOpen = false;
                e.Handled = true;
            }
        }

        private void VisualLinesChanged(object sender, EventArgs e)
        {
            if (_toolTip != null)
            {
                _toolTip.IsOpen = false;
            }
        }


        public void Indent()
        {
            _formattingStrategy.IndentLines(this, 1, Document.LineCount);

        }

        public void ValidateXml()
        {

            IServiceProvider sp = this;
            var markerService = (TextMarkerService)sp.GetService(typeof(TextMarkerService));
            markerService.Clear();

            try
            {

                var settings = new XmlReaderSettings {IgnoreWhitespace = false};
                if (CodeCompletion != null)
                {
                    settings.Schemas.Add(CodeCompletion.Schema);
                }
                settings.ValidationType = ValidationType.Schema;

                using (var xrdr = XmlReader.Create(new StringReader(Document.Text), settings))
                {
                    while (xrdr.Read())
                    {
                    }
                }

                Indent();
            }
            catch (XmlException ex)
            {
                DisplayValidationError(ex.Message, ex.LinePosition, ex.LineNumber);
            }
            catch (XmlSchemaValidationException ex)
            {
                DisplayValidationError(ex.Message, ex.LinePosition, ex.LineNumber);
            }
        }

        public ValidationEventHandler XmlValidationEvent { get; set; }

        private void DisplayValidationError(string message, int linePosition, int lineNumber)
        {
            if (lineNumber >= 1 && lineNumber <= Document.LineCount)
            {
                var offset = Document.GetOffset(new TextLocation(lineNumber, linePosition));
                var endOffset = TextUtilities.GetNextCaretPosition(Document, offset, System.Windows.Documents.LogicalDirection.Forward, CaretPositioningMode.WordBorderOrSymbol);
                if (endOffset < 0)
                {
                    endOffset = Document.TextLength;
                }
                var length = endOffset - offset;

                if (length < 2)
                {
                    length = Math.Min(2, Document.TextLength - offset);
                }

                _textMarkerService.Create(offset, length, message);
            }
        }



        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _foldingManager?.Dispose();
            }
        }
    }


}
