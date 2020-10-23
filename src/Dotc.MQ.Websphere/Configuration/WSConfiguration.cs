#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using IBM.WMQ;
using System.Configuration;

namespace Dotc.MQ.Websphere.Configuration
{
    public class WSConfiguration : ConfigurationSection
    {

        public const string SectionName = "mq.websphere";
        public static WSConfiguration Current
        {
            get
            {
                return (WSConfiguration)ConfigurationManager.GetSection(WSConfiguration.SectionName);
            }
        }

        [ConfigurationProperty("avoidPCFWhenPossible", DefaultValue = false)]
        public bool AvoidPCFWhenPossible
        {
            get { return (bool)base["avoidPCFWhenPossible"]; }
        }


        [ConfigurationProperty("remoteConnectionTransport", DefaultValue = "managed")]
        public string RemoteConnectionTransport
        {
            get
            {
                var value = (string)base["remoteConnectionTransport"];
                if (string.IsNullOrEmpty(value))
                {
                    return MQC.TRANSPORT_MQSERIES_MANAGED;
                }
                switch (value.ToLowerInvariant())
                {
                    case "client":
                        return MQC.TRANSPORT_MQSERIES_CLIENT;
                    default:
                        return MQC.TRANSPORT_MQSERIES_MANAGED;
                }
            }
        }
    }
}
