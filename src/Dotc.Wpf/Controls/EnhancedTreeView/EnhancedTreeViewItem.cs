

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Dotc.Wpf.Controls.EnhancedTreeView
{
    /// <summary>
    /// Represents an item in the EnhancedTreeView
    /// </summary>
    public class EnhancedTreeViewItem : TreeViewItem
    {

        public EnhancedTreeViewItem()
        {
            IsExpanded = true;
        }

        /// <summary>
        /// If items will be generated automatically, it uses a EnhancedTreeViewItem as a container
        /// </summary>
        /// <returns>The EnhancedTreeViewItem as a container for the item</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new EnhancedTreeViewItem() { ContentStretching = ContentStretching };
        }

        /// <summary>
        /// Checks if the current item is a correct container item
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <returns>True if the given item is a correct container</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is EnhancedTreeViewItem;
        }

        /// <summary>
        /// Gets or sets if the items should be have a streching content
        /// </summary>
        /// <value>If not set: false</value>
        [Category("Common Properties")]
        [Description("Gets or sets if the items should be have a streching content")]
        [DefaultValue(false)]
        public bool ContentStretching
        {
            get { return (bool)GetValue(ContentStretchingProperty); }
            set { SetValue(ContentStretchingProperty, value); }
        }

        /// <summary>
        /// Identifies the ContentStretching dependency property
        /// </summary>
        public static readonly DependencyProperty ContentStretchingProperty =
            DependencyProperty.Register("ContentStretching", typeof(bool), typeof(EnhancedTreeViewItem), new UIPropertyMetadata(OnContentStretchingChanged));

        private static void OnContentStretchingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            if (value)
            {
                var control = (EnhancedTreeViewItem)sender;
                if (control.IsLoaded)
                    control.AdjustChildren();
                else
                    WeakEventManager<EnhancedTreeViewItem, RoutedEventArgs>
                        .AddHandler(control, "Loaded", control.StretchingTreeViewItem_Loaded);
            }
        }

        private void StretchingTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustChildren();
        }

        private void AdjustChildren()
        {
            if (VisualChildrenCount > 0)
            {
                var grid = GetVisualChild(0) as Grid;
                if (grid != null &&
                    grid.ColumnDefinitions.Count == 3)
                    grid.ColumnDefinitions.RemoveAt(1);
            }
        }
    }
}
