#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class PartElement : ElementBase
    {
        [ConfigurationProperty("id", IsRequired = true, IsKey = true)]
        public string Id
        {
            get { return (string) base["id"]; }
            set { base["id"] = value; }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public PartFieldCollection Fields
        {
            get
            {
                return (PartFieldCollection)base[""];
            }
        }

    }
}
