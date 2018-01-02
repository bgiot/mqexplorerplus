


using System;
using System.Xml;

namespace Dotc.Wpf.Controls.XmlEditor.Folding
{
	public class XmlCommentFold
	{
		string[] _lines = new string[0];
		string _comment = String.Empty;
		DomRegion _commentRegion;
		string _displayText;
		
		const string CommentStartTag = "<!--";
		const string CommentEndTag = "-->";
		
		public XmlCommentFold(XmlTextReader reader)
		{
			ReadComment(reader);
		}
		
		void ReadComment(XmlTextReader reader)
		{
			GetCommentLines(reader);
			GetCommentRegion(reader, _lines);
			GetCommentDisplayText(_lines);
		}
		
		void GetCommentLines(XmlTextReader reader)
		{
			_comment = reader.Value.Replace("\r\n", "\n");
			_lines = _comment.Split('\n');
		}
		
		void GetCommentRegion(XmlTextReader reader, string[] lines)
		{
			var startColumn = reader.LinePosition - CommentStartTag.Length;
			var startLine = reader.LineNumber;
			
			var lastLine = lines[lines.Length - 1];
			var endColumn = lastLine.Length + startColumn + CommentEndTag.Length;
			var endLine = startLine + lines.Length - 1;
			
			_commentRegion = new DomRegion(startLine, startColumn, endLine, endColumn);
		}
		
		void GetCommentDisplayText(string[] lines)
		{
			var firstLine = String.Empty;
			if (lines.Length > 0) {
				firstLine = lines[0];
			}
			_displayText = String.Concat("<!-- ", firstLine.Trim(), " -->");
		}
		
		public FoldingRegion CreateFoldingRegion()
		{
			return new FoldingRegion(_displayText, _commentRegion);
		}
		
		public bool IsSingleLine => _commentRegion.BeginLine == _commentRegion.EndLine;
	}
}
