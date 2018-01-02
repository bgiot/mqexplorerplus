#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class LoopElement : GroupElement
    {

        public new const string ElementName = "loop";

        [ConfigurationProperty("from", IsRequired=true)]
        public string From
        {
            get
            {
                return (string)base["from"];
            }
            set
            {
                base["from"] = value;
            }
        }

        [ConfigurationProperty("to", IsRequired = true)]
        public string To
        {
            get
            {
                return (string)base["to"];
            }
            set
            {
                base["to"] = value;
            }
        }

        [ConfigurationProperty("step", DefaultValue = "1")]
        public string Step
        {
            get
            {
                return (string)base["step"];
            }
            set
            {
                base["step"] = value;
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
