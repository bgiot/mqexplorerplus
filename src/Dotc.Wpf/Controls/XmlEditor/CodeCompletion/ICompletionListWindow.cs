


namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	/// <summary>
	/// Represents the completion window showing a ICompletionItemList.
	/// </summary>
	public interface ICompletionListWindow : ICompletionWindow
	{
		/// <summary>
		/// Gets/Sets the currently selected item.
		/// </summary>
		ICompletionItem SelectedItem { get; set; }
	}
}
