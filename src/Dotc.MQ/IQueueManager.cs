#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;

namespace Dotc.MQ
{


    public interface IConnectionProperties
    {
        string HostName { get; }
        int Port { get; }
        string Channel { get; }
        string UserId { get; }
        string Password { get; }
        bool IsLocal { get; }
        void Set(string hostName, int port, string channel, string userId = null, SecureString password = null);
        void Reset();
    }

    public interface IQueueManager
    {
        string Name { get; }
        string ConnectionInfo { get; }
        string UniqueId { get; }
        IConnectionProperties ConnectionProperties { get; }
        int DefaultCharacterSet { get; }
        IQueue OpenQueue(string queueName, bool autoLoadInfo = false);
        IChannel OpenChannel(string channelName, bool autoLoadInfo = false);
        void Disconnect();
        void Commit();
        void Rollback();
        IObjectNameFilter NewObjectNameFilter(string namePrefix = null);
        IObjectNameFilter NewSystemObjectNameFilter();

        IObjectProvider NewObjectProvider(IObjectNameFilter objFilter);
    }

    public interface IObjectNameFilter
    {        
        string[] NamePrefix { get; }   
        bool IsMatch(string name);

        string UniqueId { get; }
    }

    public class StaticQueueList : IObjectNameFilter
    {

        public StaticQueueList(IEnumerable<string> names)
        {
            Names = new List<string>(names).ToArray(); 
        }

        public string[] Names { get; }
        public string[] NamePrefix => new string[] { "#" };

        private int ComputeHash()
        {
            return string.Concat(Names.OrderBy(x => x)).GetHashCode();
        }

        public string UniqueId => "#" + ComputeHash().ToString(CultureInfo.InvariantCulture);

        public bool IsMatch(string name)
        {
            return true;
        }
    }
}
