



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace Dotc.Wpf.Controls.XmlEditor.Formatting
{
	/// <summary>
	/// This class currently inserts the closing tags to typed openening tags
	/// and does smart indentation for xml files.
	/// </summary>
	public class XmlFormattingStrategy : DefaultFormattingStrategy
	{
		public override void FormatLine(TextEditor editor, char charTyped)
		{
			//editor.Document.StartUndoableAction();
			try {
				if (charTyped == '>') {
					var stringBuilder = new StringBuilder();
					var offset = Math.Min(editor.TextArea.Caret.Offset - 2, editor.Document.TextLength - 1);
					while (true) {
						if (offset < 0) {
							break;
						}
						var ch = editor.Document.GetCharAt(offset);
						if (ch == '<') {
							var reversedTag = stringBuilder.ToString().Trim();
							if (!reversedTag.StartsWith("/", StringComparison.Ordinal) && !reversedTag.EndsWith("/", StringComparison.Ordinal)) {
								var validXml = true;
								try {
									var doc = new XmlDocument();
									doc.LoadXml(editor.Document.Text);
								} catch (XmlException) {
									validXml = false;
								}
								// only insert the tag, if something is missing
								if (!validXml) {
									var tag = new StringBuilder();
									for (var i = reversedTag.Length - 1; i >= 0 && !Char.IsWhiteSpace(reversedTag[i]); --i) {
										tag.Append(reversedTag[i]);
									}
									var tagString = tag.ToString();
									if (tagString.Length > 0 && !tagString.StartsWith("!", StringComparison.Ordinal) && !tagString.StartsWith("?", StringComparison.Ordinal)) {
										var caretOffset = editor.TextArea.Caret.Offset;
										editor.Document.Insert(editor.TextArea.Caret.Offset, "</" + tagString + ">");
										editor.TextArea.Caret.Offset = caretOffset;
									}
								}
							}
							break;
						}
						stringBuilder.Append(ch);
						--offset;
					}
				}
			} catch (Exception e) { // Insanity check
				Debug.Assert(false, e.ToString());
			}
			if (charTyped == '\n') {
				IndentLine(editor, editor.Document.GetLineByNumber(editor.TextArea.Caret.Line));
			}
			//editor.Document.EndUndoableAction();
		}
		
		public override void IndentLine(TextEditor editor, DocumentLine line)
		{
			//editor.Document.StartUndoableAction();
			try {
				TryIndent(editor, line.LineNumber, line.LineNumber);
			} catch (XmlException ) {
                //LoggingService.Debug(ex.ToString());
			} finally {
				//editor.Document.EndUndoableAction();
			}
		}
		
		/// <summary>
		/// This function sets the indentlevel in a range of lines.
		/// </summary>
		public override void IndentLines(TextEditor editor, int begin, int end)
		{
			//editor.Document.StartUndoableAction();
			try {
				TryIndent(editor, begin, end);
			} catch (XmlException) {
                //LoggingService.Debug(ex.ToString());
			} finally {
				//editor.Document.EndUndoableAction();
			}
		}
		
		public override void SurroundSelectionWithComment(TextEditor editor)
		{
			SurroundSelectionWithBlockComment(editor, "<!--", "-->");
		}
		
		static void TryIndent(TextEditor editor, int begin, int end)
		{
			var currentIndentation = "";
			var tagStack = new Stack<string>();
			var document = editor.Document;
			
			var tab = editor.Options.IndentationString;
			var nextLine = begin; // in #dev coordinates
			var wasEmptyElement = false;
			var lastType = XmlNodeType.XmlDeclaration;
			using (var stringReader = new StringReader(document.Text)) {
			    var r = new XmlTextReader(stringReader) {XmlResolver = null};
			    // prevent XmlTextReader from loading external DTDs
			    while (r.Read()) {
					if (wasEmptyElement)
					{
					    wasEmptyElement = false;
					    currentIndentation = tagStack.Count == 0 ? "" : tagStack.Pop();
					}
				    if (r.NodeType == XmlNodeType.EndElement) {
						currentIndentation = tagStack.Count == 0 ? "" : tagStack.Pop();
					}
					
					while (r.LineNumber >= nextLine) {
						if (nextLine > end) break;
						if (lastType == XmlNodeType.CDATA || lastType == XmlNodeType.Comment) {
							nextLine++;
							continue;
						}
						// set indentation of 'nextLine'
						var line = document.GetLineByNumber(nextLine);
						var lineText = document.GetText(line);
						
						string newText;
						// special case: opening tag has closing bracket on extra line: remove one indentation level
						if (lineText.Trim() == ">")
							newText = tagStack.Peek() + lineText.Trim();
						else
							newText = currentIndentation + lineText.Trim();
						
						document.SmartReplaceLine(line, newText);
						nextLine++;
					}
					if (r.LineNumber > end)
						break;
					wasEmptyElement = r.NodeType == XmlNodeType.Element && r.IsEmptyElement;
					string attribIndent = null;
					if (r.NodeType == XmlNodeType.Element) {
						tagStack.Push(currentIndentation);
						if (r.LineNumber < begin)
							currentIndentation = DocumentUtilities.GetIndentation(editor.Document, r.LineNumber);
						if (r.Name.Length < 16)
							attribIndent = currentIndentation + new string(' ', 2 + r.Name.Length);
						else
							attribIndent = currentIndentation + tab;
						currentIndentation += tab;
					}
					lastType = r.NodeType;
					if (r.NodeType == XmlNodeType.Element && r.HasAttributes) {
						var startLine = r.LineNumber;
						r.MoveToAttribute(0); // move to first attribute
						if (r.LineNumber != startLine)
							attribIndent = currentIndentation; // change to tab-indentation
						r.MoveToAttribute(r.AttributeCount - 1);
						while (r.LineNumber >= nextLine) {
							if (nextLine > end) break;
							// set indentation of 'nextLine'
							var line = document.GetLineByNumber(nextLine);
							var newText = attribIndent + document.GetText(line).Trim();
							document.SmartReplaceLine(line, newText);
							nextLine++;
						}
					}
				}
				//r.Close();
			}
		}
	}
}
