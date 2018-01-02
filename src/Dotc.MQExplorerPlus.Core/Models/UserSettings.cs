#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.Mvvm;

namespace Dotc.MQExplorerPlus.Core.Models
{

    public interface IUserSettings
    {
        int AutoRefreshInterval { get; }
        int BrowseLimit { get; }
        int PutPriority { get; }
        int Port { get; }
        string Channel { get; }
        string QueueDepthWarningThreshold { get; }
        int MaxRecentConnections { get; }
        ObservableCollection<RecentConnection> RecentConnections { get; }

    }


    [Export(typeof(UserSettings)), PartCreationPolicy(CreationPolicy.Shared)]
    [DisplayName("Default Settings")]
    public class UserSettings : ValidatableBindableBase, IUserSettings
    {
        private int _autoRefreshInterval;
        private int _browseLimit;
        private string _queueDepthWarningThreshold;
        private int _putPriority;
        private int _port;
        private string _channel;
        private int _maxRecentConnections;
        private ObservableCollection<RecentConnection> _recentConnections;

        public const int DefaultBrowseLimit = 500;
        public const bool DefaultBrowseMultiThread = false;
        public const int DefaultPort = 1414;
        public const string DefaultChannel = "SYSTEM.ADMIN.SVRCONN";
        public const int DefaultPutPriority = 5;
        public const int DefaultAutoRefreshInterval = 15;
        public const string DefaultQueueDepthWarningThreshold = "80%";
        public const int DefaultMaxRecentConnections = 10;


        [ImportingConstructor]
        public UserSettings(ISettingsProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            Provider = provider;
            Initialize();
        }

        private UserSettings(UserSettings source)
        {
            _browseLimit = source.BrowseLimit;
            _autoRefreshInterval = source.AutoRefreshInterval;
            _putPriority = source.PutPriority;
            _port = source.Port;
            _channel = source.Channel;
            _queueDepthWarningThreshold = source.QueueDepthWarningThreshold;
            _maxRecentConnections = source.MaxRecentConnections;

        }

        private void Initialize()
        {

            var configSource = Provider?.ReadUserSettings();
            if (configSource != null)
            {
                _browseLimit = configSource.BrowseLimit;
                _autoRefreshInterval = configSource.AutoRefreshInterval;
                _putPriority = configSource.PutPriority;
                _port = configSource.Port;
                _channel = configSource.Channel;
                _queueDepthWarningThreshold = configSource.QueueDepthWarningThreshold;
                _maxRecentConnections = configSource.MaxRecentConnections;
                _recentConnections = new ObservableCollection<RecentConnection>(configSource.RecentConnections);
            }
            else
            {
                _browseLimit = DefaultBrowseLimit;
                _port = DefaultPort;
                _channel = DefaultChannel;
                _autoRefreshInterval = DefaultAutoRefreshInterval;
                _putPriority = DefaultPutPriority;
                _queueDepthWarningThreshold = DefaultQueueDepthWarningThreshold;
                _maxRecentConnections = DefaultMaxRecentConnections;
                _recentConnections = new ObservableCollection<RecentConnection>();
            }
        }

        private ISettingsProvider Provider { get; set; }

        public event EventHandler OnSettingsChanged;

        [Required]
        [Range(0,int.MaxValue)]
        [DefaultValue(DefaultAutoRefreshInterval)]
        public int AutoRefreshInterval
        {
            get { return _autoRefreshInterval; }
            set {SetPropertyAndNotify(ref _autoRefreshInterval , value); }
        }

        [Required]
        [Range(0, int.MaxValue)]
        [DefaultValue(DefaultBrowseLimit)]
        public int BrowseLimit
        {
            get { return _browseLimit; }
            set { SetPropertyAndNotify(ref  _browseLimit , value); }
        }


        [Required]
        [QueueDepthWarningThresholdValidation]
        [Description("Can be expressed in absolute value or percentage")]
        [DefaultValue(DefaultQueueDepthWarningThreshold)]
        public string QueueDepthWarningThreshold
        {
            get { return _queueDepthWarningThreshold; }
            set { SetPropertyAndNotify(ref _queueDepthWarningThreshold , value); }
        }

        [Required]
        [Range(0, 9)]
        [DefaultValue(DefaultPutPriority)]
        public int PutPriority
        {
            get { return _putPriority; }
            set { SetPropertyAndNotify(ref _putPriority , value); }
        }

        [Required]
        [Range(0, int.MaxValue)]
        [DefaultValue(DefaultPort)]
        public int Port
        {
            get { return _port; }
            set {SetPropertyAndNotify(ref _port , value); }
        }

        [Required]
        [DefaultValue(DefaultChannel)]
        public string Channel
        {
            get { return _channel; }
            set { SetPropertyAndNotify(ref _channel, value); }
        }


        [Required]
        [Range(1,20)]
        [DefaultValue(DefaultMaxRecentConnections)]
        [Description("Number of items shown in the recent connections list")]
        public int MaxRecentConnections
        {
            get { return _maxRecentConnections; }
            set { SetPropertyAndNotify(ref _maxRecentConnections, value); }
        }

        public bool Save()
        {
            while (_recentConnections.Count > MaxRecentConnections)
            {
                _recentConnections.RemoveAt(MaxRecentConnections);
            }

            try
            {
                Provider?.WriteUserSettings(this);
                OnSettingsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        public UserSettings Clone()
        {
            return new UserSettings(this);
        }

        public ObservableCollection<RecentConnection> RecentConnections
        {
            get { return _recentConnections; }
        }

        public void AddRecentConnection(RecentConnection rc)
        {
            var existing = _recentConnections.FirstOrDefault(x => x.UniqueId == rc.UniqueId || x.IsEqualTo(rc));

            if (existing != null)
            {
                _recentConnections.Remove(existing);
            }
            _recentConnections.Insert(0, rc);
            int index = 0;
            foreach (var item in _recentConnections)
                item.Index = ++index;
            Save();
        }

        public void ClearRecentConnections()
        {
            _recentConnections.Clear();
            Save();
        }

    }
}
