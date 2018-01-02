using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace Dotc.Wpf.Controls.JsonViewer.ValueConverters
{
    public sealed class JArrayLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var jToken = value as JToken;
            if(jToken == null)
                throw new ArgumentException("Wrong type for this converter");

            switch (jToken.Type)
            {
                case JTokenType.Array:
                    var arrayLen = jToken.Children().Count();
                    return string.Format(CultureInfo.InvariantCulture,  "[{0}]", arrayLen);
                case JTokenType.Property:
                    var propertyArrayLen = jToken.Children().First().Children().Count();
                    return string.Format(CultureInfo.InvariantCulture, "[ {0} ]", propertyArrayLen);
                default:
                    throw new ArgumentException("Type should be JProperty or JArray");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
        }
    }
}
