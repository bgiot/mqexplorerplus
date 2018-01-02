#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class SwitchElement : FieldElementBase
    {
        public const string ElementName = "switch";

        [ConfigurationProperty("on", IsRequired = true)]
        public string On
        {
            get
            {
                return (string)base["on"];
            }
            set
            {
                base["on"] = value;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public CaseCollection Cases
        {
            get
            {
                return (CaseCollection)base[""];
            }
        }

        protected override string NodeName
        {
            get
            {
                return ElementName;
            }
        }
    }
}
