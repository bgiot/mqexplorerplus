#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public class ChannelResetParameters : ValidatableBindableBase
    {
        public ChannelResetParameters()
        {
            _msgSequenceNumber = 1;
        }

        private int? _msgSequenceNumber;

        [Required(AllowEmptyStrings =false)]
        [Range(1, 999999999)]
        public int? MessageSequenceNumber
        {
            get { return _msgSequenceNumber; }
            set { SetPropertyAndNotify(ref _msgSequenceNumber, value); }
        }
    }
}
