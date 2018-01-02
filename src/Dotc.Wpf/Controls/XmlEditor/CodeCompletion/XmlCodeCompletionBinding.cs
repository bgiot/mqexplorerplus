


using System;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using System.Xml.Schema;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System.Windows;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
    public class XmlCodeCompletionBinding : ICodeCompletionBinding
    {
        readonly XmlSchemaCompletionCollection _schemas;

        public XmlCodeCompletionBinding(string xsdResourceName)
        {

            if (xsdResourceName == null) throw new ArgumentNullException(nameof(xsdResourceName));

            _schemas = new XmlSchemaCompletionCollection();

            var rsRefs = xsdResourceName.Split(';');

            var rsAssembly = rsRefs.Length == 2 ? Assembly.Load(rsRefs[1]) : Assembly.GetExecutingAssembly();

            var st = rsAssembly.GetManifestResourceStream(rsRefs[0]);

            if (st != null)
                using (var reader = new System.IO.StreamReader(st))
                {
                    _schemas.Add(new XmlSchemaCompletion(reader));
                }
        }

        public XmlSchema Schema => _schemas[0].Schema;

        readonly char[] _ignoredChars = new[] { '\\', '/', '"', '\'', '=', '>', '!', '?' };

        public CodeCompletionKeyPressResult HandleKeyPress(TextEditor editor, char ch)
        {
            return CodeCompletionKeyPressResult.None;
        }

        XmlCompletionItemCollection GetCompletionItems(TextEditor editor, XmlSchemaCompletion defaultSchema)
        {
            var offset = editor.TextArea.Caret.Offset;
            var textUpToCursor = editor.Document.GetText(0, offset);

            var items = new XmlCompletionItemCollection();
            if (XmlParser.IsInsideAttributeValue(textUpToCursor, offset))
            {
                items = _schemas.GetNamespaceCompletion(textUpToCursor);
                if (items.Count == 0)
                    items = _schemas.GetAttributeValueCompletion(textUpToCursor, editor.TextArea.Caret.Offset, defaultSchema);
            }
            else
            {
                items = _schemas.GetAttributeCompletion(textUpToCursor, defaultSchema);
                if (items.Count == 0)
                    items = _schemas.GetElementCompletion(textUpToCursor, defaultSchema);
            }
            return items;
        }


        public bool CtrlSpace(TextEditor editor)
        {
            if (editor == null) throw new ArgumentNullException(nameof(editor));

            var elementStartIndex = XmlParser.GetActiveElementStartIndex(editor.Document.Text, editor.TextArea.Caret.Offset);
            if (elementStartIndex <= -1)
                return false;
            if (ElementStartsWith("<!", elementStartIndex, editor.Document))
                return false;
            if (ElementStartsWith("<?", elementStartIndex, editor.Document))
                return false;

            var defaultSchema = _schemas[0];

            var completionItems = GetCompletionItems(editor, defaultSchema);
            if (completionItems.HasItems)
            {
                completionItems.Sort();
                var identifier = XmlParser.GetXmlIdentifierBeforeIndex(editor.Document, editor.TextArea.Caret.Offset);
                completionItems.PreselectionLength = identifier.Length;
                var completionWindow = ShowCompletionWindow(editor, completionItems);
                if (completionWindow != null)
                {
                    SetCompletionWindowWidth(completionWindow, completionItems);
                }

                return true;
            }
            return false;
        }


        private void SetCompletionWindowWidth(ICompletionListWindow completionWindow, XmlCompletionItemCollection completionItems)
        {
            var firstListItem = completionItems[0];
            if (firstListItem.DataType == XmlCompletionItemType.NamespaceUri)
            {
                completionWindow.Width = double.NaN;
            }
        }

        public SharpDevelopCompletionWindow CompletionWindow { get; set; }

        private void CloseExistingCompletionWindow()
        {
            CompletionWindow?.Close();
        }


        private ICompletionListWindow ShowCompletionWindow(TextEditor editor, XmlCompletionItemCollection completionItems)
        {
            if (completionItems == null || completionItems.HasItems == false)
                return null;

            var window = new SharpDevelopCompletionWindow(editor, editor.TextArea, completionItems);

            CloseExistingCompletionWindow();
            CompletionWindow = window;
            WeakEventManager<SharpDevelopCompletionWindow, EventArgs>
                .AddHandler(window, "Closed", CompletionWindow_Closed);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                delegate
                {
                    if (CompletionWindow == window)
                    {
                        window.Show();
                    }
                }
            ));

            return window;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CompletionWindow_Closed(object sender, EventArgs args)
        {
            CompletionWindow = null;
        }

        bool ElementStartsWith(string text, int elementStartIndex, TextDocument document)
        {
            var textLength = Math.Min(text.Length, document.TextLength - elementStartIndex);
            return document.GetText(elementStartIndex, textLength).Equals(text, StringComparison.OrdinalIgnoreCase);
        }

        public bool HandleKeyPressed(TextEditor editor, char ch)
        {
            if (editor == null) throw new ArgumentNullException(nameof(editor));
            if (char.IsWhiteSpace(ch) || editor.SelectionLength > 0)
                return false;
            if (_ignoredChars.Contains(ch))
                return false;
            if (XmlParser.GetXmlIdentifierBeforeIndex(editor.Document, editor.TextArea.Caret.Offset).Length > 1)
                return false;
            return CtrlSpace(editor);
        }
    }
}
