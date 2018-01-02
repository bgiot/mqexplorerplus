


using System;
using System.Xml;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	/// <summary>
	/// An <see cref="XmlQualifiedName"/> with the namespace prefix.
	/// </summary>
	/// <remarks>
	/// The namespace prefix active for a namespace is 
	/// needed when an element is inserted via autocompletion. This
	/// class just adds this extra information alongside the 
	/// <see cref="XmlQualifiedName"/>.
	/// </remarks>
	public class QualifiedName
	{
		XmlQualifiedName _xmlQualifiedName = XmlQualifiedName.Empty;
		string _prefix = String.Empty;
		
		public QualifiedName()
		{
		}
		
		public QualifiedName(string name, string namespaceUri)
			: this(name, namespaceUri, String.Empty)
		{
		}
		
		public QualifiedName(string name, XmlNamespace ns)
			: this(name, ns.Name, ns.Prefix)
		{
		}
		
		public QualifiedName(string name, string namespaceUri, string prefix)
		{
			_xmlQualifiedName = new XmlQualifiedName(name, namespaceUri);
			_prefix = prefix;
		}
		
		public static bool operator ==(QualifiedName lhs, QualifiedName rhs)
		{
			var lhsObject = (object)lhs;
			var rhsObject = (object)rhs;
			if ((lhsObject != null) && (rhsObject != null)) {
				return lhs.Equals(rhs);
			} else if ((lhsObject == null) && (rhsObject == null)) {
				return true;
			}		
			return false;
		}
		
		public static bool operator !=(QualifiedName lhs, QualifiedName rhs)
		{
			return !(lhs == rhs);
		}		
		
		/// <summary>
		/// A qualified name is considered equal if the namespace and 
		/// name are the same.  The prefix is ignored.
		/// </summary>
		public override bool Equals(object obj) 
		{
			var qualifiedName = obj as QualifiedName;
			if (qualifiedName != null) {
				return _xmlQualifiedName.Equals(qualifiedName._xmlQualifiedName);
			} else {
				var name = obj as XmlQualifiedName;
				if (name != null) {
					return _xmlQualifiedName.Equals(name);
				}
			}
			return false;
		}
		
		public override int GetHashCode() 
		{
			return _xmlQualifiedName.GetHashCode();
		}
		
		public bool IsEmpty => _xmlQualifiedName.IsEmpty && String.IsNullOrEmpty(_prefix);

	    /// <summary>
		/// Gets the namespace of the qualified name.
		/// </summary>
		public string Namespace {
			get { return _xmlQualifiedName.Namespace; }
			set { _xmlQualifiedName = new XmlQualifiedName(_xmlQualifiedName.Name, value); }
		}
		
		public bool HasNamespace => _xmlQualifiedName.Namespace.Length > 0;

	    /// <summary>
		/// Gets the name of the element.
		/// </summary>
		public string Name {
			get { return _xmlQualifiedName.Name; }
			set { _xmlQualifiedName = new XmlQualifiedName(value, _xmlQualifiedName.Namespace); }
		}
		
		/// <summary>
		/// Gets the namespace prefix used.
		/// </summary>
		public string Prefix {
			get { return _prefix; }
			set { _prefix = value; }
		}
		
		public bool HasPrefix => !String.IsNullOrEmpty(_prefix);

	    public override string ToString()
		{
			var qualifiedName = GetPrefixedName();
			if (HasNamespace) {
				return String.Concat(qualifiedName, " [", _xmlQualifiedName.Namespace, "]");
			}
			return qualifiedName;
		}
		
		public string GetPrefixedName()
		{
			if (String.IsNullOrEmpty(_prefix)) {
				return _xmlQualifiedName.Name;
			}
			return _prefix + ":" + _xmlQualifiedName.Name;
		}
		
		public static QualifiedName FromString(string name)
		{
			if (name == null) {
				return new QualifiedName();
			}
			
			var index = name.IndexOf(':');
			if (index >= 0) {
				return CreateFromNameWithPrefix(name, index);
			}
			return new QualifiedName(name, String.Empty);
		}
		
		static QualifiedName CreateFromNameWithPrefix(string name, int index)
		{
			var prefix = name.Substring(0, index);
			name = name.Substring(index + 1);
			return new QualifiedName(name, String.Empty, prefix);
		}
	}
}
