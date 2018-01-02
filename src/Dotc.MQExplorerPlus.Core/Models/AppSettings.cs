#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQExplorerPlus.Core.Models
{

    public interface IAppSettings
    {


    }

    [Export(typeof(AppSettings)), PartCreationPolicy(CreationPolicy.Shared)]
    public class AppSettings : IAppSettings
    {
        [ImportingConstructor]
        public AppSettings(ISettingsProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            Source = provider.GetAppSettings();
        }


        private IAppSettings Source { get; set; }
    }
}
