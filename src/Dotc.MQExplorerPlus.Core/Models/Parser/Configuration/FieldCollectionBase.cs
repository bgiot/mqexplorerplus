#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;
using System.Xml;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    [ConfigurationCollection(typeof(FieldElementBase))]

    public abstract class FieldCollectionBase : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return null;
        }

        public void Add(FieldElementBase feb)
        {
            BaseAdd(feb);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FieldElementBase)element).UniqueId;
        }

        private bool DeserializeElementCore(XmlReader reader)
        {
            if (XmlNodeType.Element != reader.NodeType)
            {
                reader.MoveToElement();
            }

            var subtreeReader = reader.ReadSubtree();

            while (subtreeReader.Read())
            {
                if (subtreeReader.NodeType == XmlNodeType.Element)
                {
                    AddNewFieldElement(subtreeReader);
                }
            }

            return true;
        }

        protected abstract void AddNewFieldElement(XmlReader reader);

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            return DeserializeElementCore(reader);
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            DeserializeElementCore(reader);
        }
    }
}
