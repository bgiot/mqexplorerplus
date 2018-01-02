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
    public class QueuesConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("depthWarningThreshold", DefaultValue = UserSettings.DefaultQueueDepthWarningThreshold)]
        public string DepthWarningThreshold
        {
            get
            {
                return (string)base["depthWarningThreshold"];
            }
            set { base["depthWarningThreshold"] = value; }
        }

    }
}
