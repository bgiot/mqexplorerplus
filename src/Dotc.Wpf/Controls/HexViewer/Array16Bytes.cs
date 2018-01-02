using System;
using System.ComponentModel;
using System.Linq;

namespace Dotc.Wpf.Controls.HexViewer
{
    public class Array16Bytes : INotifyPropertyChanged
    {
        internal Array16Bytes(HexViewerModel owner, int rowIndex)
        {
            Owner = owner;
            RowIndex = rowIndex;
            Offset = RowIndex * 16;
            RowSize = Math.Min(16, Owner.Data.Length - Offset);
            ReadData();
        }

        public int RowIndex { get; }
        public int Offset { get; }
        public int RowSize { get; }

        public HexViewerModel Owner { get; }

        private byte? GetValue(int colIndex)
        {
            if (colIndex < RowSize)
                return Owner.Data[Offset + colIndex];
            return null;
        }

        public byte? Byte0 { get; private set; }
        public byte? Byte1 { get; private set; }
        public byte? Byte2 { get; private set; }
        public byte? Byte3 { get; private set; }
        public byte? Byte4 { get; private set; }
        public byte? Byte5 { get; private set; }
        public byte? Byte6 { get; private set; }
        public byte? Byte7 { get; private set; }
        public byte? Byte8 { get; private set; }
        public byte? Byte9 { get; private set; }
        public byte? ByteA { get; private set; }
        public byte? ByteB { get; private set; }
        public byte? ByteC { get; private set; }
        public byte? ByteD { get; private set; }
        public byte? ByteE { get; private set; }
        public byte? ByteF { get; private set; }


        public string Chars { get; private set; }

        private void ReadData()
        {
            GenerateChars();
            Byte0 = GetValue(0);
            Byte1 = GetValue(1);
            Byte2 = GetValue(2);
            Byte3 = GetValue(3);
            Byte4 = GetValue(4);
            Byte5 = GetValue(5);
            Byte6 = GetValue(6);
            Byte7 = GetValue(7);
            Byte8 = GetValue(8);
            Byte9 = GetValue(9);
            ByteA = GetValue(10);
            ByteB = GetValue(11);
            ByteC = GetValue(12);
            ByteD = GetValue(13);
            ByteE = GetValue(14);
            ByteF = GetValue(15);
        }

        private void GenerateChars()
        {
            var bytes = Owner.Data.Skip(Offset).Take(RowSize).ToArray();
            Chars = Owner.CharConverter.ToString(bytes);
            //Chars = new string(Owner.Data.Skip(Offset).Take(RowSize).Select(b => Owner.CharConverter.ToChar(b)).ToArray());
        }

        internal void ConverterChanged()
        {
            GenerateChars();
            OnPropertyChanged("Chars");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
