#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class PartRefElement : FieldElementBase
    {
        public const string ElementName = "partRef";

        [ConfigurationProperty("partId", IsRequired = true)]
        public string PartId
        {
            get
            {
                return (string)base["partId"];
            }
            set
            {
                base["partId"] = value;
            }
        }
        protected override string NodeName
        {
            get { return ElementName; }
        }

    }
}
