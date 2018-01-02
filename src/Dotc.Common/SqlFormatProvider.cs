using System;
using System.Globalization;

namespace Dotc.Common
{
    public class SqlFormatProvider : IFormatProvider
    {
        private readonly SqlFormatter _formatter = new SqlFormatter();

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? _formatter : null;
        }

        class SqlFormatter : ICustomFormatter
        {
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg == null)
                    return "NULL";
                var s = arg as string;
                if (s != null)
                    return "'" + s.Replace("'", "''") + "'";
                if (arg is DateTime)
                    return "'" + ((DateTime)arg).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) + "'";
                var formattable = arg as IFormattable;
                if (formattable != null)
                    return formattable.ToString(format, CultureInfo.InvariantCulture);
                return arg.ToString();
            }
        }
    }
}
