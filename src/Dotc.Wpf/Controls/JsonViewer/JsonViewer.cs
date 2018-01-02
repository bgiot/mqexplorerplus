using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json.Linq;

namespace Dotc.Wpf.Controls.JsonViewer
{

    public sealed class JTokenTreeItem : INotifyPropertyChanged
    {

        public static JTokenTreeItem[] Build(JToken source)
        {
            var node = new JTokenTreeItem(source);
            return new JTokenTreeItem[] { node };
        }

        public JTokenType Type
        {
            get { return Token.Type; }
        }

        private bool _isExpended;
        private JTokenTreeItem(JToken jt)
        {
            Token = jt;
            if (jt.Children().Any())
            {
                Children = new List<JTokenTreeItem>();
                foreach (var subjt in jt.Children())
                {
                    Children.Add(new JTokenTreeItem(subjt));
                }
            }
        }

        public JToken Token { get; private set; }

        public bool IsRoot
        {
            get { return Token.Parent == null; }
        }

        public IList<JTokenTreeItem> Children { get; private set; }

        public bool IsExpanded
        {
            get { return _isExpended; }
            set
            {
                _isExpended = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsExpanded"));

            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class JsonViewerStyle
    {

    }

    [TemplatePart(Name = "PART_TreeView", Type = typeof(TreeView))]
    public partial class JsonViewer : Control
    {
        static JsonViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JsonViewer), new FrameworkPropertyMetadata(typeof(JsonViewer)));
        }

        private TreeView _treeView;
        private const GeneratorStatus Generated = GeneratorStatus.ContainersGenerated;

        public static readonly DependencyProperty RootTokenProperty =
                DependencyProperty.Register("RootToken", typeof(JToken), typeof(JsonViewer),
                new UIPropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var ctl = (JsonViewer)dependencyObject;

            ctl.SetItemSource();

        }

        private void SetItemSource()
        {

            if (_treeView != null)
            {
                if (RootToken == null)
                    _treeView.ItemsSource = null;
                else
                {
                    var items = JTokenTreeItem.Build(RootToken);
                    items[0].IsExpanded = true;
                    _treeView.ItemsSource = items;
                }

            }
        }

        public JToken RootToken
        {
            get { return (JToken)GetValue(RootTokenProperty); }
            set { SetValue(RootTokenProperty, value); }
        }
        public void ExpandAll()
        {
            ToggleItems(true);
        }

        public void CollapseAll()
        {
            ToggleItems(false);
        }

        private void ToggleItems(bool isExpanded)
        {
            if (_treeView.Items.IsEmpty)
                return;

            RecurseToggle((JTokenTreeItem)_treeView.Items[0], isExpanded);

        }

        private void RecurseToggle(JTokenTreeItem item, bool isExpanded)
        {
            item.IsExpanded = isExpanded;
            if (item.Children != null)
            {
                foreach (var jtti in item.Children)
                {
                    RecurseToggle(jtti, isExpanded);
                }
            }
        }


        public override void OnApplyTemplate()
        {
            ApplyTemplate();

            _treeView = GetTemplateChild("PART_TreeView") as TreeView;

            SetItemSource();

        }
    }
}
