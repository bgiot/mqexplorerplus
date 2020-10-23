#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.MQExplorerPlus.Core.Controllers;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{

    public class SettingsViewModel : ModalViewModel
    {

        public SettingsViewModel(ISettingsView view, IApplicationController appc)
            : base(view, appc)
        {
            Title = "Settings";
        }

        private UserSettings Source;

        protected override void OnOk(CancelEventArgs e)
        {
            CopyToSource();
            if (Source.Save())
                base.OnOk(e);
            else
            {
                ShowErrorMessage("Failed to save settings.");
                if (e != null) e.Cancel = true;
            }
        }


        private void CopyToSource()
        {

            if (Settings.AutoRefreshInterval != Source.AutoRefreshInterval)
            {
                Source.AutoRefreshInterval = Settings.AutoRefreshInterval;
            }
            if (Settings.BrowseLimit != Source.BrowseLimit)
            {
                Source.BrowseLimit = Settings.BrowseLimit;
            }
            if (Settings.QueueDepthWarningThreshold != Source.QueueDepthWarningThreshold)
            {
                Source.QueueDepthWarningThreshold = Settings.QueueDepthWarningThreshold;
            }
            if (Settings.PutPriority != Source.PutPriority)
            {
                Source.PutPriority = Settings.PutPriority;
            }
            if (Settings.Port != Source.Port)
            {
                Source.Port = Settings.Port;
            }
            if (Settings.Channel != Source.Channel)
            {
                Source.Channel = Settings.Channel;
            }
            if (Settings.MaxRecentConnections != Source.MaxRecentConnections)
            {
                Source.MaxRecentConnections = Settings.MaxRecentConnections;
            }
        }


        public override string OkLabel => "Save";

        protected override bool OkAllowed()
        {
            return Settings.HasErrors == false;
        }

        public UserSettings Settings { get; private set; }

        public void Initialize(UserSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            Source = settings;
            Settings = settings.Clone();
        }
    }
}
