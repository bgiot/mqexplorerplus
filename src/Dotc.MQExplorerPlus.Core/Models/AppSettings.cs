#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Services;
using System;

namespace Dotc.MQExplorerPlus.Core.Models
{

    public interface IAppSettings
    {


    }

    public class AppSettings : IAppSettings
    {

        public AppSettings(ISettingsProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            Source = provider.GetAppSettings();
        }


        private IAppSettings Source { get; set; }
    }
}
