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
using Dotc.MQ;
using System.ComponentModel;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public class ChannelInfo : SelectableItem
    {
        public IChannel ChannelSource { get; }

        public ChannelInfo(IChannel channelSource, UserSettings settings)
        {
            ChannelSource = channelSource;
            PropertyChangedEventManager.AddHandler(ChannelSource, ChannelInfo_PropertyChanged, string.Empty);
        }
        private void ChannelInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public string Name => ChannelSource.Name;

        public int Type => ChannelSource.Type;

        public string TransmissionQueue => ChannelSource.TransmissionQueue;

        public string ConnectionName => ChannelSource.ConnectionName;

        public ChannelStatus? Status => ChannelSource.Status;

        public bool? IndoubtStatus => ChannelSource.IndoubtStatus;

        public bool SupportReset
        {
            get
            {
                return ChannelSource.SupportReset;
            }
        }

        public bool SupportResolve
        {
            get
            {
                return ChannelSource.SupportResolve;
            }
        }

        public void RefreshInfo()
        {
            ChannelSource.RefreshInfo();
            OnPropertyChanged(nameof(SupportResolve));
            OnPropertyChanged(nameof(SupportReset));
        }

        public void Start()
        {
            ChannelSource.Start();
        }

        public void Stop(ChannelStopParameters settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            ChannelSource.Stop(settings.Mode, settings.SetInactive);
        }

        public void Reset(ChannelResetParameters settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            ChannelSource.Reset(settings.MessageSequenceNumber.Value);
        }

        public void Resolve(ChannelResolveParameters settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            ChannelSource.Resolve(settings.Commit);
        }
    }
}
