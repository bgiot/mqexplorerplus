using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Dotc.Wpf.Controls.HexViewer
{
    public class HexViewerModel : INotifyPropertyChanged
    {
        public HexViewerModel(byte[] data, IByteCharConverter converter = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            CharConverter = converter ?? new DefaultByteCharConverter();

            Data = data;
            var rowCount =data.Length / 16;
            if (data.Length % 16 != 0)
                rowCount++;

            Rows = new ObservableCollection<Array16Bytes>();
            for (var i=0; i < rowCount ;i ++)
            {
                Rows.Add(new Array16Bytes(this, i));
            }
        }

        public byte[] Data { get; private set; }

        public ObservableCollection<Array16Bytes> Rows { get; }

        private IByteCharConverter _charConverter;
        public IByteCharConverter CharConverter 
        { 
            get { return _charConverter; }
            set 
            { 
                if (value != _charConverter)
                {
                    _charConverter = value;
                    OnPropertyChanged("CharConverter");
                    if (Rows != null)
                        foreach (var row in Rows) row.ConverterChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
