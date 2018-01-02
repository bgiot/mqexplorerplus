


using System;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	/// <summary>
	/// The type of text held in this object.
	/// </summary>
	public enum XmlCompletionItemType {
		None = 0,
		XmlElement = 1,
		XmlAttribute = 2,
		NamespaceUri = 3,
		XmlAttributeValue = 4
	}
	
	/// <summary>
	/// Holds the text for  namespace, child element or attribute
	/// autocomplete (intellisense).
	/// </summary>
	public class XmlCompletionItem : DefaultCompletionItem, IComparable<XmlCompletionItem>
	{
	    readonly XmlCompletionItemType _dataType = XmlCompletionItemType.XmlElement;
	    readonly string _description = String.Empty;
		
		public XmlCompletionItem(string text)
			: this(text, String.Empty, XmlCompletionItemType.XmlElement)
		{
		}
		
		public XmlCompletionItem(string text, string description)
			: this(text, description, XmlCompletionItemType.XmlElement)
		{
		}

		public XmlCompletionItem(string text, XmlCompletionItemType dataType)
			: this(text, String.Empty, dataType)
		{
		}

		public XmlCompletionItem(string text, string description, XmlCompletionItemType dataType)
			: base(text)
		{
			_description = description;
			_dataType = dataType;
		}
		
		/// <summary>
		/// Returns the xml item's documentation as retrieved from
		/// the xs:annotation/xs:documentation element.
		/// </summary>
		public override string Description => _description;

	    public XmlCompletionItemType DataType => _dataType;

	    public override void Complete(CompletionContext context)
		{
			base.Complete(context);
			
			switch (_dataType) {
				case XmlCompletionItemType.XmlAttribute:
					context.Editor.Document.Insert(context.EndOffset, "=\"\"");
					context.Editor.TextArea.Caret.Offset--;
//					XmlCodeCompletionBinding.Instance.CtrlSpace(context.Editor);
					break;
			}
		}
		
		public override string ToString()
		{
			return "[" + Text + "]";
		}
		
		public override int GetHashCode()
		{
			return _dataType.GetHashCode() ^ Text.GetHashCode();
		}
		
		public override bool Equals(object obj)
		{
			var item = obj as XmlCompletionItem;
			if (item != null) {
				return (_dataType == item._dataType) && (Text == item.Text);
			}
			return false;
		}
		
		public int CompareTo(XmlCompletionItem other)
		{

		    return string.Compare(Text, other?.Text, StringComparison.Ordinal); //  Text.CompareTo(other.Text);
		}
	}
}
