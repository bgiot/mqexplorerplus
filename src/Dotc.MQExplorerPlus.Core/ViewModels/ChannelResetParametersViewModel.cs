#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Views;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public class ChannelResetParametersViewModel : ModalViewModel
    {
        public ChannelResetParametersViewModel(IChannelResetParametersView view, IApplicationController appController) : base(view, appController)
        {
            Parameters = new ChannelResetParameters();
            Title = "Reset Channel";
        }

        public ChannelResetParameters Parameters { get; set; }

        protected override bool OkAllowed()
        {
            return !Parameters.HasErrors;
        }
    }
}
