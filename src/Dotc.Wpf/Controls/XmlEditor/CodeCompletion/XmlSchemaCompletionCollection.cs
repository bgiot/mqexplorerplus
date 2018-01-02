


using System;
using System.Collections.ObjectModel;

//using ICSharpCode.Core;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	[Serializable()]
	public class XmlSchemaCompletionCollection : Collection<XmlSchemaCompletion>
	{
		public XmlSchemaCompletionCollection()
		{
		}
		
		public XmlSchemaCompletionCollection(XmlSchemaCompletionCollection schemas)
		{
			AddRange(schemas);
		}
		
		public XmlSchemaCompletionCollection(XmlSchemaCompletion[] schemas)
		{
			AddRange(schemas);
		}
		
		public XmlCompletionItemCollection GetNamespaceCompletion(string textUpToCursor)
		{
			var attrName = XmlParser.GetAttributeNameAtIndex(textUpToCursor, textUpToCursor.Length);
			if (attrName == "xmlns" || attrName.StartsWith("xmlns:", StringComparison.Ordinal)) {
				return GetNamespaceCompletion();
			}
			return new XmlCompletionItemCollection();
		}
		
		public XmlCompletionItemCollection GetNamespaceCompletion()
		{
			var completionItems = new XmlCompletionItemCollection();
			
			foreach (var schema in this) {
				var completionItem = new XmlCompletionItem(schema.NamespaceUri, XmlCompletionItemType.NamespaceUri);
				if (!completionItems.Contains(completionItem)) {
					completionItems.Add(completionItem);
				}
			}
			
			return completionItems;
		}
		
		/// <summary>
		///   Represents the <see cref='XmlSchemaCompletionData'/> entry with the specified namespace URI.
		/// </summary>
		/// <param name='namespaceUri'>The schema's namespace URI.</param>
		/// <value>The entry with the specified namespace URI.</value>
		public XmlSchemaCompletion this[string namespaceUri] => GetItem(namespaceUri);

	    public bool Contains(string namespaceUri)
		{
			return GetItem(namespaceUri) != null;
		}
		
		XmlSchemaCompletion GetItem(string namespaceUri)
		{
			foreach(var item in this) {
				if (item.NamespaceUri == namespaceUri) {
					return item;
				}
			}	
			return null;
		}		
		
		public void AddRange(XmlSchemaCompletion[] schema)
		{
			for (var i = 0; i < schema.Length; i++) {
				Add(schema[i]);
			}
		}
		
		public void AddRange(XmlSchemaCompletionCollection schemas)
		{
			for (var i = 0; i < schemas.Count; i++) {
				Add(schemas[i]);
			}
		}
		
        //public XmlSchemaCompletion GetSchemaFromFileName(string fileName)
        //{
        //    foreach (XmlSchemaCompletion schema in this) {
        //        if (FileUtility.IsEqualFileName(schema.FileName, fileName)) {
        //            return schema;
        //        }
        //    }
        //    return null;
        //}
		
		public XmlSchemaCompletionCollection GetSchemas(string namespaceUri)
		{
			var schemas = new XmlSchemaCompletionCollection();
			foreach (var schema in this) {
				if (schema.NamespaceUri == namespaceUri) {
					schemas.Add(schema);
				}
			}
			return schemas;
		}
		
		public XmlSchemaCompletionCollection GetSchemas(XmlElementPath path, XmlSchemaCompletion defaultSchema)
		{
			var namespaceUri = path.GetRootNamespace();
			if (String.IsNullOrEmpty(namespaceUri)) {
				return GetSchemaCollectionUsingDefaultSchema(path, defaultSchema);
			}
			return GetSchemas(namespaceUri);
		}
		
		XmlSchemaCompletionCollection GetSchemaCollectionUsingDefaultSchema(XmlElementPath path, XmlSchemaCompletion defaultSchema)
		{
			var schemas = new XmlSchemaCompletionCollection();
			if (defaultSchema != null) {
				path.SetNamespaceForUnqualifiedNames(defaultSchema.NamespaceUri);
				schemas.Add(defaultSchema);
			}
			return schemas;
		}
		
		public XmlCompletionItemCollection GetChildElementCompletion(XmlElementPath path, XmlSchemaCompletion defaultSchema)
		{
			var items = new XmlCompletionItemCollection();
			foreach (var schema in GetSchemas(path, defaultSchema)) {
				items.AddRange(schema.GetChildElementCompletion(path));
			}
			return items;
		}
		
		public XmlCompletionItemCollection GetElementCompletionForAllNamespaces(XmlElementPath path, XmlSchemaCompletion defaultSchema)
		{
			var pathsByNamespace = new XmlElementPathsByNamespace(path);
			return GetElementCompletion(pathsByNamespace, defaultSchema);
		}
		
		public XmlCompletionItemCollection GetElementCompletion(XmlElementPathsByNamespace pathsByNamespace, XmlSchemaCompletion defaultSchema)
		{
			var items = new XmlCompletionItemCollection();
			foreach (var path in pathsByNamespace) {
				items.AddRange(GetChildElementCompletion(path, defaultSchema));
			}
			
			var namespaceWithoutPaths = pathsByNamespace.NamespacesWithoutPaths;
			if (items.Count == 0) {
				if (!IsDefaultSchemaNamespaceDefinedInPathsByNamespace(namespaceWithoutPaths, defaultSchema)) {
					namespaceWithoutPaths.Add(defaultSchema.Namespace);
				}
			}

            if (pathsByNamespace.Count == 0)
            {
                items.AddRange(GetRootElementCompletion(namespaceWithoutPaths));
            }
			return items;
		}
		
		bool IsDefaultSchemaNamespaceDefinedInPathsByNamespace(XmlNamespaceCollection namespaces, XmlSchemaCompletion defaultSchema)
		{
			if (defaultSchema != null) {
				return namespaces.Contains(defaultSchema.Namespace);
			}
			return true;
		}
		
		public XmlCompletionItemCollection GetRootElementCompletion(XmlNamespaceCollection namespaces)
		{
			var items = new XmlCompletionItemCollection();
			foreach (var ns in namespaces) {
				foreach (var schema in GetSchemas(ns.Name)) {
					items.AddRange(schema.GetRootElementCompletion(ns.Prefix));
				}
			}
			return items;
		}
		
		public XmlCompletionItemCollection GetAttributeCompletion(XmlElementPath path, XmlSchemaCompletion defaultSchema)
		{
			var items = new XmlCompletionItemCollection();
			foreach (var schema in GetSchemas(path, defaultSchema)) {
				items.AddRange(schema.GetAttributeCompletion(path));
			}
			return items;
		}
		
		public XmlCompletionItemCollection GetElementCompletion(string textUpToCursor, XmlSchemaCompletion defaultSchema)
		{
			var parentPath = XmlParser.GetParentElementPath(textUpToCursor);
			return GetElementCompletionForAllNamespaces(parentPath, defaultSchema);
		}
		
		public XmlCompletionItemCollection GetAttributeCompletion(string textUpToCursor, XmlSchemaCompletion defaultSchema)
		{
			if (!XmlParser.IsInsideAttributeValue(textUpToCursor, textUpToCursor.Length)) {
				var path = XmlParser.GetActiveElementStartPath(textUpToCursor, textUpToCursor.Length);
				path.Compact();
				if (path.Elements.HasItems) {
					return GetAttributeCompletion(path, defaultSchema);
				}
			}
			return new XmlCompletionItemCollection();
		}
		
		public XmlCompletionItemCollection GetAttributeValueCompletion(char charTyped, string textUpToCursor, XmlSchemaCompletion defaultSchema)
		{
			if (XmlParser.IsAttributeValueChar(charTyped)) {
				var attributeName = XmlParser.GetAttributeName(textUpToCursor, textUpToCursor.Length);
				if (attributeName.Length > 0) {
					var elementPath = XmlParser.GetActiveElementStartPath(textUpToCursor, textUpToCursor.Length);
					return GetAttributeValueCompletion(elementPath, attributeName, defaultSchema);
				}
			}
			return new XmlCompletionItemCollection();
		}

		public XmlCompletionItemCollection GetAttributeValueCompletion(string text, int offset, XmlSchemaCompletion defaultSchema)
		{
			if (XmlParser.IsInsideAttributeValue(text, offset)) {
				var path = XmlParser.GetActiveElementStartPath(text, offset);
				var attributeName = XmlParser.GetAttributeNameAtIndex(text, offset);
				return GetAttributeValueCompletion(path, attributeName, defaultSchema);
			}
			return new XmlCompletionItemCollection();
		}
		
		public XmlCompletionItemCollection GetAttributeValueCompletion(XmlElementPath path, string attributeName, XmlSchemaCompletion defaultSchema)
		{
			path.Compact();
			var items = new XmlCompletionItemCollection();
			foreach (var schema in GetSchemas(path, defaultSchema)) {
				items.AddRange(schema.GetAttributeValueCompletion(path, attributeName));
			}
			return items;
		}
	}
}
