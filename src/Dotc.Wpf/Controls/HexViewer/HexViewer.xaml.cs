using System.Windows;
using System.Windows.Controls;

namespace Dotc.Wpf.Controls.HexViewer
{
    /// <summary>
    /// Interaction logic for HexViewer.xaml
    /// </summary>
    public partial class HexViewer : UserControl
    {
        public HexViewer()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty ByteCharConverterProperty =
            DependencyProperty.Register("ByteCharConverter", typeof(IByteCharConverter), typeof(HexViewer),
            new PropertyMetadata(null, ByteCharConverterPropertyChangedCallback));



        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(HexViewerModel), typeof(HexViewer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, DataPropertyChangedCallback));

        private static void ByteCharConverterPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var converter = dependencyPropertyChangedEventArgs.NewValue as IByteCharConverter;
            if( converter != null)
            {
                var source = (HexViewer)dependencyObject;
                if (source.Data != null && source.Data.CharConverter != converter)
                {
                    source.Data.CharConverter = converter;
                }
            }
        }

        private static void DataPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var value = dependencyPropertyChangedEventArgs.NewValue as HexViewerModel;
            if (value != null)
            {
                var source = (HexViewer)dependencyObject;
                if (source.Data != null && source.ByteCharConverter  !=null && source.Data.CharConverter != source.ByteCharConverter)
                {
                    source.Data.CharConverter = source.ByteCharConverter;
                }
            }
        }

        public IByteCharConverter ByteCharConverter
        {
            get { return (IByteCharConverter)GetValue(ByteCharConverterProperty); }
            set { SetValue(ByteCharConverterProperty, value); }
        }

        public HexViewerModel Data
        {
            get { return (HexViewerModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

    }
}
