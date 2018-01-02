


using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Folding;

namespace Dotc.Wpf.Controls.XmlEditor.Folding
{
	public interface IFoldingManager : IDisposable
	{
		void UpdateFoldings(IEnumerable<NewFolding> newFoldings, int firstErrorOffset);
	}
}
