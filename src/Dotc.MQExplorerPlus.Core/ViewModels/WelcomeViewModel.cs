#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel.Composition;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Views;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(WelcomeViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
    public class WelcomeViewModel : ViewModel
    {
        [ImportingConstructor]
        public WelcomeViewModel(IWelcomeView view, IApplicationController appController) : base(view, appController)
        {
        }

        public string Version => ApplicationInfo.Version;
    }
}
