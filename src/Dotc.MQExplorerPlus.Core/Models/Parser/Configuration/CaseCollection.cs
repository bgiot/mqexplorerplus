#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Configuration;
using System.Xml;
using static System.FormattableString;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    [ConfigurationCollection(typeof (GroupElement))]
    public class CaseCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new GroupElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GroupElement) element).UniqueId;
        }

        private bool _elseIsSet;

        public void Add(GroupElement ce)
        {
            if (ce is ElseElement)
            {
                if (_elseIsSet)
                    throw new ConfigurationErrorsException("Only one 'else' node can be set inside a 'switch' node");
                _elseIsSet = true;
            }

            BaseAdd(ce);
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

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            return DeserializeElementCore(reader);
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            DeserializeElementCore(reader);
        }

        private void AddNewFieldElement(XmlReader reader)
        {

            if (reader == null) throw new ArgumentNullException(nameof(reader));

            string tagName = reader.Name;

            switch (tagName)
            {
                case CaseElement.ElementName:
                    Add(FieldElementBase.Create<CaseElement>(reader));
                    break;
                case ElseElement.ElementName:
                    Add(FieldElementBase.Create<ElseElement>(reader));
                    break;
                default:
                    throw new ConfigurationErrorsException(Invariant($"Unsupported tag '{tagName}'"));
            }
        }
    }
}
