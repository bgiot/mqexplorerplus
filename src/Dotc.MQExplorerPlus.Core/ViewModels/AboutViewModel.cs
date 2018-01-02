#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Views;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Dotc.MQExplorerPlus.Core.Models;
using System.Windows;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(AboutViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class AboutViewModel : ModalViewModel
    {
        [ImportingConstructor]
        public AboutViewModel(IAboutView view, IApplicationController appc)
            : base(view, appc)
        {
            Title = "About MQ Explorer Plus";
        }


        public override bool ShowDefaultButtons
        {
            get
            {
                return false;
            }
        }

        public string ProductName => ApplicationInfo.ProductName;
        public string Version => ApplicationInfo.Version;
        public string Copyright => ApplicationInfo.Copyright;
        public string MqVersion => App.MqController.GetMqSoftwareVersion();

        public override void OnOpened()
        {
        }

    }
}
