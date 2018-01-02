#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Xml;
using Dotc.MQExplorerPlus.Core.Models;

namespace Dotc.MQExplorerPlus.Configuration
{
    public class UserSettingsConfiguration : ConfigurationSection, IUserSettings
    {
        public const string SectionName = "mqexplorerplus";

        [ConfigurationProperty("general")]
        public GeneralConfigElement General
        {
            get
            {
                return (GeneralConfigElement)base["general"];
            }
            set
            {
                base["general"] = value;
            }
        }

        [ConfigurationProperty("messages")]
        public MessagesConfigElement Messages
        {
            get
            {
                return (MessagesConfigElement)base["messages"];
            }
            set
            {
                base["messages"] = value;
            }
        }

        [ConfigurationProperty("queues")]
        public QueuesConfigElement Queues
        {
            get
            {
                return (QueuesConfigElement)base["queues"];
            }
            set
            {
                base["queues"] = value;
            }
        }

        [ConfigurationProperty("connection")]
        public ConnectionConfigElement Connection
        {
            get
            {
                return (ConnectionConfigElement)base["connection"];
            }
            set
            {
                base["connection"] = value;
            }
        }

        internal void Set(IUserSettings settings)
        {
            General.AutoRefreshInterval = settings.AutoRefreshInterval;
            Messages.BrowseLimit = settings.BrowseLimit;
            Messages.PutPriority = settings.PutPriority;
            Connection.Port = settings.Port;
            Connection.Channel = settings.Channel;
            Connection.MaxRecentConnections = settings.MaxRecentConnections;
            Queues.DepthWarningThreshold = settings.QueueDepthWarningThreshold;
            SetRecentConnectionsInternal(settings.RecentConnections);
        }

        public void Serialize(XmlWriter writer)
        {
            base.SerializeToXmlElement(writer, SectionName);
        }

        public void Deserialize(XmlReader reader)
        {
            base.DeserializeSection(reader);
        }

        public int AutoRefreshInterval => General.AutoRefreshInterval;

        public int BrowseLimit => Messages.BrowseLimit;

        public int PutPriority => Messages.PutPriority;

        public int Port => Connection.Port;

        public string Channel => Connection.Channel;

        public string QueueDepthWarningThreshold => Queues.DepthWarningThreshold;

        public int MaxRecentConnections => Connection.MaxRecentConnections;

        public ObservableCollection<RecentConnection> RecentConnections
        {
            get { return new ObservableCollection<RecentConnection>(GetRecentConnectionsInternal()); }
        }

        private List<RecentConnection> GetRecentConnectionsInternal()
        {
            var result = new List<RecentConnection>();
            int index = 0;
            foreach (RecentConnectionConfigElement x in  Connection.RecentList)
            {
                var o = x.ConvertToModel();
                o.Index = ++index;
                result.Add(o);
            }
            return result;
        }

        private void SetRecentConnectionsInternal(ObservableCollection<RecentConnection> rc)
        {
            Connection.RecentList.Clear();
            foreach (RecentConnection x in rc)
            {
                var item = RecentConnectionConfigElement.ConvertFromModel(x);
                Connection.RecentList.Add(item);
            }
        }
    }
}
