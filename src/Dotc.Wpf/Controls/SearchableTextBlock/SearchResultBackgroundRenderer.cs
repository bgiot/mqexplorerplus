using System;
using System.Linq;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Dotc.Wpf.Controls.SearchableTextBlock
{
    internal class SearchResultBackgroundRenderer : IBackgroundRenderer
    {
        public TextSegmentCollection<SearchResult> CurrentResults { get; } = new TextSegmentCollection<SearchResult>();

        internal SearchResultBackgroundRenderer()
        {
            markerBrush = Brushes.Orange;
            markerPen = new Pen(markerBrush, 1);
        }

        Brush markerBrush;
        Pen markerPen;

        internal Brush MarkerBrush
        {
            get { return markerBrush; }
            set
            {
                this.markerBrush = value;
                markerPen = new Pen(markerBrush, 1);
            }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView == null)
                throw new ArgumentNullException(nameof(textView));
            if (drawingContext == null)
                throw new ArgumentNullException(nameof(drawingContext));

            if (CurrentResults == null || !textView.VisualLinesValid)
                return;

            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
                return;

            int viewStart = visualLines.First().FirstDocumentLine.Offset;
            int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;

            foreach (SearchResult result in CurrentResults.FindOverlappingSegments(viewStart, viewEnd - viewStart))
            {
                BackgroundGeometryBuilder geoBuilder = new BackgroundGeometryBuilder();
                geoBuilder.AlignToWholePixels = true;
                geoBuilder.BorderThickness = markerPen != null ? markerPen.Thickness : 0;
                //geoBuilder.CornerRadius = 3;
                geoBuilder.AddSegment(textView, result);
                Geometry geometry = geoBuilder.CreateGeometry();
                if (geometry != null)
                {
                    drawingContext.DrawGeometry(markerBrush, markerPen, geometry);
                }
            }

        }

        public KnownLayer Layer => KnownLayer.Selection;
    }
}
