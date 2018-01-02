using System;

namespace Dotc.Common
{
    public class UrlFormatProvider : IFormatProvider
    {
        private readonly UrlFormatter _formatter = new UrlFormatter();

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? _formatter : null;
        }

        class UrlFormatter : ICustomFormatter
        {
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg == null)
                    return string.Empty;
                return format == "r" ? arg.ToString() : Uri.EscapeDataString(arg.ToString());
            }
        }
    }
}
