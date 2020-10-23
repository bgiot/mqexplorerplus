#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using Dotc.MQ;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Views;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public class OpenQueueViewModel : ModalViewModel
    {
    
        public OpenQueueViewModel(IOpenQueueView view, IApplicationController appc)
            : base(view, appc)
        {
            Title = "Select Queue...";

            Suggestions = new ObservableCollection<string>();
        }

        public void Initialize(IQueueManager queueManager = null)
        {
            if (queueManager == null)
            {
                QueueManagerProvided = false;
                App.ShellService.WithGlobalBusy(() =>
                    {
                        QueueManagerName = App.MqController.TryGetDefaultQueueManagerName();
                    });
            }
            else
            {
                QueueManagerProvided = true;
                QueueManager = queueManager;
                QueueManagerName = queueManager.ConnectionInfo;
                if (QueueManager.ConnectionProperties == null || QueueManager.ConnectionProperties.IsLocal)
                {
                    UpdateSuggestions();
                }
            }
        }

        private bool _qmProvided;
        public bool QueueManagerProvided
        {
            get { return _qmProvided; }
            set { SetPropertyAndNotify(ref _qmProvided, value); }
        }

        private bool _includeSystemQueues;

        public bool IncludeSystemQueues
        {
            get { return _includeSystemQueues; }
            set
            {
                if (SetPropertyAndNotify(ref _includeSystemQueues, value))
                    UpdateSuggestions();
            }
        }

        public IQueue SelectedQueue { get; private set; }
        public IQueueManager QueueManager { get; private set; }
        public ObservableCollection<string> Suggestions { get; }

        private string _queueName;
        [CustomValidation(typeof(OpenQueueViewModel), "CheckQueueNameIsValid")]
        public string QueueName
        {
            get { return _queueName; }
            set
            {
                if (SetPropertyAndNotify(ref _queueName, value))
                {
                    UpdateSuggestions();
                }
            }
        }

        private string _queueManagerName;
        [CustomValidation(typeof(OpenQueueViewModel), "CheckQueueManagerNameIsValid")]
        public string QueueManagerName
        {
            get { return _queueManagerName; }
            set
            {
                if (SetPropertyAndNotify(ref _queueManagerName, value))
                {
                    if (!QueueManagerProvided)
                    {
                        UpdateSuggestions();
                    }
                }
            }
        }

        public static ValidationResult CheckQueueManagerNameIsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null) throw new ArgumentNullException(nameof(validationContext));

            var qmName = value as string;
            var source = validationContext.ObjectInstance as OpenQueueViewModel;

            if (source == null) return new ValidationResult("Queue manager name required");

            if (source.QueueManagerProvided)
            {
                return ValidationResult.Success;
            }

            if (!string.IsNullOrEmpty(qmName))
            {
                if (!source.App.MqController.CheckLocalQueueManagerNameIsValid(qmName))
                {
                    source.QueueManager = null;
                    return new ValidationResult("Invalid queue manager name");
                }
                source.QueueManager = source.App.MqController.ConnectQueueManager(qmName);
                return ValidationResult.Success;
            }
            source.QueueManager = null;
            return new ValidationResult("Queue manager name required");
        }

        public static ValidationResult CheckQueueNameIsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null) throw new ArgumentNullException(nameof(validationContext));

            var qName = value as string;
            var source = validationContext.ObjectInstance as OpenQueueViewModel;

            if (!string.IsNullOrEmpty(qName))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Queue name required");
        }

        private bool _selectedSuggestionChanged;

        private string _selectedSuggestion;
        public string SelectedSuggestion
        {
            get { return _selectedSuggestion; }
            set
            {
                if (SetPropertyAndNotify(ref _selectedSuggestion, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _selectedSuggestionChanged = true;
                        QueueName = value;
                        _selectedSuggestionChanged = false;
                    }
                }
            }
        }

        private void UpdateSuggestions()
        {
            if (_selectedSuggestionChanged) return;

            Suggestions.Clear();

            if (QueueManagerProvided || QueueManager != null)
            {

                App.ShellService.WithGlobalBusy(() =>
                {
                    try
                    {
                        var filter = QueueManager.NewObjectNameFilter(QueueName);
                        var provider = QueueManager.NewObjectProvider(filter);
                        var sysFilter = QueueManager.NewSystemObjectNameFilter();
                        var list = provider.GetQueueNames();
                        foreach (var s in list)
                        {
                            if (!IncludeSystemQueues && sysFilter.IsMatch(s))
                                continue;

                            Suggestions.Add(s);
                        }
                    }
                    catch(MqException)
                    { }
                });
            }

            CommandManager.InvalidateRequerySuggested();
        }

        protected override bool OkAllowed()
        {
            return !HasErrors;
        }

        protected override void OnCancel(CancelEventArgs e)
        {
            SelectedQueue = null;
        }

        protected override void OnOk(CancelEventArgs e)
        {
            App.ShellService.WithGlobalBusy(() =>
                {
                    try
                    {
                        SelectedQueue = QueueManager.OpenQueue(QueueName);
                    }
                    catch (MqException mqe)
                    {
                        e.Cancel = true;
                        ShowErrorMessage("Error opening queue", mqe);
                    }
                });
        }
    }
}
