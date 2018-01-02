


namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	/// <summary>
	/// Represents the path to an xml element starting from the root of the
	/// document.
	/// </summary>
	public class XmlElementPath
	{
	    readonly QualifiedNameCollection _elements = new QualifiedNameCollection();
	    readonly XmlNamespaceCollection _namespacesInScope = new XmlNamespaceCollection();
		
		public XmlElementPath()
		{
		}
		
		/// <summary>
		/// Gets the elements specifying the path.
		/// </summary>
		/// <remarks>The order of the elements determines the path.</remarks>
		public QualifiedNameCollection Elements => _elements;

	    public void AddElement(QualifiedName elementName)
		{
			_elements.Add(elementName);
		}
		
		public bool IsEmpty => _elements.IsEmpty;

	    /// <summary>
		/// Compacts the path so it only contains the elements that are from 
		/// the namespace of the last element in the path. 
		/// </summary>
		/// <remarks>This method is used when we need to know the path for a
		/// particular namespace and do not care about the complete path.
		/// </remarks>
		public void Compact()
		{
			if (_elements.HasItems) {
				var lastName = Elements.GetLast();
				var index = LastIndexNotMatchingNamespace(lastName.Namespace);
				if (index != -1) {
					_elements.RemoveFirst(index + 1);
				}
			}
		}
		
		public XmlNamespaceCollection NamespacesInScope => _namespacesInScope;

	    public string GetNamespaceForPrefix(string prefix)
		{
			return _namespacesInScope.GetNamespaceForPrefix(prefix);
		}
		
		/// <summary>
		/// An xml element path is considered to be equal if 
		/// each path item has the same name and namespace.
		/// </summary>
		public override bool Equals(object obj) 
		{
			var rhsPath = obj as XmlElementPath;			
			if (rhsPath == null) {
				return false;
			}
			
			return _elements.Equals(rhsPath._elements);
		}
		
		public override int GetHashCode() 
		{
			return _elements.GetHashCode();
		}
		
		public override string ToString()
		{
			return _elements.ToString();
		}
		
		public string GetRootNamespace() 
		{
			return _elements.GetRootNamespace();
		}
		
		/// <summary>
		/// Only updates those names without a namespace.
		/// </summary>
		public void SetNamespaceForUnqualifiedNames(string namespaceUri)
		{
			foreach (var name in _elements) {
				if (!name.HasNamespace) {
					name.Namespace = namespaceUri;
				}
			}
		}
				
		int LastIndexNotMatchingNamespace(string namespaceUri)
		{
			if (_elements.Count > 1) {
				// Start the check from the last but one item.
				for (var i = _elements.Count - 2; i >= 0; --i) {
					var name = _elements[i];
					if (name.Namespace != namespaceUri) {
						return i;
					}
				}
			}
			return -1;
		}
	}
}
