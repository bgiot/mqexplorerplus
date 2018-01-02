


using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;

namespace Dotc.Wpf.Controls.XmlEditor.Folding
{
	public sealed class XmlFoldParser : IXmlFoldParser, IDisposable
	{
		XmlTextReader _reader;
		List<FoldingRegion> _folds = new List<FoldingRegion>();
		Stack<XmlElementFold> _elementFoldStack;

		
		public XmlFoldParser()
		{
		}
		
		public IList<FoldingRegion> GetFolds(ITextSource textSource)
		{
			try {
				GetFolds(textSource.CreateReader());
				return _folds;
			} catch (XmlException) {
			}
			return null;
		}
		
		void GetFolds(TextReader textReader)
		{
			_folds = new List<FoldingRegion>();
			_elementFoldStack = new Stack<XmlElementFold>();
			
			CreateXmlTextReaderWithNoNamespaceSupport(textReader);
			
			while (_reader.Read()) {
				switch (_reader.NodeType) {
					case XmlNodeType.Element:
						AddElementFoldToStackIfNotEmptyElement();
						break;
						
					case XmlNodeType.EndElement:
						CreateElementFoldingRegionIfNotSingleLine();
						break;
						
					case XmlNodeType.Comment:
						CreateCommentFoldingRegionIfNotSingleLine();
						break;
				}
			}
			_folds.Sort(CompareFoldingRegion);
		}
		
		void CreateXmlTextReaderWithNoNamespaceSupport(TextReader textReader)
		{
		    _reader = new XmlTextReader(textReader)
		    {
		        XmlResolver = null,
		        Namespaces = false
		    };
		    // prevent XmlTextReader from loading external DTDs
		}
		
		void AddElementFoldToStackIfNotEmptyElement()
		{
			if (!_reader.IsEmptyElement) {
				var fold = new XmlElementFold();
				fold.ReadStart(_reader);
				_elementFoldStack.Push(fold);
			}
		}
		
		void CreateElementFoldingRegionIfNotSingleLine()
		{
			var fold = _elementFoldStack.Pop();
			fold.ReadEnd(_reader);
			if (!fold.IsSingleLine) {
				var foldingRegion = CreateFoldingRegion(fold);
				_folds.Add(foldingRegion);
			}
		}
		
		FoldingRegion CreateFoldingRegion(XmlElementFold fold)
		{
				return fold.CreateFoldingRegionWithAttributes();
		}
		
		/// <summary>
		/// Creates a comment fold if the comment spans more than one line.
		/// </summary>
		/// <remarks>The text displayed when the comment is folded is the first
		/// line of the comment.</remarks>
		void CreateCommentFoldingRegionIfNotSingleLine()
		{
			var fold = new XmlCommentFold(_reader);
			if (!fold.IsSingleLine) {
				_folds.Add(fold.CreateFoldingRegion());
			}
		}
		
		int CompareFoldingRegion(FoldingRegion lhs, FoldingRegion rhs)
		{
			var compareBeginLine = lhs.Region.BeginLine.CompareTo(rhs.Region.BeginLine);
			if (compareBeginLine == 0) {
				return lhs.Region.BeginColumn.CompareTo(rhs.Region.BeginColumn);
			}
			return compareBeginLine;
		}

        public void Dispose()
        {
            ((IDisposable) _reader)?.Dispose();
        }
	}
}
