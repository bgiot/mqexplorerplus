#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public class ChannelResolveParameters : ValidatableBindableBase
    {
        public ChannelResolveParameters()
        {
            Commit = false;
        }

        public bool Commit { get; set; }
    }
}
