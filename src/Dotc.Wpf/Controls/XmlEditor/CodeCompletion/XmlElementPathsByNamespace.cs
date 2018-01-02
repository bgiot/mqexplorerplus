


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	public class XmlElementPathsByNamespace : Collection<XmlElementPath>
	{
	    readonly Dictionary<string, XmlElementPath> _pathsByNamespace = new Dictionary<string, XmlElementPath>();
	    readonly XmlNamespaceCollection _namespacesWithoutPaths = new XmlNamespaceCollection();
		
		public XmlElementPathsByNamespace(XmlElementPath path)
		{
		    if (path == null) throw new ArgumentNullException(nameof(path));
			SeparateIntoPathsByNamespace(path);
			AddSeparatedPathsToCollection();
			FindNamespacesWithoutAssociatedPaths(path.NamespacesInScope);
			_pathsByNamespace.Clear();
		}
		
		void SeparateIntoPathsByNamespace(XmlElementPath path)
		{
			foreach (var elementName in path.Elements) {
				var matchedPath = FindOrCreatePath(elementName.Namespace);
				matchedPath.AddElement(elementName);
			}
		}
		
		XmlElementPath FindOrCreatePath(string elementNamespace)
		{
			var path = FindPath(elementNamespace);
			if (path != null) {
				return path;
			}
			return CreatePath(elementNamespace);
		}
		
		XmlElementPath FindPath(string elementNamespace)
		{
			XmlElementPath path;
			if (_pathsByNamespace.TryGetValue(elementNamespace, out path)) {
				return path;
			}
			return null;
		}
		
		XmlElementPath CreatePath(string elementNamespace)
		{
			var path = new XmlElementPath();
			_pathsByNamespace.Add(elementNamespace, path);
			return path;
		}
		
		void AddSeparatedPathsToCollection()
		{
			foreach (var dictionaryEntry in _pathsByNamespace) {
				Add(dictionaryEntry.Value);
			}
		}
		
		void FindNamespacesWithoutAssociatedPaths(XmlNamespaceCollection namespacesInScope)
		{
			foreach (var ns in namespacesInScope) {
				if (!HavePathForNamespace(ns)) {
					_namespacesWithoutPaths.Add(ns);
				}
			}
		}
		
		bool HavePathForNamespace(XmlNamespace ns)
		{
			return _pathsByNamespace.ContainsKey(ns.Name);
		}
		
		public XmlNamespaceCollection NamespacesWithoutPaths => _namespacesWithoutPaths;
	}
}
