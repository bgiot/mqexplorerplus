


using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	[Serializable()]
	public class QualifiedNameCollection : Collection<QualifiedName> 
	{
		public QualifiedNameCollection()
		{
		}
		
		public QualifiedNameCollection(QualifiedNameCollection names)
		{
			AddRange(names);
		}
		
		public QualifiedNameCollection(QualifiedName[] names)
		{
			AddRange(names);
		}
		
		public bool HasItems => Count > 0;

	    public bool IsEmpty => !HasItems;

	    public override string ToString()
		{
			var text = new StringBuilder();
			for (var i = 0; i < Count; i++) {
				if (i > 0) {
					text.Append(" > ");
				}
				text.Append(this[i].ToString());
			}
			return text.ToString();
		}
		
		public void AddRange(QualifiedName[] names)
		{
		    if (names == null) throw new ArgumentNullException(nameof(names));
			for (var i = 0; i < names.Length; i++) {
				Add(names[i]);
			}
		}
		
		public void AddRange(QualifiedNameCollection names)
		{
            if (names == null) throw new ArgumentNullException(nameof(names));
            for (var i = 0; i < names.Count; i++) {
				Add(names[i]);
			}
		}
		
		public void RemoveLast()
		{
			if (HasItems) {
				RemoveAt(Count - 1);
			}
		}
		
		public void RemoveFirst()
		{
			if (HasItems) {
				RemoveFirst(1);
			}
		}
		
		public void RemoveFirst(int howMany)
		{
			if (howMany > Count) {
				howMany = Count;
			}
			
			while (howMany > 0) {
				RemoveAt(0);
				--howMany;
			}
		}
		
		public string GetLastPrefix() 
		{
			if (HasItems) {
				var name = this[Count - 1];
				return name.Prefix;
			}
			return String.Empty;
		}
		
		public string GetNamespaceForPrefix(string prefix)
		{
			foreach (var name in this) {
				if (name.Prefix == prefix) {
					return name.Namespace;
				}
			}
			return String.Empty;
		}
		
		public QualifiedName GetLast()
		{
			if (HasItems) {
				return this[Count - 1];
			}
			return null;			
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public override bool Equals(object obj)
		{
			var rhs = obj as QualifiedNameCollection;
		    if (Count == rhs?.Count) {
		        for (var i = 0; i < Count; ++i) {
		            var lhsName = this[i];
		            var rhsName = rhs[i];
		            if (!lhsName.Equals(rhsName)) {
		                return false;
		            }
		        }	
		        return true;
		    }
		    return false;
		}
		
		public string GetRootNamespace()
		{
			if (HasItems) {
				return this[0].Namespace;
			}
			return String.Empty;
		}
	}
}
