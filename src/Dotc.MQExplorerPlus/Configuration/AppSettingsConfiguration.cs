#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;

namespace Dotc.MQExplorerPlus.Configuration
{
    public class AppSettingsConfiguration : ConfigurationSection
    {
        public const string SectionName = "mqexplorerplus";

        public static AppSettingsConfiguration Current
        {
            get
            {
                return (AppSettingsConfiguration)ConfigurationManager.GetSection(SectionName);
            }
        }

    }
}
