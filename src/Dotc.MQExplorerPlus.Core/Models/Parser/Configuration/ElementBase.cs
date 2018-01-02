#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class ElementBase : ConfigurationElement
    {

        public ElementBase()
        {
            UniqueId = Guid.NewGuid();
        }

        public Guid UniqueId { get; private set; }
    }
}
