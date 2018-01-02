using System;
using System.Globalization;

namespace Dotc.Common
{
    public static class StringFormatExtensions
    {

        public static string Url(this FormattableString formattable)
        {
            return formattable.ToString(new UrlFormatProvider());
        }

        public static string Sql(this FormattableString formattable)
        {
            return formattable.ToString(new SqlFormatProvider());
        }
    }
}


namespace System.Runtime.CompilerServices
{
    public class FormattableStringFactory
    {
        public static FormattableString Create(string messageFormat, params object[] args)
        {
            return new FormattableString(messageFormat, args);
        }

        public static FormattableString Create(string messageFormat, DateTime bad, params object[] args)
        {
            var realArgs = new object[args.Length + 1];
            realArgs[0] = "Please don't use DateTime";
            Array.Copy(args, 0, realArgs, 1, args.Length);
            return new FormattableString(messageFormat, realArgs);
        }
    }
}

namespace System
{
    public class FormattableString : IFormattable
    {
        private readonly string _messageFormat;
        private readonly object[] _args;

        public static string Invariant(FormattableString formattable)
        {
            return formattable.ToString(CultureInfo.InvariantCulture);
        }

        public FormattableString(string messageFormat, object[] args)
        {
            _messageFormat = messageFormat;
            _args = args;
        }
        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, _messageFormat, _args);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString(formatProvider);
        }
    }
}
