#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(ChannelResolveParametersViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ChannelResolveParametersViewModel : ModalViewModel
    {
        [ImportingConstructor]
        public ChannelResolveParametersViewModel(IChannelResolveParametersView view, IApplicationController appController) : base(view, appController)
        {
            Parameters = new ChannelResolveParameters();
            Title = "Resolve Channel";
        }

        public ChannelResolveParameters Parameters { get; set; }

        protected override bool OkAllowed()
        {
            return !Parameters.HasErrors;
        }
    }
}
