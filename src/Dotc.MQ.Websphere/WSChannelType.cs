#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQ.Websphere
{
    public static class WsChannelType
    {
        public const int Unknown = 0;
        public const int Amqp = 1;
        public const int ClientConnection = 2;
        public const int ServerConnection = 3;
        public const int Sender = 4;
        public const int Receiver = 5;
        public const int Server = 6;
        public const int Requester = 7;
        public const int ClusterSender = 8;
        public const int ClusterReceiver = 9;

        public static string GetName(int type)
        {
            switch (type)
            {
                case 1: return "AMQP";
                case 2: return "Client-connection";
                case 3: return "Server-connection";
                case 4: return "Sender";
                case 5: return "Receiver";
                case 6: return "Server";
                case 7: return "Requester";
                case 8: return "Cluster-sender";
                case 9: return "Cluster-receiver";
                default: return "Unknown";
            }
        }
    }
}
