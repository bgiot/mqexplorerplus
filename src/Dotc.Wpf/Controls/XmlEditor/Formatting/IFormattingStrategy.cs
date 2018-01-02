﻿


using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace Dotc.Wpf.Controls.XmlEditor.Formatting
{
	/// <summary>
	/// Indentation and formatting strategy.
	/// </summary>
	public interface IFormattingStrategy
	{
		/// <summary>
		/// This function formats a specific line after <code>charTyped</code> is pressed.
		/// </summary>
		void FormatLine(TextEditor editor, char charTyped);
		
		/// <summary>
		/// This function sets the indentation level in a specific line
		/// </summary>
		void IndentLine(TextEditor editor, DocumentLine line);
		
		/// <summary>
		/// This function sets the indentation in a range of lines.
		/// </summary>
		void IndentLines(TextEditor editor, int beginLine, int endLine);
		
		/// <summary>
		/// This function surrounds the selected text with a comment.
		/// </summary>
		void SurroundSelectionWithComment(TextEditor editor);
	}
	
	public class DefaultFormattingStrategy : IFormattingStrategy
	{
		internal static readonly DefaultFormattingStrategy DefaultInstance = new DefaultFormattingStrategy();
		
		public virtual void FormatLine(TextEditor editor, char charTyped)
		{
		}
		
		public virtual void IndentLine(TextEditor editor, DocumentLine line)
		{
			var document = editor.Document;
			var lineNumber = line.LineNumber;
			if (lineNumber > 1) {
				var previousLine = document.GetLineByNumber(lineNumber - 1);
				var indentation = DocumentUtilities.GetWhitespaceAfter(document, previousLine.Offset);
				// copy indentation to line
				var newIndentation = DocumentUtilities.GetWhitespaceAfter(document, line.Offset);
				document.Replace(line.Offset, newIndentation.Length, indentation);
			}
		}
		
		public virtual void IndentLines(TextEditor editor, int begin, int end)
		{
			//using (editor.Document.OpenUndoGroup()) {
				for (var i = begin; i <= end; i++) {
					IndentLine(editor, editor.Document.GetLineByNumber(i));
				}
			//}
		}
		
		public virtual void SurroundSelectionWithComment(TextEditor editor)
		{
		}
		
		/// <summary>
		/// Default implementation for single line comments.
		/// </summary>
		protected void SurroundSelectionWithSingleLineComment(TextEditor editor, string comment)
		{
			var document = editor.Document;
			//using (document.OpenUndoGroup()) {
				var startPosition = document.GetLocation(editor.SelectionStart);
				var endPosition = document.GetLocation(editor.SelectionStart + editor.SelectionLength);
				
				// endLine is one above endPosition if no characters are selected on the last line (e.g. line selection from the margin)
				var endLine = (endPosition.Column == 1 && endPosition.Line > startPosition.Line) ? endPosition.Line - 1 : endPosition.Line;
				
				var lines = new List<DocumentLine>();
				var removeComment = true;
				
				for (var i = startPosition.Line; i <= endLine; i++) {
					lines.Add(editor.Document.GetLineByNumber(i));
					if (!document.GetText(lines[i - startPosition.Line]).Trim().StartsWith(comment, StringComparison.Ordinal))
						removeComment = false;
				}
				
				foreach (var line in lines) {
					if (removeComment) {
						document.Remove(line.Offset + document.GetText(line).IndexOf(comment, StringComparison.Ordinal), comment.Length);
					} else {
                        document.Insert(line.Offset, comment); //, AnchorMovementType.BeforeInsertion);
					}
				}
			//}
		}
		
		/// <summary>
		/// Default implementation for multiline comments.
		/// </summary>
		protected void SurroundSelectionWithBlockComment(TextEditor editor, string blockStart, string blockEnd)
		{
			//using (editor.Document.OpenUndoGroup()) {
				var startOffset = editor.SelectionStart;
				var endOffset = editor.SelectionStart + editor.SelectionLength;
				
				if (editor.SelectionLength == 0) {
					var line = editor.Document.GetLineByOffset(editor.SelectionStart);
					startOffset = line.Offset;
					endOffset = line.Offset + line.Length;
				}
				
				var region = FindSelectedCommentRegion(editor, blockStart, blockEnd);
				
				if (region != null) {
					editor.Document.Remove(region.EndOffset, region.CommentEnd.Length);
					editor.Document.Remove(region.StartOffset, region.CommentStart.Length);
				} else {
					editor.Document.Insert(endOffset, blockEnd);
					editor.Document.Insert(startOffset, blockStart);
				}
			//}
		}
		
		public static BlockCommentRegion FindSelectedCommentRegion(TextEditor editor, string commentStart, string commentEnd)
		{
			var document = editor.Document;
			
			if (document.TextLength == 0) {
				return null;
			}
			
			// Find start of comment in selected text.
			
			var commentEndOffset = -1;
			var selectedText = editor.SelectedText;
			
			var commentStartOffset = selectedText.IndexOf(commentStart);
			if (commentStartOffset >= 0) {
				commentStartOffset += editor.SelectionStart;
			}

			// Find end of comment in selected text.
			
			commentEndOffset = commentStartOffset >= 0 ? selectedText.IndexOf(commentEnd, commentStartOffset + commentStart.Length - editor.SelectionStart) : selectedText.IndexOf(commentEnd);
			
			if (commentEndOffset >= 0) {
				commentEndOffset += editor.SelectionStart;
			}
			
			// Find start of comment before or partially inside the
			// selected text.
			
			var commentEndBeforeStartOffset = -1;
			if (commentStartOffset == -1) {
				var offset = editor.SelectionStart + editor.SelectionLength + commentStart.Length - 1;
				if (offset > document.TextLength) {
					offset = document.TextLength;
				}
				var text = document.GetText(0, offset);
				commentStartOffset = text.LastIndexOf(commentStart);
				if (commentStartOffset >= 0) {
					// Find end of comment before comment start.
					commentEndBeforeStartOffset = text.IndexOf(commentEnd, commentStartOffset, editor.SelectionStart - commentStartOffset);
					if (commentEndBeforeStartOffset > commentStartOffset) {
						commentStartOffset = -1;
					}
				}
			}
			
			// Find end of comment after or partially after the
			// selected text.
			
			if (commentEndOffset == -1) {
				var offset = editor.SelectionStart + 1 - commentEnd.Length;
				if (offset < 0) {
					offset = editor.SelectionStart;
				}
				var text = document.GetText(offset, document.TextLength - offset);
				commentEndOffset = text.IndexOf(commentEnd);
				if (commentEndOffset >= 0) {
					commentEndOffset += offset;
				}
			}
			
			if (commentStartOffset != -1 && commentEndOffset != -1) {
				return new BlockCommentRegion(commentStart, commentEnd, commentStartOffset, commentEndOffset);
			}
			
			return null;
		}
	}
	
	public class BlockCommentRegion
	{
		public string CommentStart { get; }
		public string CommentEnd { get; }
		public int StartOffset { get; }
		public int EndOffset { get; }
		
		/// <summary>
		/// The end offset is the offset where the comment end string starts from.
		/// </summary>
		public BlockCommentRegion(string commentStart, string commentEnd, int startOffset, int endOffset)
		{
			CommentStart = commentStart;
			CommentEnd = commentEnd;
			StartOffset = startOffset;
			EndOffset = endOffset;
		}
		
		public override int GetHashCode()
		{
			var hashCode = 0;
			unchecked {
				if (CommentStart != null) hashCode += 1000000007 * CommentStart.GetHashCode();
				if (CommentEnd != null) hashCode += 1000000009 * CommentEnd.GetHashCode();
				hashCode += 1000000021 * StartOffset.GetHashCode();
				hashCode += 1000000033 * EndOffset.GetHashCode();
			}
			return hashCode;
		}
		
		public override bool Equals(object obj)
		{
			var other = obj as BlockCommentRegion;
			if (other == null) return false;
			return CommentStart == other.CommentStart &&
				CommentEnd == other.CommentEnd &&
				StartOffset == other.StartOffset &&
				EndOffset == other.EndOffset;
		}
	}
}
