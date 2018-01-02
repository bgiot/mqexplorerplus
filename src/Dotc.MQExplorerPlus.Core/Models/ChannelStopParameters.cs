#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQ;
using Dotc.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public class ChannelStopParameters : ValidatableBindableBase
    {
        public ChannelStopParameters()
        {
            Mode = ChannelStopMode.Normal;
            SetInactive = false;
        }
        public ChannelStopMode Mode { get; set; }
        public bool SetInactive { get; set; }
    }
}
