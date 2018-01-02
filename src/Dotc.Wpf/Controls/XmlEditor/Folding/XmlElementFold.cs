


using System;
using System.Text;
using System.Xml;

namespace Dotc.Wpf.Controls.XmlEditor.Folding
{
	public class XmlElementFold
	{
		int _line = 0;
		int _column = 0;
		
		int _endLine = 0;
		int _endColumn = 0;
		
		string _prefix = String.Empty;
		string _name = String.Empty;
		string _qualifiedElementName = String.Empty;
		string _elementDisplayText = String.Empty;
		string _elementWithAttributesDisplayText = String.Empty;
		
		public void ReadStart(XmlTextReader reader)
		{
			// Take off 1 from the line position returned 
			// from the xml since it points to the start
			// of the element name and not the beginning 
			// tag.
			_column = reader.LinePosition - 1;
			_line = reader.LineNumber;
			
			_prefix = reader.Prefix;
			_name = reader.LocalName;
			
			GetQualifiedElementName();
			GetElementDisplayText(_qualifiedElementName);
			GetElementWithAttributesDisplayText(reader);
		}
		
		void GetQualifiedElementName()
		{
		    _qualifiedElementName = _prefix.Length > 0 ? $"{_prefix}:{_name}" : _name;
		}

	    void GetElementDisplayText(string qualifiedName)
		{
			_elementDisplayText = $"<{qualifiedName}>";
		}
		
		/// <summary>
		/// Gets the element's attributes as a string on one line that will
		/// be displayed when the element is folded.
		/// </summary>
		void GetElementWithAttributesDisplayText(XmlTextReader reader)
		{
		    var attributesDisplayText = GetAttributesDisplayText(reader);
		    _elementWithAttributesDisplayText = String.IsNullOrEmpty(attributesDisplayText) ? _elementDisplayText :
		        $"<{_qualifiedElementName} {attributesDisplayText}>";
		}

	    string GetAttributesDisplayText(XmlTextReader reader)
		{
			var text = new StringBuilder();
			
			for (var i = 0; i < reader.AttributeCount; ++i) {
				reader.MoveToAttribute(i);
				
				text.Append(reader.Name);
				text.Append("=");
				text.Append(reader.QuoteChar.ToString());
				text.Append(XmlEncodeAttributeValue(reader.Value, reader.QuoteChar));
				text.Append(reader.QuoteChar.ToString());
				
				// Append a space if this is not the
				// last attribute.
				if (!IsLastAttributeIndex(i, reader)) {
					text.Append(" ");
				}
			}
			
			return text.ToString();
		}
		
		/// <summary>
		/// Xml encode the attribute string since the string returned from
		/// the XmlTextReader is the plain unencoded string and .NET
		/// does not provide us with an xml encode method.
		/// </summary>
		static string XmlEncodeAttributeValue(string attributeValue, char quoteChar)
		{
			var encodedValue = new StringBuilder(attributeValue);
			
			encodedValue.Replace("&", "&amp;");
			encodedValue.Replace("<", "&lt;");
			encodedValue.Replace(">", "&gt;");
			
			if (quoteChar == '"') {
				encodedValue.Replace("\"", "&quot;");
			} else {
				encodedValue.Replace("'", "&apos;");
			}
			
			return encodedValue.ToString();
		}
		
		bool IsLastAttributeIndex(int attributeIndex, XmlTextReader reader)
		{
			 return attributeIndex == (reader.AttributeCount - 1);
		}
		
		public void ReadEnd(XmlTextReader reader)
		{
			_endLine = reader.LineNumber;
			var columnAfterEndTag = reader.LinePosition + _qualifiedElementName.Length + 1;
			_endColumn = columnAfterEndTag;
		}
		
		public FoldingRegion CreateFoldingRegion()
		{
			return CreateFoldingRegion(_elementDisplayText);
		}
		
		FoldingRegion CreateFoldingRegion(string displayText)
		{
			var region = new DomRegion(_line, _column, _endLine, _endColumn);
			return new FoldingRegion(displayText, region);
		}
		
		public FoldingRegion CreateFoldingRegionWithAttributes()
		{
			return CreateFoldingRegion(_elementWithAttributesDisplayText);
		}
		
		public bool IsSingleLine => _line == _endLine;
	}
}
