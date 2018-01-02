#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class FieldElement : FieldElementBase
    {
        public const string ElementName = "field";

        [ConfigurationProperty("length", IsRequired=true)]
        public int Length
        {
            get
            {
                return (int)base["length"];
            }
            set
            {
                base["length"] = value;
            }
        }

        [ConfigurationProperty("id", DefaultValue = "*")]
        public string Id
        {
            get
            {
                return (string)base["id"];
            }
            set
            {
                base["id"] = value;
            }
        }
        protected override string NodeName
        {
            get { return ElementName; }
        }
    }
}
