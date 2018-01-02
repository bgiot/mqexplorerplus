#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public abstract class FieldElementBase : ElementBase
    {

        protected abstract string NodeName { get; }

        [ConfigurationProperty("label", IsRequired = true)]
        public string Label
        {
            get
            {
                return (string)base["label"];
            }
            set
            {
                base["label"] = value;
            }
        }

        public static T Create<T>(System.Xml.XmlReader reader) where T : FieldElementBase, new()
        {
            T obj = new T();
            obj.DeserializeElement(reader, false);
            return obj;
        }

        protected override bool SerializeToXmlElement(System.Xml.XmlWriter writer, string elementName)
        {
            return base.SerializeToXmlElement(writer, NodeName);
        }
    }
}
