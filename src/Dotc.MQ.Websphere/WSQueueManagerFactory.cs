#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel.Composition;
using IBM.WMQ;

namespace Dotc.MQ.Websphere
{
    [Export(typeof(IQueueManagerFactory)), PartCreationPolicy(CreationPolicy.Shared)]
    public class WsQueueManagerFactory : IQueueManagerFactory
    {


        public IQueueManager Connect()
        {
            try
            {
                return ConnectCore();
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException();
            }
        }

        public IQueueManager Connect(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                return ConnectCore(name);
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException((data) => data.AddOrReplace("Name",name));
            }
        }   


        public IQueueManager Connect(string name, IConnectionProperties properties)
        {
            try
            {

                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                if (properties == null) throw new ArgumentNullException(nameof(properties));

                return ConnectCore(name, (WsConnectionProperties)properties);
            }
            catch (MQException ibmEx)
            {
                throw ibmEx.ToMqException((data) => data.AddOrReplace("Name", name));
            }
        }

        private IQueueManager ConnectCore(string name = null, WsConnectionProperties properties = null)
        {

            if (name == null)
            {
                var ibmLocalQM = new MQQueueManager();
                return new WsQueueManager(ibmLocalQM, WsConnectionProperties.Default);
            }

            if (properties == null || properties.IsLocal)
            {
                var ibmLocalQM = new MQQueueManager(name);
                return new WsQueueManager(ibmLocalQM, WsConnectionProperties.Default);
            }

            var coreProperties = properties.CoreData;
            var ibmQueueManager = new MQQueueManager(name, coreProperties);
            return new WsQueueManager(ibmQueueManager, properties);

        }


        public IConnectionProperties NewConnectionProperties()
        {
            return WsConnectionProperties.Default;
        }

        public string GetSoftwareVersion()
        {
            return WsSoftwareInfo.ApiVersion;
        }

        public bool LocalMqInstalled
        {

            get
            {
                return WsSoftwareInfo.LocalMqIsInstalled;
            }
        }

        internal static WsQueueManager Clone(WsQueueManager from)
        {
            var props = (WsConnectionProperties)from.ConnectionProperties;
            if (props.IsLocal)
            {
                var newQm = new MQQueueManager(from.Name);
                return new WsQueueManager(newQm, props);
            }
            else
            {
                var newQm = new MQQueueManager(from.Name, props.CoreData);
                return new WsQueueManager(newQm, props);
            }

        }

    }
}
