


using System;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;

namespace Dotc.Wpf.Controls.XmlEditor.Formatting
{
	/// <summary>
	/// Implements AvalonEdit's <see cref="IIndentationStrategy"/> by forwarding calls
	/// to a <see cref="IFormattingStrategy"/>.
	/// </summary>
	public class IndentationStrategyAdapter : IIndentationStrategy
	{
		readonly TextEditor _editor;
		readonly IFormattingStrategy _formattingStrategy;
		
		public IndentationStrategyAdapter(TextEditor editor, IFormattingStrategy formattingStrategy)
		{
			if (editor == null)
				throw new ArgumentNullException(nameof(editor));
			if (formattingStrategy == null)
				throw new ArgumentNullException(nameof(formattingStrategy));
			_editor = editor;
			_formattingStrategy = formattingStrategy;
		}
		
		public virtual void IndentLine(TextDocument document, DocumentLine line)
		{
			if (line == null)
				throw new ArgumentNullException(nameof(line));
			_formattingStrategy.IndentLine(_editor, _editor.Document.GetLineByNumber(line.LineNumber));
		}
		
		public virtual void IndentLines(TextDocument document, int beginLine, int endLine)
		{
			_formattingStrategy.IndentLines(_editor, beginLine, endLine);
		}
	}


}
