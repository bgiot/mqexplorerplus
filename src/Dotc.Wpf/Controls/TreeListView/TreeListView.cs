
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Dotc.Wpf.Controls.TreeListView
{
    public class TreeListView : EnhancedTreeView.EnhancedTreeView
    {
        static TreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));
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
        /// Gets or sets an object that defines how the data is styled and organized in a System.Windows.Controls.ListView control
        /// </summary>
        /// <value>If not set: null</value>
        [Category("Common Properties")]
        [Description("Gets or sets an object that defines how the data is styled and organized in a System.Windows.Controls.ListView control")]
        [DefaultValue(null)]
        public GridView View
        {
            get { return (GridView)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        /// <summary>
        /// Identifies the View dependency property
        /// </summary>
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register("View", typeof(GridView), typeof(TreeListView), new UIPropertyMetadata(null));
    }
}
