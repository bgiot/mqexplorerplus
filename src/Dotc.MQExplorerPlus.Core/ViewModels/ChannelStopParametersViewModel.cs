#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.MQExplorerPlus.Core.Models;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{

    public class ChannelStopParametersViewModel : ModalViewModel
    {

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
