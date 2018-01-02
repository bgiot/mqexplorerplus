using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;

namespace Dotc.Wpf.Controls.SearchableTextBlock
{
    internal class SearchResult : TextSegment
    {
        internal Match Data { get; set; }
    }
}
