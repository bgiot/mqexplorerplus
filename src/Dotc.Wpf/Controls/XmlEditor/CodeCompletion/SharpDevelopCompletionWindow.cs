


using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	/// <summary>
	/// The code completion window.
	/// </summary>
	public partial class SharpDevelopCompletionWindow : CompletionWindow, ICompletionListWindow
	{
		public ICompletionItem SelectedItem {
			get {
				return ((CodeCompletionDataAdapter)CompletionList.SelectedItem).Item;
			}
			set {
				var itemAdapters = CompletionList.CompletionData.Cast<CodeCompletionDataAdapter>();
				CompletionList.SelectedItem = itemAdapters.FirstOrDefault(a => a.Item == value);
			}
		}
		
		double ICompletionWindow.Width {
			get { return Width; }
			set {
				// Disable virtualization if we use automatic width - this prevents the window from resizing
				// when the user scrolls.
				//VirtualizingPanel.SetIsVirtualizing(this.CompletionList.ListBox, !double.IsNaN(value));
				Width = value;
				if (double.IsNaN(value)) {
					// enable size-to-width:
					if (SizeToContent == SizeToContent.Manual)
						SizeToContent = SizeToContent.Width;
					else if (SizeToContent == SizeToContent.Height)
						SizeToContent = SizeToContent.WidthAndHeight;
				} else {
					// disable size-to-width:
					if (SizeToContent == SizeToContent.Width)
						SizeToContent = SizeToContent.Manual;
					else if (SizeToContent == SizeToContent.WidthAndHeight)
						SizeToContent = SizeToContent.Height;
				}
			}
		}
		
		readonly ICompletionItemList _itemList;
		
		public ICompletionItemList ItemList => _itemList;

	    public TextEditor Editor { get; }
		
		public SharpDevelopCompletionWindow(TextEditor editor, TextArea textArea, ICompletionItemList itemList) : base(textArea)
		{
			if (editor == null)
				throw new ArgumentNullException(nameof(editor));
			if (itemList == null)
				throw new ArgumentNullException(nameof(itemList));
			
			Editor = editor;
			_itemList = itemList;
			var suggestedItem = itemList.SuggestedItem;
			foreach (var item in itemList.Items) {
				ICompletionData adapter = new CodeCompletionDataAdapter(this, item);
				CompletionList.CompletionData.Add(adapter);
				if (item == suggestedItem)
					CompletionList.SelectedItem = adapter;
			}
			StartOffset -= itemList.PreselectionLength;
			EndOffset += itemList.PostselectionLength;
		}
		
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			if (_itemList.PreselectionLength > 0 && _itemList.SuggestedItem == null) {
				var preselection = TextArea.Document.GetText(StartOffset, EndOffset - StartOffset);
				CompletionList.SelectItem(preselection);
			}
		}
		
		protected override void OnTextInput(TextCompositionEventArgs e)
		{
		    if (e == null) throw new ArgumentNullException(nameof(e));
			base.OnTextInput(e);
			if (!e.Handled) {
				foreach (var c in e.Text) {
					switch (_itemList.ProcessInput(c)) {
						case CompletionItemListKeyResult.BeforeStartKey:
							ExpectInsertionBeforeStart = true;
							break;
						case CompletionItemListKeyResult.NormalKey:
							break;
						case CompletionItemListKeyResult.InsertionKey:
							CompletionList.RequestInsertion(e);
							return;
						case CompletionItemListKeyResult.Cancel:
							Close();
							return;
					}
				}
			}
		}
	}
	
	sealed class CodeCompletionDataAdapter : ICompletionData, INotifyPropertyChanged
	{
		readonly SharpDevelopCompletionWindow _window;
		readonly ICompletionItem _item;
		//readonly IFancyCompletionItem fancyCompletionItem;
		
		public CodeCompletionDataAdapter(SharpDevelopCompletionWindow window, ICompletionItem item)
		{
			if (window == null)
				throw new ArgumentNullException(nameof(window));
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			_window = window;
			_item = item;
			//this.fancyCompletionItem = item as IFancyCompletionItem;
		}
		
		public ICompletionItem Item => _item;

	    public string Text => _item.Text;

	    public object Content => _item.Text;

	    public object Description => string.IsNullOrWhiteSpace(_item.Description) ? null : _item.Description;

	    public ImageSource Image => null;

	    public double Priority => _item.Priority;

	    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			var context = new CompletionContext {
				Editor = _window.Editor,
				StartOffset = _window.StartOffset,
				EndOffset = _window.EndOffset
			};
			var txea = insertionRequestEventArgs as TextCompositionEventArgs;
			var kea = insertionRequestEventArgs as KeyEventArgs;
			if (txea != null && txea.Text.Length > 0)
				context.CompletionChar = txea.Text[0];
			else if (kea != null && kea.Key == Key.Tab)
				context.CompletionChar = '\t';
			_window.ItemList.Complete(context, _item);
			if (context.CompletionCharHandled && txea != null)
				txea.Handled = true;
		}
		
		// This is required to work around http://support.microsoft.com/kb/938416/en-us
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
			add { }
			remove { }
		}
	}
}
