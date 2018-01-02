


using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	public class SchemaDocumentation
	{
		XmlSchemaAnnotation _annotation;
	    readonly StringBuilder _documentation = new StringBuilder();
	    readonly StringBuilder _documentationWithoutWhitespace = new StringBuilder();
		
		public SchemaDocumentation(XmlSchemaAnnotation annotation)
		{
			_annotation = annotation;
			if (annotation != null) {
				ReadDocumentationFromAnnotation(annotation.Items);
			}
		}
		
		void ReadDocumentationFromAnnotation(XmlSchemaObjectCollection annotationItems)
		{
			foreach (var schemaObject in annotationItems) {
				var schemaDocumentation = schemaObject as XmlSchemaDocumentation;
				if (schemaDocumentation != null) {
					ReadSchemaDocumentationFromMarkup(schemaDocumentation.Markup);
				}
			}
			RemoveWhitespaceFromDocumentation();
		}
		
		void ReadSchemaDocumentationFromMarkup(XmlNode[] markup)
		{
			foreach (var node in markup) {
				var textNode = node as XmlText;
				AppendTextToDocumentation(textNode);
			}
		}
		
		void AppendTextToDocumentation(XmlText textNode)
		{
		    if (textNode?.Data != null) {
		        _documentation.Append(textNode.Data);
		    }
		}

	    void RemoveWhitespaceFromDocumentation()
		{
			var lines = _documentation.ToString().Split('\n');
			RemoveWhitespaceFromLines(lines);
		}
		
		void RemoveWhitespaceFromLines(string[] lines)
		{
			foreach (var line in lines) {
				var lineWithoutWhitespace = line.Trim();
				_documentationWithoutWhitespace.AppendLine(lineWithoutWhitespace);
			}
		}
		
		public override string ToString()
		{
			return _documentationWithoutWhitespace.ToString().Trim();
		}
	}
}
