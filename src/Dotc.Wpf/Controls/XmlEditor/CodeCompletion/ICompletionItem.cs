


using System;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	public interface ICompletionItem
	{
		string Text { get; }
		string Description { get; }
        //IImage Image { get; }
		
		/// <summary>
		/// Performs code completion for the item.
		/// </summary>
		void Complete(CompletionContext context);
		
		/// <summary>
		/// Gets a priority value for the completion data item.
		/// When selecting items by their start characters, the item with the highest
		/// priority is selected first.
		/// </summary>
		double Priority {
			get;
		}
	}
	
	
	public class DefaultCompletionItem : ICompletionItem
	{
		public string Text { get; }
		public virtual string Description { get; set; }
        //public virtual IImage Image { get; set; }
		
		public virtual double Priority => 0;

	    public DefaultCompletionItem(string text)
		{
			Text = text;
		}
		
		public virtual void Complete(CompletionContext context)
		{
		    if (context == null) throw new ArgumentNullException(nameof(context));
			context.Editor.Document.Replace(context.StartOffset, context.Length, Text);
			// In case someone calls base.Complete() and then continues using the context, update EndOffset:
			context.EndOffset = context.StartOffset + Text.Length;
		}
	}
}
