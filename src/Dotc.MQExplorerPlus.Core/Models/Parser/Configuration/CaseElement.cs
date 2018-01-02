#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class CaseElement : GroupElement
    {
        public new const string ElementName = "case";

        [ConfigurationProperty("when", IsRequired=true)]
        public string When
        {
            get
            {
                return (string)base["when"];
            }
            set
            {
                base["when"] = value;
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
