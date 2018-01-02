using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace Dotc.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for XmlViewer.xaml
    /// </summary>
    public partial class XmlViewer //: UserControl
    {
        public XmlViewer()
        {
            InitializeComponent();
        }

        public XmlDocument Document
        {
            private get { return (XmlDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(XmlDocument), typeof(XmlViewer),
            new FrameworkPropertyMetadata(null, OnDocumentChanged));

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var xmlV = d as XmlViewer;
            if (xmlV != null)
            {
                if (xmlV.Document == null)
                {
                    xmlV.XmlTree.ItemsSource = null;
                }
                else
                {
                    var provider = new XmlDataProvider {Document = xmlV.Document};
                    var b = new Binding
                    {
                        Source = provider,
                        XPath = "child::node()"
                    };

                    xmlV.XmlTree.SetBinding(ItemsControl.ItemsSourceProperty, b);
                }
            }
        }

    }
}
