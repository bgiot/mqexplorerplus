


using System.Collections.Generic;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;

namespace Dotc.Wpf.Controls.XmlEditor.Folding
{
	/// <summary>
	/// Description of FoldingManagerAdapter.
	/// </summary>
	public class FoldingManagerAdapter : IFoldingManager
	{
		FoldingManager _foldingManager;
		
		public FoldingManagerAdapter(TextEditor editor)
		{
            _foldingManager = FoldingManager.Install(editor.TextArea);
		}
		
		public void UpdateFoldings(IEnumerable<NewFolding> newFoldings, int firstErrorOffset)
		{
		    _foldingManager?.UpdateFoldings(newFoldings, firstErrorOffset);
		}

	    public void Dispose()
		{
            Dispose(true);
		}

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_foldingManager != null)
                {
                    FoldingManager.Uninstall(_foldingManager);
                    _foldingManager = null;
                }
            }
        }
	}
}
