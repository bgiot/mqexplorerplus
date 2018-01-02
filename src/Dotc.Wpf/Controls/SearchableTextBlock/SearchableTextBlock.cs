using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using Dotc.Mvvm;

namespace Dotc.Wpf.Controls.SearchableTextBlock
{

    [TemplatePart(Name = "Part_Editor", Type = typeof(TextEditor))]
    public class SearchableTextBlock : Control
    {

        private TextEditor _textEditor;
        private SearchResultBackgroundRenderer _renderer;
        static SearchableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableTextBlock), new FrameworkPropertyMetadata(typeof(SearchableTextBlock)));
        }

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string), typeof(SearchableTextBlock),
            new FrameworkPropertyMetadata(null, SearchTextChangedCallback));

        public static readonly DependencyProperty MatchCaseProperty = DependencyProperty.Register("MatchCase", typeof(bool), typeof(SearchableTextBlock),
            new FrameworkPropertyMetadata(false, SearchTextChangedCallback));

        public static readonly DependencyProperty MarkerBrushProperty = DependencyProperty.Register("MarkerBrush", typeof(Brush), typeof(SearchableTextBlock),
            new FrameworkPropertyMetadata(Brushes.Orange, MarkerBrushChangedCallback));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SearchableTextBlock),
            new FrameworkPropertyMetadata(null, TextChangedCallback));

        public static readonly DependencyProperty WordWrapProperty = DependencyProperty.Register("WordWrap", typeof(bool), typeof(SearchableTextBlock),
            new FrameworkPropertyMetadata(true, TextChangedCallback));

        public static readonly DependencyProperty ShowLineNumbersProperty = DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(SearchableTextBlock),
            new FrameworkPropertyMetadata(true, TextChangedCallback));

        private static void TextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = d as SearchableTextBlock;
            if (ctl != null)
                ctl.UpdateText();
        }

        private static void MarkerBrushChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = d as SearchableTextBlock;
            if (ctl != null)
                ctl._renderer.MarkerBrush = (Brush)e.NewValue;
        }

        private static void SearchTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = d as SearchableTextBlock;
            if (ctl != null)
                ctl.DoSearch();
        }

        private ICommand _nextCommand;
        private ICommand _previousCommand;

        public ICommand NextCommand
        {
            get
            {
                return _nextCommand ?? (_nextCommand = new RelayCommand(() => FindNext(), () => CanNavigateResults()));
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                return _previousCommand ?? (_previousCommand = new RelayCommand(() => FindPrevious(), () => CanNavigateResults()));
            }
        }

        private bool CanNavigateResults()
        {
            if (_renderer == null) return false;
            return _renderer.CurrentResults.Any();
        }

        public bool WordWrap
        {
            get { return (bool)GetValue(WordWrapProperty); }
            set { SetValue(WordWrapProperty, value); }
        }

        public bool ShowLineNumbers
        {
            get { return (bool)GetValue(ShowLineNumbersProperty); }
            set { SetValue(ShowLineNumbersProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public Brush MarkerBrush
        {
            get { return (Brush)GetValue(MarkerBrushProperty); }
            set { SetValue(MarkerBrushProperty, value); }
        }
        public bool MatchCase
        {
            get { return (bool)GetValue(MatchCaseProperty); }
            set { SetValue(MatchCaseProperty, value); }
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textEditor = (TextEditor)GetTemplateChild("PART_Editor");
            _renderer = new SearchResultBackgroundRenderer();
            _textEditor?.TextArea.TextView.BackgroundRenderers.Add(_renderer);

            UpdateText();
        }

        private void DoSearch()
        {
            _renderer.CurrentResults.Clear();

            var textArea = _textEditor.TextArea;
            textArea.ClearSelection();

            if (!string.IsNullOrEmpty(SearchText))
            {

                RegexOptions options = RegexOptions.Compiled | RegexOptions.Multiline;
                if (!MatchCase)
                    options |= RegexOptions.IgnoreCase;

                var searchPattern = Regex.Escape(SearchText);
                Regex pattern = new Regex(searchPattern, options);
            
                var strategy = new RegexSearchStrategy(pattern, false);

                bool firstResult = true;
                var results = strategy.FindAll(textArea.Document, 0, textArea.Document.TextLength);

                foreach (SearchResult result in results)
                {
                    if (firstResult)
                    {
                        SelectResult(result);
                        firstResult = false;
                    }
                    _renderer.CurrentResults.Add(result);
                }
            }

            textArea.TextView.InvalidateLayer(KnownLayer.Selection);

        }

        void SelectResult(SearchResult result)
        {
            var textArea = _textEditor.TextArea;

            textArea.Caret.Offset = result.StartOffset;
            textArea.Selection = Selection.Create(textArea, result.StartOffset, result.EndOffset);
            textArea.SelectionCornerRadius = 0;
            textArea.Caret.BringCaretToView();
        }

        public void FindNext()
 		{
            var textArea = _textEditor.TextArea;
            SearchResult result = _renderer.CurrentResults.FindFirstSegmentWithStartAfter(textArea.Caret.Offset + 1); 
 			if (result == null) 
 				result = _renderer.CurrentResults.FirstSegment; 
 			if (result != null) { 
 				SelectResult(result); 
 			} 
 		} 
 
 		public void FindPrevious()
 		{
            var textArea = _textEditor.TextArea;
            SearchResult result = _renderer.CurrentResults.FindFirstSegmentWithStartAfter(textArea.Caret.Offset); 
 			if (result != null) 
 				result = _renderer.CurrentResults.GetPreviousSegment(result); 
 			if (result == null) 
 				result = _renderer.CurrentResults.LastSegment; 
 			if (result != null) { 
 				SelectResult(result); 
 			} 
 		} 
   

        private void UpdateText()
        {
            if (_textEditor != null)
            {
                _textEditor.WordWrap = WordWrap;
                _textEditor.ShowLineNumbers = ShowLineNumbers;
                _textEditor.Text = Text;
                DoSearch();
            }
        }
    }
}
