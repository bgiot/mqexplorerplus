


using System;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	/// <summary>
	/// A namespace Uri and a prefix.
	/// </summary>
	public class XmlNamespace
	{
		string _prefix = String.Empty;
		string _name = String.Empty;
		
		const string PrefixToStringStart = "Prefix [";
		const string UriToStringMiddle = "] Uri [";
		
		public XmlNamespace()
		{
		}
		
		public XmlNamespace(string prefix, string name)
		{
			Prefix = prefix;
			Name = name;
		}
		
		public string Prefix {
			get { return _prefix; }
			set { 
				_prefix = value ?? String.Empty;
			}
		}
		
		public string Name {
			get { return _name; }
			set { 
				_name = value ?? String.Empty;
			}
		}
		
		public bool HasName => !String.IsNullOrEmpty(_name);

	    public override string ToString()
		{
			return String.Concat(PrefixToStringStart, _prefix, UriToStringMiddle, _name, "]");
		}
		
		/// <summary>
		/// Creates an XmlNamespace instance from the given string that is in the
		/// format returned by ToString.
		/// </summary>
		public static XmlNamespace FromString(string namespaceString)
		{
		    if (namespaceString == null) throw new ArgumentNullException(nameof(namespaceString));
			var prefixIndex = namespaceString.IndexOf(PrefixToStringStart, StringComparison.Ordinal);
			if (prefixIndex >= 0) {
				prefixIndex += PrefixToStringStart.Length;
				var uriIndex = namespaceString.IndexOf(UriToStringMiddle, prefixIndex, StringComparison.Ordinal);
				if (uriIndex >= 0) {
					var prefix = namespaceString.Substring(prefixIndex, uriIndex - prefixIndex);
					uriIndex += UriToStringMiddle.Length;
					var uri = namespaceString.Substring(uriIndex, namespaceString.Length - (uriIndex + 1));
					return new XmlNamespace(prefix, uri);
				}
			}
			return new XmlNamespace();
		}
		
		public override bool Equals(object obj)
		{
			var rhs = obj as XmlNamespace;
			if (rhs != null) {
				return (Name == rhs.Name) && (Prefix == rhs.Prefix);
			}
			return false;
		}
		
		public override int GetHashCode()
		{
			return Name.GetHashCode() ^ Prefix.GetHashCode();
		}
	}
}
