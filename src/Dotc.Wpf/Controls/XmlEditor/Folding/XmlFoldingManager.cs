


using System;
using System.Collections.Generic;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System.Windows;

namespace Dotc.Wpf.Controls.XmlEditor.Folding
{
	public sealed class XmlFoldingManager : IDisposable
	{
		bool _documentHasChangedSinceLastFoldUpdate;
	    readonly TextDocument _document;
	    readonly IXmlFoldParser _xmlFoldParser;
	    readonly IFoldingManager _foldingManager;
		const int NoFirstErrorOffset = -1;
		DispatcherTimer _timer;
		bool _updating;
		
		public XmlFoldingManager(TextEditor textEditor)
			: this(textEditor, new FoldingManagerAdapter(textEditor), new XmlFoldParser())
		{
		}
		
		public XmlFoldingManager(TextEditor textEditor, IFoldingManager foldingManager, IXmlFoldParser xmlFoldParser)
		{
			_document = textEditor.Document;
            WeakEventManager<TextDocument, EventArgs>
                .AddHandler(_document, "TextChanged", DocumentChanged);
			
			_foldingManager = foldingManager;
			_xmlFoldParser = xmlFoldParser;
		}


        void DocumentChanged(object source, EventArgs e)
		{
			_documentHasChangedSinceLastFoldUpdate = true;
		}
		
		public bool DocumentHasChangedSinceLastFoldUpdate {
			get { return _documentHasChangedSinceLastFoldUpdate; }
			set { _documentHasChangedSinceLastFoldUpdate = value; }
		}
		
		public ITextSource CreateTextEditorSnapshot()
		{
			return _document.CreateSnapshot();
		}
		
		public void Dispose()
		{
			_document.TextChanged -= DocumentChanged;
			_foldingManager.Dispose();
		}


		
		public IList<FoldingRegion> GetFolds(ITextSource textSource)
		{
			return _xmlFoldParser.GetFolds(textSource);
		}

		
		public void UpdateFolds(IEnumerable<FoldingRegion> folds)
		{
			var firstErrorOffset = NoFirstErrorOffset;
			_foldingManager.UpdateFoldings(ConvertFoldRegionsToNewFolds(folds), firstErrorOffset);
		}
		
		public void UpdateFolds()
		{
			var textSource = CreateTextEditorSnapshot();
			var folds = GetFolds(textSource);
			if (folds != null) {
				UpdateFolds(folds);
			}
		}
		
		public IList<NewFolding> ConvertFoldRegionsToNewFolds(IEnumerable<FoldingRegion> folds)
		{
			var newFolds = new List<NewFolding>();
			foreach (var foldingRegion in folds) {
				var newFold = ConvertToNewFold(foldingRegion);
				newFolds.Add(newFold);
			}
			return newFolds;
		}
		
		NewFolding ConvertToNewFold(FoldingRegion foldingRegion)
		{
		    var newFold = new NewFolding
		    {
		        Name = foldingRegion.Name,
		        StartOffset = GetStartOffset(foldingRegion.Region),
		        EndOffset = GetEndOffset(foldingRegion.Region)
		    };


		    return newFold;
		}
		
		int GetStartOffset(DomRegion region)
		{
			return GetOffset(region.BeginLine, region.BeginColumn);
		}
		
		int GetEndOffset(DomRegion region)
		{
			return GetOffset(region.EndLine, region.EndColumn);
		}
		
		int GetOffset(int line, int column)
		{
			return _document.GetOffset(line, column);
		}
		
		public void Start()
		{
		    _timer = new DispatcherTimer(DispatcherPriority.Background) {Interval = new TimeSpan(0, 0, 2)};
            WeakEventManager<DispatcherTimer, EventArgs>
                .AddHandler(_timer, "Tick", TimerTick);

			_timer.Start();
		}
		
		void TimerTick(object source, EventArgs e)
		{
			if (DocumentHasChangedSinceLastFoldUpdate) {
				if (!_updating) {
					_updating = true;
					DocumentHasChangedSinceLastFoldUpdate = false;
					UpdateFolds();
					_updating = false;
				}
			}
		}
		
		public void Stop()
		{
            _timer.Stop();
            WeakEventManager<DispatcherTimer, EventArgs>
                .RemoveHandler(_timer, "Tick", TimerTick);

		}
	}
}
