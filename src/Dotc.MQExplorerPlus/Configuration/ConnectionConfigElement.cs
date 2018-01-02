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
    public class ConnectionConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("port", DefaultValue = UserSettings.DefaultPort)]
        public int Port
        {
            get
            {
                return (int)base["port"];
            }
            set
            {
                base["port"] = value;
            }
        }

        [ConfigurationProperty("channel", DefaultValue = UserSettings.DefaultChannel)]
        public string Channel
        {
            get
            {
                return (string)base["channel"];
            }
            set
            {
                base["channel"] = value;
            }
        }

        [ConfigurationProperty("maxRecentList", DefaultValue = UserSettings.DefaultMaxRecentConnections)]
        public int MaxRecentConnections
        {
            get
            {
                return (int)base["maxRecentList"];
            }
            set
            {
                base["maxRecentList"] = value;
            }
        }

        [ConfigurationProperty("recentList")]
        public RecentConnectionsConfigCollection RecentList
        {
            get { return (RecentConnectionsConfigCollection) base["recentList"]; }
        }
    }
}
