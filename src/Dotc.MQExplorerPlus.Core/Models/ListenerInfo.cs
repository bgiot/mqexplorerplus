#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQ;
using System.ComponentModel;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public class ListenerInfo : SelectableItem
    {
        public IListener ListenerSource { get; }

        public ListenerInfo(IListener source, UserSettings settings)
        {
            ListenerSource = source;
            PropertyChangedEventManager.AddHandler(ListenerSource, ListenerInfo_PropertyChanged, string.Empty);
        }
        private void ListenerInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public string Name => ListenerSource.Name;


        public int? Port => ListenerSource.Port;

        public string Protocol => ListenerSource.Protocol;

        public string Ip => ListenerSource.Ip;


        public ListenerStatus? Status => ListenerSource.Status;


        public void RefreshInfo()
        {
            ListenerSource.RefreshInfo();
        }

        public void Start()
        {
            ListenerSource.Start();
        }

        public void Stop()
        {
            ListenerSource.Stop();
        }

    }
}
