#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class GroupElement : FieldElementBase
    {
        public const string ElementName = "group";

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public FieldCollection Fields
        {
            get
            {
                return (FieldCollection)base[""];
            }
        }

        protected override string NodeName
        {
            get { return ElementName; }
        }
    }
}
