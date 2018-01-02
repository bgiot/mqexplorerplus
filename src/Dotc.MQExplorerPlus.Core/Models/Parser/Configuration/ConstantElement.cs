#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class ConstantElement : FieldElementBase
    {
        public const string ElementName = "const";

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)base["value"];
            }
            set
            {
                base["value"] = value;
            }
        }
        protected override string NodeName
        {
            get { return ElementName; }
        }
    }
}
