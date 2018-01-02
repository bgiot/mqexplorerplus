


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	[Serializable()]
	public class XmlCompletionItemCollection : Collection<XmlCompletionItem>, ICompletionItemList
	{
	    readonly List<char> _normalKeys = new List<char>();

		public XmlCompletionItemCollection()
		{
			_normalKeys.AddRange(new char[] { ':', '.', '_' });
		}
		
		public XmlCompletionItemCollection(XmlCompletionItemCollection items)
			: this()
		{
			AddRange(items);
		}
		
		public XmlCompletionItemCollection(XmlCompletionItem[] items)
			: this()
		{
			AddRange(items);
		}
		
		public bool HasItems => Count > 0;

	    public bool ContainsAllAvailableItems => true;

	    public void Sort()
		{
			var items = base.Items as List<XmlCompletionItem>;
			items.Sort();
		}
		
		public void AddRange(XmlCompletionItem[] items)
		{
		    if (items == null) throw new ArgumentNullException(nameof(items));
			for (var i = 0; i < items.Length; i++) {
				if (!Contains(items[i].Text)) {
					Add(items[i]);
				}
			}
		}
		
		public void AddRange(XmlCompletionItemCollection item)
		{
            if (item == null) throw new ArgumentNullException(nameof(item));

            for (var i = 0; i < item.Count; i++) {
				if (!Contains(item[i].Text)) {
					Add(item[i]);
				}
			}
		}
		
		public bool Contains(string name)
		{			
			foreach (var data in this)
			{
			    if (data.Text?.Length > 0) {
			        if (data.Text == name) {
			            return true;
			        }
			    }
			}
		    return false;
		}
		
		/// <summary>
		/// Gets a count of the number of occurrences of a particular name
		/// in the completion data.
		/// </summary>
		public int GetOccurrences(string name)
		{
			var count = 0;
			
			foreach (var item in this) {
				if (item.Text == name) {
					++count;
				}
			}
			
			return count;
		}
		
		/// <summary>
		/// Checks whether the completion item specified by name has
		/// the correct description.
		/// </summary>
		public bool ContainsDescription(string name, string description)
		{
			foreach (var item in this) {
				if (item.Text == name) {
					if (item.Description == description) {
						return true;
					}
				}
			}				
			return false;
		}
		
		public XmlCompletionItem[] ToArray()
		{
			var data = new XmlCompletionItem[Count];
			CopyTo(data, 0);
			return data;
		}
		
		public CompletionItemListKeyResult ProcessInput(char key)
		{
			if (key == '!' || key == '?')
				return CompletionItemListKeyResult.Cancel;
			if (char.IsLetterOrDigit(key))
				return CompletionItemListKeyResult.NormalKey;
			if (_normalKeys.Contains(key))
				return CompletionItemListKeyResult.NormalKey;
			return CompletionItemListKeyResult.InsertionKey;
		}
		
		IEnumerable<ICompletionItem> ICompletionItemList.Items => this;

	    public ICompletionItem SuggestedItem {
			get { 
				if (HasItems && PreselectionLength == 0) {
					return this[0];
				}
				return null;
			}
		}
		
		public int PreselectionLength { get; set; }
		public int PostselectionLength { get; set; }
		
		public void Complete(CompletionContext context, ICompletionItem item)
		{
		    if (item == null) throw new ArgumentNullException(nameof(item));
			item.Complete(context);
		}
	}
}
