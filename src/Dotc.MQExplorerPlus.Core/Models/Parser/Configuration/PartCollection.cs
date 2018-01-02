#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    [ConfigurationCollection(typeof(PartElement), AddItemName="part")]
    public class PartCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PartElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PartElement)element).Id;
        }

        public bool TryFindById(string id, out PartElement result)
        {
            result = BaseGet(id) as PartElement;
            return (result != null);
        }

        public void Add(PartElement pe)
        {
            BaseAdd(pe);
        }
    }
}
