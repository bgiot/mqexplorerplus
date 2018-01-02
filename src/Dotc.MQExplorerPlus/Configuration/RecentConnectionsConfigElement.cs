#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Configuration;
using System.Xml;
using Dotc.MQExplorerPlus.Core.Models;
using System.ComponentModel;
using System.Collections.Generic;

namespace Dotc.MQExplorerPlus.Configuration
{
    [ConfigurationCollection(typeof(RecentConnectionConfigElement))]
    public class RecentConnectionsConfigCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return null;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RecentConnectionConfigElement)element).Id;
        }

        public void Add(RecentConnectionConfigElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            RecentConnectionConfigElement element = null;

            switch (elementName)
            {
                case "localManager":
                    element = new RecentConnectionConfigElement();
                    break;
                case "localQueue":
                    element = new RecentQueueConnectionConfigElement();
                    break;
                case "remoteManager":
                    element = new RecentRemoteConnectionConfigElement();
                    break;
                case "remoteQueue":
                    element = new RecentRemoteQueueConnectionConfigElement();
                    break;

            }

            if (element != null)
            {
                element.DeserializeElementInternal(reader);
                base.BaseAdd(element);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class RecentConnectionConfigElement : ConfigurationElement
    {
        internal RecentConnectionConfigElement(RecentQueueManagerConnection rc) : this()
        {
            QueueManagerName = rc.QueueManagerName;
            FilterPrefix = rc.ObjectNamePrefix;
            if (rc.QueueList != null && rc.QueueList.Count >0)
            {
                QueueList = new CommaDelimitedStringCollection();
                QueueList.AddRange(rc.QueueList.ToArray());
            }

        }
        public RecentConnectionConfigElement()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; }

        internal void DeserializeElementInternal(XmlReader reader)
        {
            base.DeserializeElement(reader, false);
        }

        public virtual string ElementName
        {
            get { return "localManager"; }
        }

        [ConfigurationProperty("manager", IsRequired = true)]
        public string QueueManagerName
        {
            get { return (string)base["manager"]; }
            set { base["manager"] = value; }
        }

        [ConfigurationProperty("filter", IsRequired = false, DefaultValue = null)]
        public string FilterPrefix
        {
            get { return (string)base["filter"]; }
            set { base["filter"] = value; }
        }

        [ConfigurationProperty("queues", IsRequired = false, DefaultValue = null)]
        [TypeConverter(typeof(CommaDelimitedStringCollectionConverter))]
        public CommaDelimitedStringCollection QueueList
        {
            get { return (CommaDelimitedStringCollection)base["queues"]; }
            set { base["queues"] = value; }
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            return base.SerializeToXmlElement(writer, this.ElementName);
        }

        public virtual RecentConnection ConvertToModel()
        {
            var item = new RecentQueueManagerConnection
            {
                QueueManagerName = this.QueueManagerName,
                ObjectNamePrefix = this.FilterPrefix,
            };
            if (this.QueueList != null && this.QueueList.Count > 0)
            {
                item.QueueList = new List<string>();
                foreach( string x in this.QueueList)
                {
                    item.QueueList.Add(x);
                }
            }
            return item;
        }

        public static RecentConnectionConfigElement ConvertFromModel(RecentConnection rc)
        {
            if (rc is RecentRemoteQueueConnection)
                return new RecentRemoteQueueConnectionConfigElement((RecentRemoteQueueConnection)rc);

            if (rc is RecentRemoteQueueManagerConnection)
                return new RecentRemoteConnectionConfigElement((RecentRemoteQueueManagerConnection)rc);

            if (rc is RecentQueueConnection)
                return new RecentQueueConnectionConfigElement((RecentQueueConnection)rc);

            if (rc is RecentQueueManagerConnection)
                return new RecentConnectionConfigElement((RecentQueueManagerConnection)rc);

            throw new NotSupportedException("Invalid RecentConnection type");
        }
    }

    public class RecentQueueConnectionConfigElement : RecentConnectionConfigElement
    {

        public RecentQueueConnectionConfigElement() : base()
        { }
        internal RecentQueueConnectionConfigElement(RecentQueueConnection rc) : this()
        {
            QueueManagerName = rc.QueueManagerName;
            QueueName = rc.QueueName;
        }

        [ConfigurationProperty("queue", IsRequired = true)]
        public string QueueName
        {
            get { return (string)base["queue"]; }
            set { base["queue"] = value; }
        }

        public override string ElementName
        {
            get
            {
                return "localQueue";
            }
        }


        public override RecentConnection ConvertToModel()
        {
            return new RecentQueueConnection
            {
                QueueManagerName = this.QueueManagerName,
                QueueName = this.QueueName
            };
        }
    }

    public class RecentRemoteConnectionConfigElement : RecentConnectionConfigElement
    {


        public RecentRemoteConnectionConfigElement() : base()
        { }
        internal RecentRemoteConnectionConfigElement(RecentRemoteQueueManagerConnection rc) : this()
        {
            QueueManagerName = rc.QueueManagerName;
            FilterPrefix = rc.ObjectNamePrefix;
            if (rc.QueueList != null && rc.QueueList.Count > 0)
            {
                QueueList = new CommaDelimitedStringCollection();
                QueueList.AddRange(rc.QueueList.ToArray());
            }
            Host = rc.HostName;
            Port = rc.Port;
            Channel = rc.Channel;
            UserId = rc.UserId;
        }

        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get { return (string)base["host"]; }
            set { base["host"] = value; }
        }

        [ConfigurationProperty("channel", IsRequired = true)]
        public string Channel
        {
            get { return (string)base["channel"]; }
            set { base["channel"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)base["port"]; }
            set { base["port"] = value; }
        }

        [ConfigurationProperty("userid", IsRequired = false)]
        public string UserId
        {
            get { return (string)base["userid"]; }
            set { base["userid"] = value; }
        }


        public override string ElementName
        {
            get
            {
                return "remoteManager";
            }
        }
        public override RecentConnection ConvertToModel()
        {
            var item = new RecentRemoteQueueManagerConnection
            {
                QueueManagerName = this.QueueManagerName,
                ObjectNamePrefix = this.FilterPrefix,
                HostName = this.Host,
                Channel = this.Channel,
                Port = this.Port,
                UserId = this.UserId

            };
            if (this.QueueList != null && this.QueueList.Count > 0)
            {
                item.QueueList = new List<string>();
                foreach (string x in this.QueueList)
                {
                    item.QueueList.Add(x);
                }
            }
            return item;
        }
    }

    public class RecentRemoteQueueConnectionConfigElement : RecentRemoteConnectionConfigElement
    {

        public RecentRemoteQueueConnectionConfigElement() : base()
        { }
        internal RecentRemoteQueueConnectionConfigElement(RecentRemoteQueueConnection rc) : this()
        {
            QueueManagerName = rc.QueueManagerName;
            QueueName = rc.QueueName;
            QueueName = rc.QueueName;
            Host = rc.HostName;
            Port = rc.Port;
            Channel = rc.Channel;
            UserId = rc.UserId;
        }

        [ConfigurationProperty("queue", IsRequired = true)]
        public string QueueName
        {
            get { return (string)base["queue"]; }
            set { base["queue"] = value; }
        }


        public override string ElementName
        {
            get
            {
                return "remoteQueue";
            }
        }

        public override RecentConnection ConvertToModel()
        {
            return new RecentRemoteQueueConnection
            {
                QueueManagerName = this.QueueManagerName,
                ObjectNamePrefix = this.FilterPrefix,
                HostName = this.Host,
                Channel = this.Channel,
                Port = this.Port,
                UserId = this.UserId,
                QueueName = this.QueueName

            };
        }
    }
}

