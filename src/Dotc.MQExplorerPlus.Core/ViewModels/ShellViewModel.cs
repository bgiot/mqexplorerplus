#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel.Composition;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.MQExplorerPlus.Core.Controllers;
using System.Windows;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    [Export(typeof(ShellViewModel)), PartCreationPolicy(CreationPolicy.Shared)]

    public sealed class ShellViewModel : ViewModel
    {

        private ViewModel _content;
        private readonly UserSettings _settings;
        private readonly WelcomeViewModel _welcomeContent;
        private readonly MainViewModel _mainContent;

        [ImportingConstructor]
        public ShellViewModel(IShellView window, IApplicationController appController)
            : base (window, appController)
        {

            Title = ApplicationInfo.ProductName;

            _settings = App.UserSettings;

            WeakEventManager<UserSettings, EventArgs>
                .AddHandler(_settings, "OnSettingsChanged", _settings_OnSettingsChanged);


            WeakEventManager<IViewService, EventArgs>
                .AddHandler(App.ViewService, "ModalOpening", Modal_Opening);
            WeakEventManager<IViewService, EventArgs>
                .AddHandler(App.ViewService, "ModalClosing", Modal_Closing);

            WeakEventManager<IViewService, CountEventArgs>
                .AddHandler(App.ViewService, "DocumentsCountChanged", Documents_CountChanged);

            _welcomeContent = CompositionHost.GetInstance<WelcomeViewModel>();
            _mainContent = CompositionHost.GetInstance<MainViewModel>();

            Content = _welcomeContent;

        }

        private void Documents_CountChanged(object sender, CountEventArgs e)
        {
            if (e.Count != 0)
            {
                Content = _mainContent;
            }
            else
            {
                Content = _welcomeContent;
            }
        }

        public ShellService ShellService {  get { return App.ShellService; } }
        public IViewService ViewService {  get { return App.ViewService; } }
        private void _settings_OnSettingsChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(AutoRefreshInterval));
        }

        void Modal_Closing(object sender, EventArgs e)
        {
            ShellService.EnableAutomaticRefresh();
        }

        void Modal_Opening(object sender, EventArgs e)
        {
            ShellService.DisableAutomaticRefresh();
        }

        public ViewModel Content
        {
            get { return _content; }
            set { SetPropertyAndNotify(ref _content, value); }
        }

        public int AutoRefreshInterval => _settings.AutoRefreshInterval;

    }
}
