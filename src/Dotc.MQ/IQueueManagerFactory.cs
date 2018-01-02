#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
namespace Dotc.MQ
{
    public interface IQueueManagerFactory
    {
        IQueueManager Connect();
        IQueueManager Connect(string name);
        IQueueManager Connect(string name, IConnectionProperties properties);
        IConnectionProperties NewConnectionProperties();
        string GetSoftwareVersion();
        bool LocalMqInstalled { get; }
    }
}
