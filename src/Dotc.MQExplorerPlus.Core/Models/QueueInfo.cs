#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Dotc.MQ;
using System.Threading;
using Dotc.Wpf.Controls.HexViewer;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public class QueueInfo : SelectableItem //, IQueueInfo
    {
        public IQueue QueueSource { get; }

        public QueueInfo(IQueue queueSource, UserSettings settings)
        {
            QueueSource = queueSource;

            PropertyChangedEventManager.AddHandler(QueueSource, QueueInfo_PropertyChanged, string.Empty);

            if (settings != null)
            {
                _thresholdParamter = settings.QueueDepthWarningThreshold;

                WeakEventManager<UserSettings, EventArgs>
                .AddHandler(settings, "OnSettingsChanged",
                    (s, e) =>
                    {
                        ThresholdParameter = ((UserSettings)s).QueueDepthWarningThreshold;
                    });
            }
        }

        private void QueueInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == nameof(Depth))
                OnPropertyChanged(nameof(IsDepthOverThreshold));
        }

        private string _thresholdParamter;
        public string ThresholdParameter
        {
            get { return _thresholdParamter; }
            set
            {
                _thresholdParamter = value;
                OnPropertyChanged(nameof(IsDepthOverThreshold));
            }
        }
        public bool IsDepthOverThreshold
        {
            get
            {
                if (Depth.HasValue)
                    return QueueDepthValidator.IsDepthOverThreshold(Depth.Value, ExtendedProperties.MaxDepth, _thresholdParamter);
                else
                    return false;
            }
        }
        public dynamic ExtendedProperties => QueueSource.ExtendedProperties;

        public int? Depth => QueueSource.Depth;

        public GetPutStatus? GetStatus => QueueSource.GetStatus;

        public string Name => QueueSource.Name;

        public GetPutStatus? PutStatus => QueueSource.PutStatus;

        public int Type => QueueSource.Type;

        public bool SupportTruncate => QueueSource.SupportTruncate;

        public void RefreshInfo()
        {
            QueueSource.RefreshInfo();
        }

        public void SetGetStatus(GetPutStatus newStatus)
        {
            QueueSource.SetGetStatus(newStatus);
        }

        public void SetPutStatus(GetPutStatus newStatus)
        {
            QueueSource.SetPutStatus(newStatus);
        }

        public void EmptyQueue(bool truncate)
        {
            QueueSource.ClearQueue(truncate);
        }


        public string UniqueId => QueueSource.UniqueId;


        public IQueueManager QueueManager => QueueSource.QueueManager;

        public IDump DumpEngine => QueueSource.DumpEngine;

        public IEnumerable<MessageInfo> Browse(int numberOfMessages, BrowseFilter filter, CancellationToken ct, IProgress<int> progress, MessageInfo lastMessage, IByteCharConverter converter)
        {

            int indexOffset = 0;
            if (lastMessage != null && lastMessage.Index.HasValue)
            {
                indexOffset = lastMessage.Index.Value;
            }

            foreach (var msg in QueueSource.BrowseMessages(numberOfMessages,ct, lastMessage?.MessageId, filter, progress))
            {
                var mi = new MessageInfo(msg, converter, indexOffset);
                yield return mi;
            }
        }

        public void DeleteMessages(IEnumerable<MessageInfo> list, CancellationToken ct, IProgress<int> progress)
        {
            QueueSource.DeleteMessages(list.Select(m => m.MessageSource).ToList(),ct, progress);
        }

        public void ForwardMessages(IEnumerable<MessageInfo> list, IQueue destinationQ, CancellationToken ct, IProgress<int> progress)
        {
            QueueSource.ForwardMessages(list.Select(m => m.MessageSource).ToList(), destinationQ,ct, progress);
        }

        internal void Empty(bool withTruncate)
        {
            QueueSource.ClearQueue(withTruncate);
        }

        public void PutMessage(string content, int priority, int characterSet, string correlationId, string groupId, int? lsn)
        {
            var ep = BuildExtendedProperties(correlationId, groupId, lsn);
            var msg = QueueSource.NewMessage(content, priority, characterSet, ep);
            QueueSource.PutMessages(new[] { msg }, CancellationToken.None);
        }

        public void PutMessages(IEnumerable<string> contents, int priority, int characterSet, string correlationId, string groupId, int? lsn, CancellationToken ct, IProgress<int> progress)
        {

            var msgs = contents.Select(s =>
            {
                var ep = BuildExtendedProperties(correlationId, groupId, lsn);
                if (lsn.HasValue) lsn++;
                return QueueSource.NewMessage(s, priority, characterSet, ep);
            }).ToList();
            QueueSource.PutMessages(msgs, ct,progress);
        }

        private Dictionary<string, object> BuildExtendedProperties(string correlationId, string groupId, int? lsn)
        {
            var obj = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(correlationId))
            {
                obj["CorrelationId"] = correlationId.HexStringToBytes();
            }
            if (!string.IsNullOrEmpty(groupId))
            {
                obj["GroupId"] = groupId.HexStringToBytes();
                if (lsn.HasValue)
                {
                    obj["LogicalSequenceNumber"] = lsn.Value;
                }
            }
            return obj;
        }
    }
}
