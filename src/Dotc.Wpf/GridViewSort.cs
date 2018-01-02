using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Dotc.Wpf
{
    public class GridViewSort
    {
        #region GridView auto sort

        #region Public attached properties

        /// <summary>
        /// Obtient la valeur de la propriété attachée SortCommand pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut obtenir la valeur de la propriété</param>
        /// <returns>La commande de tri associée à l'objet spécifié</returns>
        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static ICommand GetSortCommand(ListView obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return (ICommand)obj.GetValue(SortCommandProperty);
        }

        /// <summary>
        /// Définit la valeur de la propriété attachée SortCommand pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut définir la valeur de la propriété</param>
        /// <param name="value">La commande de tri à associer à l'objet spécifié</param>
        public static void SetSortCommand(ListView obj, ICommand value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            obj.SetValue(SortCommandProperty, value);
        }

        /// <summary>
        /// Identifie la propriété attachée SortCommand
        /// </summary>
        public static readonly DependencyProperty SortCommandProperty =
            DependencyProperty.RegisterAttached(
                "SortCommand",
                typeof(ICommand),
                typeof(GridViewSort),
                new UIPropertyMetadata(
                    null,
                    OnSortCommandChanged
                )
            );

        private static void OnSortCommandChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var listView = o as ListView;
            if (listView == null)
                return;

            // Don't change click handler if AutoSort enabled
            if (GetAutoSort(listView))
                return;

            if (e.OldValue != null && e.NewValue == null)
            {
                listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
            }
            if (e.OldValue == null && e.NewValue != null)
            {
                listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
            }
        }

        /// <summary>
        /// Obtient la valeur de la propriété attachée AutoSort pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut obtenir la valeur de la propriété</param>
        /// <returns>true si le tri automatique est activé pour l'objet spécifié, false sinon</returns>
        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static bool GetAutoSort(ListView obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return (bool)obj.GetValue(AutoSortProperty);
        }

        /// <summary>
        /// Définit la valeur de la propriété attachée AutoSort pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut définir la valeur de la propriété</param>
        /// <param name="value">true pour activer le tri automatique pour l'objet spécifié, false sinon</param>
        public static void SetAutoSort(ListView obj, bool value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            obj.SetValue(AutoSortProperty, value);
        }

        /// <summary>
        /// Identifie la propriété attachée AutoSort
        /// </summary>
        public static readonly DependencyProperty AutoSortProperty =
            DependencyProperty.RegisterAttached(
                "AutoSort",
                typeof(bool),
                typeof(GridViewSort),
                new UIPropertyMetadata(
                    false,
                    OnAutoSortChanged
                )
            );

        private static void OnAutoSortChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var listView = o as ListView;
            if (listView == null)
                return;

            // Don't change click handler if a command is set
            if (GetSortCommand(listView) != null)
                return;

            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (oldValue && !newValue)
            {
                listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
            }
            if (!oldValue && newValue)
            {
                listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                listView.DoWhenLoaded(SetGlyphForInitialSort);
            }
        }

        private static void SetGlyphForInitialSort(ListView listView)
        {
            if (!GetAutoSort(listView) || !GetShowSortGlyph(listView))
                return;

            var view = CollectionViewSource.GetDefaultView(listView.Items);
            if (!view.SortDescriptions.Any())
                return;

            var headerRow = listView.FindDescendants<GridViewHeaderRowPresenter>().FirstOrDefault();
            if (headerRow == null)
                return;

            var schs = new Dictionary<string, GridViewColumnHeader>();
            var headers = headerRow.FindDescendants<GridViewColumnHeader>();

            foreach (var sort in view.SortDescriptions)
            {
                foreach (var header in headers)
                {
                    if (header.Column == null)
                        continue;
                    var sortPropertyName = GetSortPropertyName(header.Column);
                    if (sortPropertyName != sort.PropertyName)
                        continue;

                    AddSortGlyph(
                        header,
                        sort.Direction,
                        sort.Direction == ListSortDirection.Ascending ? GetSortGlyphAscending(listView) : GetSortGlyphDescending(listView));

                    schs.Add(sortPropertyName, header);
                }
            }

            SetSortedColumnHeaders(listView, schs);


        }

        /// <summary>
        /// Obtient la valeur de la propriété attachée SortPropertyName pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut obtenir la valeur de la propriété</param>
        /// <returns>Le nom de la propriété de tri associée à l'objet spécifié</returns>
        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static string GetSortPropertyName(GridViewColumn obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return (string)obj.GetValue(SortPropertyNameProperty);
        }

        /// <summary>
        /// Définit la valeur de la propriété attachée SortPropertyName pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut définir la valeur de la propriété</param>
        /// <param name="value">La propriété de tri à associer à l'objet spécifié</param>
        public static void SetSortPropertyName(GridViewColumn obj, string value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            obj.SetValue(SortPropertyNameProperty, value);
        }

        /// <summary>
        /// Identifie la propriété attachée SortPropertyName
        /// </summary>
        public static readonly DependencyProperty SortPropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "SortPropertyName",
                typeof(string),
                typeof(GridViewSort),
                new UIPropertyMetadata(null)
            );

        /// <summary>
        /// Obtient la valeur de la propriété attachée ShowSortGlyph pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut obtenir la valeur de la propriété</param>
        /// <returns>true si le symbole de tri est affiché, false sinon</returns>
        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static bool GetShowSortGlyph(ListView obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return (bool)obj.GetValue(ShowSortGlyphProperty);
        }

        /// <summary>
        /// Définit la valeur de la propriété attachée ShowSortGlyph pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut définir la valeur de la propriété</param>
        /// <param name="value">true pour afficher le symbole de tri, false sinon</param>
        public static void SetShowSortGlyph(ListView obj, bool value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            obj.SetValue(ShowSortGlyphProperty, value);
        }

        /// <summary>
        /// Identifie la propriété attachée ShowSortGlyph
        /// </summary>
        public static readonly DependencyProperty ShowSortGlyphProperty =
            DependencyProperty.RegisterAttached("ShowSortGlyph", typeof(bool), typeof(GridViewSort), new UIPropertyMetadata(true));

        /// <summary>
        /// Obtient la valeur de la propriété attachée SortGlyphAscending pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut obtenir la valeur de la propriété</param>
        /// <returns>Le symbole de tri croissant associé à l'objet spécifié</returns>
        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static ImageSource GetSortGlyphAscending(ListView obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return (ImageSource)obj.GetValue(SortGlyphAscendingProperty);
        }

        /// <summary>
        /// Définit la valeur de la propriété attachée SortGlyphAscending pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut définir la valeur de la propriété</param>
        /// <param name="value">Le symbole de tri croissant à associer à l'objet spécifié</param>
        public static void SetSortGlyphAscending(ListView obj, ImageSource value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            obj.SetValue(SortGlyphAscendingProperty, value);
        }

        /// <summary>
        /// Identifie la propriété attachée SortGlyphAscending
        /// </summary>
        public static readonly DependencyProperty SortGlyphAscendingProperty =
            DependencyProperty.RegisterAttached("SortGlyphAscending", typeof(ImageSource), typeof(GridViewSort), new UIPropertyMetadata(null));

        /// <summary>
        /// Obtient la valeur de la propriété attachée SortGlyphDescending pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut obtenir la valeur de la propriété</param>
        /// <returns>Le symbole de tri décroissant associé à l'objet spécifié</returns>
        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static ImageSource GetSortGlyphDescending(ListView obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return (ImageSource)obj.GetValue(SortGlyphDescendingProperty);
        }

        /// <summary>
        /// Définit la valeur de la propriété attachée SortGlyphDescending pour l'objet spécifié
        /// </summary>
        /// <param name="obj">Objet dont on veut définir la valeur de la propriété</param>
        /// <param name="value">Le symbole de tri décroissant à associer à l'objet spécifié</param>
        public static void SetSortGlyphDescending(ListView obj, ImageSource value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            obj.SetValue(SortGlyphDescendingProperty, value);
        }

        /// <summary>
        /// Identifie la propriété attachée SortGlyphDescending
        /// </summary>
        public static readonly DependencyProperty SortGlyphDescendingProperty =
            DependencyProperty.RegisterAttached("SortGlyphDescending", typeof(ImageSource), typeof(GridViewSort), new UIPropertyMetadata(null));


        #endregion

        #region Private attached properties

        private static Dictionary<string, GridViewColumnHeader> GetSortedColumnHeaders(DependencyObject obj)
        {
            return (Dictionary<string, GridViewColumnHeader>)obj.GetValue(SortedColumnHeadersProperty);
        }

        private static void SetSortedColumnHeaders(DependencyObject obj, Dictionary<string, GridViewColumnHeader> value)
        {
            obj.SetValue(SortedColumnHeadersProperty, value);
        }

        private static readonly DependencyProperty SortedColumnHeadersProperty =
            DependencyProperty.RegisterAttached("SortedColumnHeaders", typeof(Dictionary<string, GridViewColumnHeader>), typeof(GridViewSort), new UIPropertyMetadata(null));



        #endregion

        #region Column header click event handler

        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var resetSorts = ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift);

            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked?.Column != null)
            {
                var propertyName = GetSortPropertyName(headerClicked.Column);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    var listView = headerClicked.FindAncestor<ListView>();
                    if (listView != null)
                    {
                        var command = GetSortCommand(listView);
                        if (command != null)
                        {
                            if (command.CanExecute(propertyName))
                            {
                                command.Execute(propertyName);
                            }
                        }
                        else if (GetAutoSort(listView))
                        {
                            ApplySort(listView.Items, propertyName, listView, headerClicked, resetSorts);
                        }
                    }
                }
            }
        }

        #endregion

        #region Helper methods

        private static void ApplySort(ICollectionView view, string propertyName, ListView listView, GridViewColumnHeader sortedColumnHeader, bool resetSorts)
        {
            bool sortIsExisting = false;
            ListSortDirection direction = ListSortDirection.Ascending;
            SortDescription currentSd = new SortDescription();

            var schs = GetSortedColumnHeaders(listView);
            if (schs == null)
                schs = new Dictionary<string, GridViewColumnHeader>();

            if (view.SortDescriptions.Count > 0)
            {
                foreach (var sd in view.SortDescriptions)
                {
                    if (sd.PropertyName == propertyName)
                    {
                        sortIsExisting = true;
                        currentSd = sd;
                        direction = sd.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                    }
                }

                if (resetSorts)
                {
                    view.SortDescriptions.Clear();
                    var gv = listView.View as GridView;
                    foreach (var de in schs)
                    {
                        RemoveSortGlyph(de.Value);
                    }
                    schs.Clear();
                }
                else
                {
                    if (sortIsExisting)
                    {
                        RemoveSortGlyph(schs[propertyName]);
                        view.SortDescriptions.Remove(currentSd);
                        schs.Remove(propertyName);
                    }
                }

            }
            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));

                if (GetShowSortGlyph(listView))
                    AddSortGlyph(
                        sortedColumnHeader,
                        direction,
                        direction == ListSortDirection.Ascending ? GetSortGlyphAscending(listView) : GetSortGlyphDescending(listView));

                schs.Add(propertyName, sortedColumnHeader);

                SetSortedColumnHeaders(listView, schs);
            }
        }

        private static void AddSortGlyph(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            adornerLayer.Add(
                new SortGlyphAdorner(
                    columnHeader,
                    direction,
                    sortGlyph
                    ));
        }

        private static void RemoveSortGlyph(GridViewColumnHeader columnHeader)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            var adorners = adornerLayer.GetAdorners(columnHeader);
            if (adorners != null)
            {
                foreach (var adorner in adorners)
                {
                    if (adorner is SortGlyphAdorner)
                        adornerLayer.Remove(adorner);
                }
            }
        }

        #endregion

        #region SortGlyphAdorner nested class

        private class SortGlyphAdorner : Adorner
        {
            private readonly GridViewColumnHeader _columnHeader;
            private readonly ListSortDirection _direction;
            private readonly ImageSource _sortGlyph;

            public SortGlyphAdorner(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
                : base(columnHeader)
            {
                _columnHeader = columnHeader;
                _direction = direction;
                _sortGlyph = sortGlyph;
            }

            private Geometry GetDefaultGlyph()
            {
                var x1 = _columnHeader.ActualWidth - 13;
                var x2 = x1 + 10;
                var x3 = x1 + 5;
                var y1 = _columnHeader.ActualHeight / 2 - 3;
                var y2 = y1 + 5;

                if (_direction == ListSortDirection.Ascending)
                {
                    var tmp = y1;
                    y1 = y2;
                    y2 = tmp;
                }

                var pathSegmentCollection = new PathSegmentCollection
                {
                    new LineSegment(new Point(x2, y1), true),
                    new LineSegment(new Point(x3, y2), true)
                };

                var pathFigure = new PathFigure(
                    new Point(x1, y1),
                    pathSegmentCollection,
                    true);

                var pathFigureCollection = new PathFigureCollection { pathFigure };

                var pathGeometry = new PathGeometry(pathFigureCollection);
                return pathGeometry;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (drawingContext == null) throw new ArgumentNullException(nameof(drawingContext));

                if (_sortGlyph != null)
                {
                    var x = _columnHeader.ActualWidth - 13;
                    var y = _columnHeader.ActualHeight / 2 - 5;
                    var rect = new Rect(x, y, 10, 10);
                    drawingContext.DrawImage(_sortGlyph, rect);
                }
                else
                {
                    drawingContext.DrawGeometry(Brushes.LightGray, new Pen(Brushes.Gray, 1.0), GetDefaultGlyph());
                }
            }
        }

        #endregion

        #endregion
    }
}
