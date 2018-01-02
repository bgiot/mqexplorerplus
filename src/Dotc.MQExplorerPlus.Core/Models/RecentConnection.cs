#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using Dotc.Mvvm;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;

namespace Dotc.MQExplorerPlus.Core.Models
{

    public interface IRecentConnection
    {
        string QueueManagerName { get; set; }
    }

    public interface IRecentQueueConnection
    {
        string QueueName { get; set; }
    }
    public abstract class RecentConnection : BindableBase, IRecentConnection
    {
        private int _index;
        protected RecentConnection()
        {
            UniqueId = Guid.NewGuid();
        }

        public Guid UniqueId { get; }

        public string QueueManagerName { get; set; }

        public virtual bool IsEqualTo(RecentConnection rc)
        {
            if (rc == null) return false;
            if (rc.GetType() != this.GetType()) return false;
            return rc.QueueManagerName == QueueManagerName;
        }


        public int Index
        {
            get
            {
                return _index;
            }
            set { SetPropertyAndNotify(ref _index, value); }
        }

    }

    public class RecentQueueManagerConnection : RecentConnection
    {
        public string ObjectNamePrefix { get; set; }

        public List<string> QueueList { get; set; }

        public string ObjectNamePrefixLabel
        {
            get
            {
                if (QueueList != null && QueueList.Count > 0)
                {
                    if (QueueList.Count > 2)
                    {
                        var temp = string.Join(",", QueueList.Take(3));
                        return Invariant($"{{{temp},...}}");
                    }
                    else
                    {
                        var temp = string.Join(",", QueueList);
                        return Invariant($"{{{temp}}}");
                    }
                }
                else
                {
                    return string.IsNullOrEmpty(ObjectNamePrefix) ? "*" : string.Concat(ObjectNamePrefix, "*");
                }
            }
        }

        private bool QueueListEquals(List<string> l1, List<string> l2)
        {
            if (l1 == null && l2 == null) return true;

            if (l1 == null || l2 == null || l1.Count != l2.Count)
                return false;

            if (l1.Except(l2, StringComparer.OrdinalIgnoreCase).FirstOrDefault() != null)
                return false;

            return true;

        }

        public override bool IsEqualTo(RecentConnection rc)
        {
            if (rc is RecentQueueManagerConnection && base.IsEqualTo(rc))
            {
                var rqmc = (RecentQueueManagerConnection)rc;

                return ((string.IsNullOrEmpty(ObjectNamePrefix) && string.IsNullOrEmpty(rqmc.ObjectNamePrefix))
                    || ObjectNamePrefix == rqmc.ObjectNamePrefix) && QueueListEquals(QueueList, rqmc.QueueList);
            }
            else
                return false;
        }
    }

    public class RecentQueueConnection : RecentConnection, IRecentQueueConnection
    {
        public string QueueName { get; set; }

        public override bool IsEqualTo(RecentConnection rc)
        {
            if (rc is RecentQueueConnection && base.IsEqualTo(rc))
            {
                return QueueName == ((RecentQueueConnection)rc).QueueName;
            }
            else
                return false;
        }
    }


    public class RecentRemoteQueueManagerConnection : RecentQueueManagerConnection
    {

        public string HostName { get; set; }
        public string Channel { get; set; }
        public int Port { get; set; }
        public string UserId { get; set; }

        public override bool IsEqualTo(RecentConnection rc)
        {
            if (rc is RecentRemoteQueueManagerConnection && base.IsEqualTo(rc))
            {
                var temp = (RecentRemoteQueueManagerConnection)rc;
                return (temp.HostName == HostName &&
                        temp.Channel == Channel &&
                        temp.Port == Port &&
                        (temp.UserId ?? "") == (UserId ?? ""));
            }
            else
                return false;
        }
    }

    public class RecentRemoteQueueConnection : RecentRemoteQueueManagerConnection, IRecentQueueConnection
    {
        public string QueueName { get; set; }

        public override bool IsEqualTo(RecentConnection rc)
        {
            if (rc is RecentRemoteQueueConnection && base.IsEqualTo(rc))
            {
                return QueueName == ((RecentRemoteQueueConnection)rc).QueueName;
            }
            else
                return false;
        }
    }
}
