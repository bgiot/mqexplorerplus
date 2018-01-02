


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	public class XmlNamespaceCollection : Collection<XmlNamespace>
	{
		public XmlNamespaceCollection()
		{
		}
		
		public XmlNamespace[] ToArray()
		{
			var namespaces = new List<XmlNamespace>(this);
			return namespaces.ToArray();
		}
		
		public string GetNamespaceForPrefix(string prefix)
		{
			foreach (var ns in this) {
				if (ns.Prefix == prefix) {
					return ns.Name;
				}
			}
			return String.Empty;
		}
		
		public string GetPrefix(string namespaceToMatch)
		{
			foreach (var ns in this) {
				if (ns.Name == namespaceToMatch)  {
					return ns.Prefix;
				}
			}
			return String.Empty;
		}
	}
}
