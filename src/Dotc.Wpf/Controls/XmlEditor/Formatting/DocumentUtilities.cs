﻿


using System;
using System.Diagnostics;
using System.Windows.Documents;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace Dotc.Wpf.Controls.XmlEditor.Formatting
{
	/// <summary>
	/// Extension methods for ITextEditor and IDocument.
	/// </summary>
	public static class DocumentUtilities
	{
		

		public static void ClearSelection(this TextEditor editor)
		{
			editor.Select(editor.Document.GetOffset(editor.TextArea.Caret.Location), 0);
		}
		
		/// <summary>
		/// Gets the word in front of the caret.
		/// </summary>
		public static string GetWordBeforeCaret(this TextEditor editor)
		{
			if (editor == null)
				throw new ArgumentNullException(nameof(editor));
			var endOffset = editor.TextArea.Caret.Offset;
			var startOffset = FindPrevWordStart(editor.Document, endOffset);
			if (startOffset < 0)
				return string.Empty;
			else
				return editor.Document.GetText(startOffset, endOffset - startOffset);
		}
		
		static readonly char[] WhitespaceChars = {' ', '\t'};
		
		/// <summary>
		/// Replaces the text in a line.
		/// If only whitespace at the beginning and end of the line was changed, this method
		/// only adjusts the whitespace and doesn't replace the other text.
		/// </summary>
		public static void SmartReplaceLine(this TextDocument document, DocumentLine line, string newLineText)
		{
			if (document == null)
				throw new ArgumentNullException(nameof(document));
			if (line == null)
				throw new ArgumentNullException(nameof(line));
			if (newLineText == null)
				throw new ArgumentNullException(nameof(newLineText));
			var newLineTextTrim = newLineText.Trim(WhitespaceChars);
			var oldLineText = document.GetText(line);
			if (oldLineText == newLineText)
				return;
			var pos = oldLineText.IndexOf(newLineTextTrim, StringComparison.Ordinal);
			if (newLineTextTrim.Length > 0 && pos >= 0) {
				//using (document.OpenUndoGroup()) {
					// find whitespace at beginning
					var startWhitespaceLength = 0;
					while (startWhitespaceLength < newLineText.Length) {
						var c = newLineText[startWhitespaceLength];
						if (c != ' ' && c != '\t')
							break;
						startWhitespaceLength++;
					}
					// find whitespace at end
					var endWhitespaceLength = newLineText.Length - newLineTextTrim.Length - startWhitespaceLength;
					
					// replace whitespace sections
					var lineOffset = line.Offset;
					document.Replace(lineOffset + pos + newLineTextTrim.Length, line.Length - pos - newLineTextTrim.Length, newLineText.Substring(newLineText.Length - endWhitespaceLength));
					document.Replace(lineOffset, pos, newLineText.Substring(0, startWhitespaceLength));
				//}
			} else {
				document.Replace(line.Offset, line.Length, newLineText);
			}
		}
		
		/// <summary>
		/// Finds the first word start in the document before offset.
		/// </summary>
		/// <returns>The offset of the word start, or -1 if there is no word start before the specified offset.</returns>
		public static int FindPrevWordStart(this ITextSource textSource, int offset)
		{
			return TextUtilities.GetNextCaretPosition(textSource, offset, LogicalDirection.Backward, CaretPositioningMode.WordStart);
		}
		
		/// <summary>
		/// Finds the first word start in the document before offset.
		/// </summary>
		/// <returns>The offset of the word start, or -1 if there is no word start before the specified offset.</returns>
		public static int FindNextWordStart(this ITextSource textSource, int offset)
		{
			return TextUtilities.GetNextCaretPosition(textSource, offset, LogicalDirection.Forward, CaretPositioningMode.WordStart);
		}
		
		/// <summary>
		/// Gets the word at the specified position.
		/// </summary>
		public static string GetWordAt(this ITextSource document, int offset)
		{
			if (offset < 0 || offset >= document.TextLength || !IsWordPart(document.GetCharAt(offset))) {
				return String.Empty;
			}
			var startOffset = offset;
			var endOffset   = offset;
			while (startOffset > 0 && IsWordPart(document.GetCharAt(startOffset - 1))) {
				--startOffset;
			}
			
			while (endOffset < document.TextLength - 1 && IsWordPart(document.GetCharAt(endOffset + 1))) {
				++endOffset;
			}
			
			Debug.Assert(endOffset >= startOffset);
			return document.GetText(startOffset, endOffset - startOffset + 1);
		}
		
		static bool IsWordPart(char ch)
		{
			return char.IsLetterOrDigit(ch) || ch == '_';
		}
		
		public static string GetIndentation(TextDocument document, int line)
		{
			return GetWhitespaceAfter(document, document.GetLineByNumber(line).Offset);
		}

        /// <summary>
        /// Gets all indentation starting at offset.
        /// </summary>
        /// <param name="textSource">The document.</param>
        /// <param name="offset">The offset where the indentation starts.</param>
        /// <returns>The indentation text.</returns>
        public static string GetWhitespaceAfter(ITextSource textSource, int offset)
		{
			var segment = TextUtilities.GetWhitespaceAfter(textSource, offset);
			return textSource.GetText(segment.Offset, segment.Length);
		}

        /// <summary>
        /// Gets all indentation before the offset.
        /// </summary>
        /// <param name="textSource">The document.</param>
        /// <param name="offset">The offset where the indentation ends.</param>
        /// <returns>The indentation text.</returns>
        public static string GetWhitespaceBefore(ITextSource textSource, int offset)
		{
			var segment = TextUtilities.GetWhitespaceBefore(textSource, offset);
			return textSource.GetText(segment.Offset, segment.Length);
		}
		
		/// <summary>
		/// Gets the line terminator for the document around the specified line number.
		/// </summary>
		public static string GetLineTerminator(TextDocument document, int lineNumber)
		{
			var line = document.GetLineByNumber(lineNumber);
			if (line.DelimiterLength == 0) {
				// at the end of the document, there's no line delimiter, so use the delimiter
				// from the previous line
				if (lineNumber == 1)
					return Environment.NewLine;
				line = document.GetLineByNumber(lineNumber - 1);
			}
			return document.GetText(line.Offset + line.Length, line.DelimiterLength);
		}
		
		public static string NormalizeNewLines(string input, string newLine)
		{
			return input.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", newLine);
		}
		
		public static string NormalizeNewLines(string input, TextDocument document, int lineNumber)
		{
			return NormalizeNewLines(input, GetLineTerminator(document, lineNumber));
		}
		
		public static void InsertNormalized(this TextDocument document, int offset, string text)
		{
			if (document == null)
				throw new ArgumentNullException(nameof(document));
			var line = document.GetLineByOffset(offset);
			text = NormalizeNewLines(text, document, line.LineNumber);
			document.Insert(offset, text);
		}
		
		#region ITextSource implementation
		[Obsolete("We now directly use ITextSource everywhere, no need for adapters")]
		public static ITextSource GetTextSource(ITextSource textBuffer)
		{
			return textBuffer;
		}
		#endregion
	}
}
