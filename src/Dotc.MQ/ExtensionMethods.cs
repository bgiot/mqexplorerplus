#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQ
{
    public static class ExtensionMethods
    {

        public static void AddOrReplace(this IDictionary dic, object key, object value)
        {
            if (dic != null)
            {
                if (dic.Contains(key))
                {
                    dic[key] = value;
                }
                else
                {
                    dic.Add(key, value);
                }
            }
        }

        public static byte[] HexStringToBytes(this string hexString)
        {
            if (hexString == null) throw new ArgumentNullException(nameof(hexString));
            var data = new byte[hexString.Length >> 1];

            for (var i = 0; i < (hexString.Length >> 1); ++i)
            {
                data[i] = (byte)((GetHexVal(hexString[i << 1]) << 4) + (GetHexVal(hexString[(i << 1) + 1])));
            }

            return data;
        }

        private static int GetHexVal(char hex)
        {
            int val = hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static string ToHexString(this IEnumerable<byte> data, bool addPrefix = false)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var sb = new StringBuilder();
            foreach (var b in data)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture,  "{1}{0:X2}", b, addPrefix ? "0x" : "");
            }
            return sb.ToString();
        }

        public static string ToString(this byte[] data, Encoding encoding)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            return encoding.GetString(data).TrimEnd(new char[1]);
        }

        public static void ToBytes(this string s, ref byte[] buffer, Encoding encoding)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            encoding.GetBytes(s.PadRight(buffer.Length, '\0'), 0, buffer.Length, buffer, 0);
        }
    }
}
