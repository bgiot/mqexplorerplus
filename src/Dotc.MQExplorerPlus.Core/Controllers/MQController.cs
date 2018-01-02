#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel.Composition;
using System.Security;
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core.Models;

namespace Dotc.MQExplorerPlus.Core.Controllers
{
    [Export(typeof(MqController)), PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class MqController
    {
        private readonly IQueueManagerFactory _mqFactory;

        [ImportingConstructor]
        public MqController(IQueueManagerFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _mqFactory = factory;
        }

        public string TryGetDefaultQueueManagerName()
        {
            try
            {
                var mq = _mqFactory.Connect();
                var result = mq.Name;
                mq.Disconnect();
                return result.TrimEnd();
            }
            catch (MqException)
            {
                return null;
            }
        }

        public IQueueManager ConnectQueueManager(string qmName, IConnectionProperties cp = null)
        {
            return cp == null ? _mqFactory.Connect(qmName) : _mqFactory.Connect(qmName, cp);
        }

        public IConnectionProperties CreateConnectionProperties(string hostName, int port, string channel,
            string userId = null, SecureString password = null)
        {
            var cp = _mqFactory.NewConnectionProperties();
            cp.Set(hostName, port, channel, userId, password);
            return cp;
        }

        public bool CheckLocalQueueManagerNameIsValid(string queueManagerName)
        {
            if (string.IsNullOrEmpty(queueManagerName)) throw new ArgumentNullException(nameof(queueManagerName));

            try
            {
                var qm = _mqFactory.Connect(queueManagerName);
                if (qm != null)
                {
                    qm.Disconnect();
                    return true;
                }
                return false;
            }
            catch (MqException)
            {
                return false;
            }
        }

        public string GetMqSoftwareVersion()
        {
            return _mqFactory.GetSoftwareVersion();
        }

        public bool LocalMqInstalled
        {
            get { return _mqFactory.LocalMqInstalled; }
        }

        public IQueueManager TryOpenQueueManager(RecentQueueManagerConnection rc)
        {
            if (rc == null) return null;
            try
            {
                var qm = this.ConnectQueueManager(rc.QueueManagerName);
                return qm;
            }
            catch (MqException ex)
            {
                ex.Log();
                return null;
            }
        }

        public IQueueManager TryOpenRemoteQueueManager(RecentRemoteQueueManagerConnection rc)
        {
            if (rc == null) return null;
            try
            {
                if (!string.IsNullOrWhiteSpace(rc.UserId)) return null;
                var cp = CreateConnectionProperties(rc.HostName, rc.Port, rc.Channel);
                var qm = this.ConnectQueueManager(rc.QueueManagerName, cp);
                return qm;
            }
            catch (MqException ex)
            {
                ex.Log();
                return null;
            }
        }

        public IQueue TryOpenQueue(RecentQueueConnection rc)
        {
            if (rc == null) return null;
            try
            {
                var qm = this.ConnectQueueManager(rc.QueueManagerName);
                var q = qm.OpenQueue(rc.QueueName);
                return q;
            }
            catch (MqException ex)
            {
                ex.Log();
                return null;
            }
        }

        public IQueue TryOpenRemoteQueue(RecentRemoteQueueConnection rc)
        {
            if (rc == null) return null;
            try
            {
                if (!string.IsNullOrWhiteSpace(rc.UserId)) return null;
                var cp = CreateConnectionProperties(rc.HostName, rc.Port, rc.Channel);
                var qm = this.ConnectQueueManager(rc.QueueManagerName, cp);
                var q = qm.OpenQueue(rc.QueueName);
                return q;
            }
            catch (MqException ex)
            {
                ex.Log();
                return null;
            }
        }
    }
}
