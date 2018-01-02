using System;
using System.Text;

namespace Dotc.Wpf.Controls.HexViewer
{
    public interface IByteCharConverter
    {
        char ToChar(byte b);
        byte ToByte(char c);

        string ToString(byte[] b);
    }

    public sealed class DefaultByteCharConverter : IByteCharConverter
    {
        private readonly Encoding _encoding = Encoding.GetEncoding(1252);

        public char ToChar(byte b)
        {
            if (b < 0x20 || (b > 0x7E && b < 0xA0))
                return '.';

            var encoded = _encoding.GetChars(new byte[] { b });
            return encoded.Length > 0 ? encoded[0] : '.';

            //return b > 0x1F && !(b > 0x7E && b < 0xA0) ? (char)b : '.';
        }

        public byte ToByte(char c)
        {
            var decoded = _encoding.GetBytes(new[] { c });
            return decoded.Length > 0 ? decoded[0] : (byte)0;
            //return (byte)c;
        }

        public override string ToString()
        {
            return "CP-1252 (Default)";
        }

        public string ToString(byte[] b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            char[] chars = new char[b.Length];
            for (int i = 0; i < b.Length; i++)
                chars[i] = ToChar(b[i]);
            return new string(chars);
        }
    }

    public sealed class EbcdicByteCharConverter : IByteCharConverter
    {

        private readonly Encoding _ebcdicEncoding = Encoding.GetEncoding(500);

        public char ToChar(byte b)
        {
            var encoded = _ebcdicEncoding.GetString(new[] { b });
            return encoded.Length > 0 ? encoded[0] : '.';
        }

        public byte ToByte(char c)
        {
            var decoded = _ebcdicEncoding.GetBytes(new[] { c });
            return decoded.Length > 0 ? decoded[0] : (byte)0;
        }

        public override string ToString()
        {
            return "EBCDIC (Code Page 500)";
        }
        public string ToString(byte[] b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            char[] chars = new char[b.Length];
            for (int i = 0; i < b.Length; i++)
                chars[i] = ToChar(b[i]);
            return new string(chars);
        }
    }
}
