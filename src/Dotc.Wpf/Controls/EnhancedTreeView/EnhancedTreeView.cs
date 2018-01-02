

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dotc.Wpf.Controls.EnhancedTreeView
{
    public class EnhancedTreeView : TreeView
    {
        /// <summary>
        /// Initializes a new instance of the EnhancedTreeView class
        /// </summary>
        public EnhancedTreeView()
        {
            WeakEventManager<TreeView, MouseButtonEventArgs>
                .AddHandler(this, "PreviewMouseRightButtonDown", EnhancedTreeView_PreviewMouseRightButtonDown);
            _isSelectionChangeActiveProperty = typeof(TreeView).GetProperty("IsSelectionChangeActive", BindingFlags.NonPublic | BindingFlags.Instance);
            SelectedTreeViewItems = new ObservableCollection<TreeViewItem>();

            _isCodeSelection = true;

            WeakEventManager<TreeView, MouseButtonEventArgs>
                .AddHandler(this, "PreviewMouseDown", EnhancedTreeView_PreviewMouseDown);
            WeakEventManager<TreeView, MouseButtonEventArgs>
                .AddHandler(this, "PreviewMouseUp", EnhancedTreeView_PreviewMouseUp);

            WeakEventManager<TreeView, KeyEventArgs>
                .AddHandler(this, "PreviewKeyDown", EnhancedTreeView_PreviewKeyDown);
            WeakEventManager<TreeView, KeyEventArgs>
                .AddHandler(this, "PreviewKeyUp", EnhancedTreeView_PreviewKeyUp);

        }

        private bool _isCodeSelection;
        private TreeViewItem _lastSelectedItem;

        private void EnhancedTreeView_PreviewMouseDown(object sender, MouseButtonEventArgs args)
        {
            _isCodeSelection = false;
        }
        private void EnhancedTreeView_PreviewMouseUp(object sender, MouseButtonEventArgs args)
        {
            _isCodeSelection = true;
        }
        private void EnhancedTreeView_PreviewKeyDown(object sender, KeyEventArgs args)
        {
            _isCodeSelection = false;
        }
        private void EnhancedTreeView_PreviewKeyUp(object sender, KeyEventArgs args)
        {
            _isCodeSelection = true;
        }
        /// <summary>
        /// If items will be generated automatically, it uses a EnhancedTreeViewItem as a container
        /// </summary>
        /// <returns>The EnhancedTreeViewItem as a container for the item</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new EnhancedTreeViewItem() { ContentStretching = ItemsContentStretching };
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

        private void EnhancedTreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var control = (EnhancedTreeView)sender;
            var clickedItem = control.InputHitTest(e.GetPosition(control));
            while (clickedItem != null && !(clickedItem is TreeViewItem))
            {
                var frameworkkItem = (FrameworkElement)clickedItem;
                clickedItem = (IInputElement)(frameworkkItem.Parent ?? frameworkkItem.TemplatedParent);
            }

            if (clickedItem != null)
                ((TreeViewItem)clickedItem).IsSelected = true;
        }

        /// <summary>
        /// The item selection was changed
        /// </summary>
        /// <param name="e">Information about the change</param>
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);

            if (SelectionMode == SelectionMode.Single)
                return;

            DisableSelectionChangedEvent();

            var newSelected = GetSelectedContainer();
            if (newSelected != null)
            {
                if (_isCodeSelection)
                    HandleCodeSelection(newSelected);
                else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    HandleControlKeySelection(newSelected);
                else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    HandleShiftKeySelection(newSelected);
                else
                    HandleSingleSelection(newSelected);
            }

            EnableSelectionChangedEvent();
        }

        private void HandleCodeSelection(TreeViewItem newSelected)
        {
            HandleControlKeySelection(newSelected);
            RemoveDeselectedItemContainers();
        }

        private void RemoveDeselectedItemContainers()
        {
            for (var i = 0; i < SelectedTreeViewItems.Count; i++)
            {
                if (!SelectedTreeViewItems[i].IsSelected)
                    SelectedTreeViewItems.RemoveAt(i);
            }
        }

        private void HandleControlKeySelection(TreeViewItem newSelected)
        {
            if (SelectedTreeViewItems.Contains(newSelected))
            {
                newSelected.IsSelected = false;
                SelectedTreeViewItems.Remove(newSelected);
                if (_lastSelectedItem != null)
                    _lastSelectedItem.IsSelected = true;
                _lastSelectedItem = null;
            }
            else
            {
                if (_lastSelectedItem != null)
                    _lastSelectedItem.IsSelected = true;
                SelectedTreeViewItems.Add(newSelected);
                _lastSelectedItem = newSelected;
            }
        }

        private void HandleShiftKeySelection(TreeViewItem newSelectedItemContainer)
        {
            if (_lastSelectedItem != null)
            {
                ClearAllSelections();
                var items = GetFlatTreeViewItems(this);

                var firstItemPos = items.IndexOf(_lastSelectedItem);
                var lastItemPos = items.IndexOf(newSelectedItemContainer);
                Sort(ref firstItemPos, ref lastItemPos);

                for (var i = firstItemPos; i <= lastItemPos; ++i)
                {
                    items[i].IsSelected = true;
                    SelectedTreeViewItems.Add(items[i]);
                }
            }
        }

        private void HandleSingleSelection(TreeViewItem newSelectedItem)
        {
            ClearAllSelections();
            newSelectedItem.IsSelected = true;
            SelectedTreeViewItems.Add(newSelectedItem);
            _lastSelectedItem = newSelectedItem;
        }

        private void Sort(ref int firstItemPos, ref int lastItemPos)
        {
            if (firstItemPos > lastItemPos)
            {
                var tmp = firstItemPos;
                firstItemPos = lastItemPos;
                lastItemPos = tmp;
            }
        }

        private List<TreeViewItem> GetFlatTreeViewItems(ItemsControl control)
        {
            var items = new List<TreeViewItem>();

            foreach (var item in control.Items)
            {
                var containerItem = item as TreeViewItem ??
                                    control.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (containerItem != null)
                {
                    items.Add(containerItem);
                    if (containerItem.IsExpanded)
                        items.AddRange(GetFlatTreeViewItems(containerItem));
                }
            }

            return items;
        }

        private void ClearAllSelections()
        {
            while (SelectedTreeViewItems.Count > 0)
            {
                SelectedTreeViewItems[0].IsSelected = false;
                SelectedTreeViewItems.RemoveAt(0);
            }
        }

        private TreeViewItem GetSelectedContainer()
        {
            var fieldInfo = typeof(TreeView).GetField("_selectedContainer", BindingFlags.NonPublic | BindingFlags.Instance);
            return (TreeViewItem) fieldInfo?.GetValue((TreeView)this);
        }

        private readonly PropertyInfo _isSelectionChangeActiveProperty;
        
        private void DisableSelectionChangedEvent()
        {
            _isSelectionChangeActiveProperty.SetValue((TreeView)this, true, null);
        }

        private void EnableSelectionChangedEvent()
        {
            _isSelectionChangeActiveProperty.SetValue((TreeView)this, false, null);
        }

        /// <summary>
        /// Gets a list of the selected items
        /// </summary>
        public ObservableCollection<object> SelectedItems
        {
            get
            {
                var items = new ObservableCollection<object>();
                var selectedItems = SelectedTreeViewItems.Select(i => i.Header);
                foreach (var item in selectedItems)
                    items.Add(item);
                return items;
            }
        }

        /// <summary>
        /// Gets a list of the selected items
        /// </summary>
        public ObservableCollection<TreeViewItem> SelectedTreeViewItems { get; }

        /// <summary>
        /// Gets or sets the way how items can be selected
        /// </summary>
        /// <value>If not set: SelectionMode.Extended</value>
        [Category("Common Properties")]
        [Description("Gets or sets the way how items can be selected")]
        [DefaultValue(SelectionMode.Extended)]
        public SelectionMode SelectionMode
        {
            get { return (SelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectionMode dependency property
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register("SelectionMode", typeof(SelectionMode), typeof(EnhancedTreeView), new UIPropertyMetadata(SelectionMode.Extended));

        /// <summary>
        /// Gets or sets if the items in the tree should be have a streching content
        /// </summary>
        /// <value>If not set: false</value>
        [Category("Common Properties")]
        [Description("Gets or sets if the items in the tree should be have a streching content")]
        [DefaultValue(false)]
        public bool ItemsContentStretching
        {
            get { return (bool)GetValue(ItemsContentStretchingProperty); }
            set { SetValue(ItemsContentStretchingProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemsContentStretching dependency property
        /// </summary>
        public static readonly DependencyProperty ItemsContentStretchingProperty =
            DependencyProperty.Register("ItemsContentStretching", typeof(bool), typeof(EnhancedTreeView), new UIPropertyMetadata(false));
    }
}
