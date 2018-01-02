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
    [ConfigurationCollection(typeof(FieldElementBase))]
    public class FieldCollection : FieldCollectionBase
    {
        protected override void AddNewFieldElement(XmlReader reader)
        {

            if (reader == null) throw new ArgumentNullException(nameof(reader));

            string tagName = reader.Name;

            switch (tagName)
            {
                case GroupElement.ElementName:
                    BaseAdd(FieldElementBase.Create<GroupElement>(reader));
                    break;
                case FieldElement.ElementName:
                    BaseAdd(FieldElementBase.Create<FieldElement>(reader));
                    break;
                case SwitchElement.ElementName:
                    BaseAdd(FieldElementBase.Create<SwitchElement>(reader));
                    break;
                case LoopElement.ElementName:
                    BaseAdd(FieldElementBase.Create<LoopElement>(reader));
                    break;
                case PartRefElement.ElementName:
                    BaseAdd(FieldElementBase.Create<PartRefElement>(reader));
                    break;
                case ConstantElement.ElementName:
                    BaseAdd(FieldElementBase.Create<ConstantElement>(reader));
                    break;
                default:
                    throw new ConfigurationErrorsException(Invariant($"Unsupported tag '{tagName}'"));
            }
        }
    }
}
