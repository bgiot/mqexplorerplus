using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Dotc.Wpf.Controls.XmlEditor
{
    public class TextMarkerService : IBackgroundRenderer, IVisualLineTransformer
    {
        private readonly TextEditor _textEditor;
        private readonly TextSegmentCollection<TextMarker> _markers;

        public sealed class TextMarker : TextSegment
        {
            public TextMarker(int startOffset, int length)
            {
                StartOffset = startOffset;
                Length = length;
            }

            public Color? BackgroundColor { get; set; }
            public Color MarkerColor { get; set; }
            public string ToolTip { get; set; }
        }

        public TextMarkerService(TextEditor textEditor)
        {
            _textEditor = textEditor;
            _markers = new TextSegmentCollection<TextMarker>(textEditor.Document);
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_markers == null || !textView.VisualLinesValid)
            {
                return;
            }
            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
            {
                return;
            }
            var viewStart = visualLines.First().FirstDocumentLine.Offset;
            var viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
            foreach (var marker in _markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
            {
                if (marker.BackgroundColor != null)
                {
                    var geoBuilder = new BackgroundGeometryBuilder { AlignToWholePixels = true, CornerRadius = 3 };
                    geoBuilder.AddSegment(textView, marker);
                    var geometry = geoBuilder.CreateGeometry();
                    if (geometry != null)
                    {
                        var color = marker.BackgroundColor.Value;
                        var brush = new SolidColorBrush(color);
                        brush.Freeze();
                        drawingContext.DrawGeometry(brush, null, geometry);
                    }
                }
                foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    var startPoint = r.BottomLeft;
                    var endPoint = r.BottomRight;

                    var usedPen = new Pen(new SolidColorBrush(marker.MarkerColor), 1);
                    usedPen.Freeze();
                    const double offset = 2.5;

                    var count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);

                    var geometry = new StreamGeometry();

                    using (var ctx = geometry.Open())
                    {
                        ctx.BeginFigure(startPoint, false, false);
                        ctx.PolyLineTo(CreatePoints(startPoint, endPoint, offset, count).ToArray(), true, false);
                    }

                    geometry.Freeze();

                    drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                    break;
                }
            }
        }

        public KnownLayer Layer => KnownLayer.Selection;

        public void Transform(ITextRunConstructionContext context, IList<VisualLineElement> elements)
        { }

        private IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return new Point(start.X + (i * offset), start.Y - ((i + 1) % 2 == 0 ? offset : 0));
            }
        }

        public void Clear()
        {
            foreach (var m in _markers)
            {
                Remove(m);
            }
        }

        private void Remove(TextMarker marker)
        {
            if (_markers.Remove(marker))
            {
                Redraw(marker);
            }
        }

        private void Redraw(ISegment segment)
        {
            _textEditor.TextArea.TextView.Redraw(segment);
        }

        public void Create(int offset, int length, string message)
        {
            var m = new TextMarker(offset, length);
            _markers.Add(m);
            m.MarkerColor = Colors.Red;
            m.ToolTip = message;
            Redraw(m);
        }

        public IEnumerable<TextMarker> GetMarkersAtOffset(int offset)
        {
            return _markers == null ? Enumerable.Empty<TextMarker>() : _markers.FindSegmentsContaining(offset);
        }
    }
}
