using System.ComponentModel;
using System.Windows;
using Dotc.Wpf.Controls.EnhancedTreeView;

namespace Dotc.Wpf.Controls.TreeListView
{
    /// <summary>
    /// Represents an item in a TreeListView
    /// </summary>
    public class TreeListViewItem : EnhancedTreeViewItem
    {
        static TreeListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListViewItem), new FrameworkPropertyMetadata(typeof(TreeListViewItem)));
        }

        /// <summary>
        /// If items will be generated automatically, it uses a TreeListViewItem as a container
        /// </summary>
        /// <returns>The TreeListViewItem as a container for the item</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        /// <summary>
        /// Checks if the current item is a correct container item
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <returns>True if the given item is a correct container</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        /// <summary>
        /// Gets the deepness level for this item
        /// </summary>
        [Category("Common Properties")]
        [Description("Gets the deepness level for this item")]
        public int Level => VisualTreeAssist.GetParentsUntilCount<TreeListViewItem, TreeListView>(this);
    }
}
