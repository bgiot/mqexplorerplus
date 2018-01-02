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
    [Export(typeof(ChannelResetParametersViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ChannelResetParametersViewModel : ModalViewModel
    {
        [ImportingConstructor]
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
