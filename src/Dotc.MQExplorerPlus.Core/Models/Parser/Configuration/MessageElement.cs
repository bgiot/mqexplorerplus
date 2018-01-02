#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class MessageElement : ElementBase
    {

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public FieldCollection Fields
        {
            get
            {
                return (FieldCollection)base[""];
            }
        }

    }
}
