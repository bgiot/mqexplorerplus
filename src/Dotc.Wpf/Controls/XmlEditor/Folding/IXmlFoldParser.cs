


using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;

namespace Dotc.Wpf.Controls.XmlEditor.Folding
{
	public interface IXmlFoldParser
	{
		IList<FoldingRegion> GetFolds(ITextSource textSource);
	}
}
