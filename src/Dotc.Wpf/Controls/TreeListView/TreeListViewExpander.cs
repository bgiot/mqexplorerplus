

using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dotc.Wpf.Controls.TreeListView
{
    /// <summary>
    /// Represents the expander in a TreListViewItem
    /// </summary>
    public class TreeListViewExpander : ToggleButton
    {
        static TreeListViewExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListViewExpander), new FrameworkPropertyMetadata(typeof(TreeListViewExpander)));
        }
    }
}
