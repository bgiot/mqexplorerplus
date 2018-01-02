using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace Dotc.Wpf.Controls.JsonViewer.TemplateSelectors
{
    public sealed class JPropertyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PrimitivePropertyTemplate { get; set; }
        public DataTemplate ComplexPropertyTemplate { get; set; }
        public DataTemplate ArrayPropertyTemplate { get; set; }
        public DataTemplate ObjectPropertyTemplate { get; set; }
        public DataTemplate ArrayTemplate { get; set; }
        public DataTemplate ObjectTemplate { get; set; }
        public DataTemplate ValueTemplate { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item == null)
                return null;

            var frameworkElement = container as FrameworkElement;
            if(frameworkElement == null)
                return null;

            var tvItem = (JTokenTreeItem) item;
            var type = tvItem.Token.GetType();
            if (type == typeof(JProperty))
            {
                var jProperty = tvItem.Token as JProperty;
                switch (jProperty.Value.Type)
                {
                    case JTokenType.Object:
                        return frameworkElement.FindResource("ObjectPropertyTemplate") as DataTemplate;
                    case JTokenType.Array:
                        return frameworkElement.FindResource("ArrayPropertyTemplate") as DataTemplate;
                    default:
                        return frameworkElement.FindResource("PrimitivePropertyTemplate") as DataTemplate;

                }
            }
            if (type == typeof(JObject))
                return frameworkElement.FindResource("ObjectTemplate") as DataTemplate;

            if (type == typeof(JArray))
                return frameworkElement.FindResource("ArrayTemplate") as DataTemplate;

            if (type == typeof(JValue))
                return frameworkElement.FindResource("ValueTemplate") as DataTemplate;

            throw new NotSupportedException();
        }
    }
}
