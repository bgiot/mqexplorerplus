#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Views;
using System.ComponentModel.Composition;
using Dotc.MQExplorerPlus.Core.Models;
using System.ComponentModel;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(ChannelStopParametersViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ChannelStopParametersViewModel : ModalViewModel
    {
        [ImportingConstructor]
        public ChannelStopParametersViewModel(IChannelStopParametersView view, IApplicationController appController) : base(view, appController)
        {
            Parameters = new ChannelStopParameters();
            Title = "Stop Channel";
        }

        public ChannelStopParameters Parameters { get; set; }

        protected override bool OkAllowed()
        {
            return !Parameters.HasErrors;
        }

    }
}
