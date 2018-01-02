#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;
using Dotc.MQExplorerPlus.Core.Models;

namespace Dotc.MQExplorerPlus.Configuration
{
    public class GeneralConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("autoRefreshInterval", DefaultValue = UserSettings.DefaultAutoRefreshInterval)]
        public int AutoRefreshInterval
        {
            get
            {
                return (int)base["autoRefreshInterval"];
            }
            set
            {
                base["autoRefreshInterval"] = value;
            }
        }
    }
}
