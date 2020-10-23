#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;

using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.ViewModels;
using Dotc.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace Dotc.MQExplorerPlus.Core
{
    public static class ServiceCollectionExtensions
    {

        public static ServiceCollection AddMqExplorerPlusCoreServices(this ServiceCollection sc)
        {
            sc.AddSingleton<MqController>();
            sc.AddSingleton<IApplicationController, ApplicationController>();
            sc.AddSingleton<ShellService>();
            sc.AddSingleton<UserSettings>();
            sc.AddSingleton<AppSettings>();

            sc.AddSingleton<ShellViewModel>();
            sc.AddSingleton<WelcomeViewModel>();
            sc.AddSingleton<MainViewModel>();
            
            sc.AddTransient<AboutViewModel>();
            sc.AddTransient<ChannelResetParametersViewModel>();
            sc.AddTransient<ChannelResolveParametersViewModel>();
            sc.AddTransient<ChannelStopParametersViewModel>();
            sc.AddTransient<DumpCreationSettingsViewModel>();
            sc.AddTransient<DumpLoadSettingsViewModel>();
            sc.AddTransient<ExportMessagesSettingsViewModel>();
            sc.AddTransient<MessageListViewModel>();
            sc.AddTransient<OpenQueueManagerViewModel>();
            sc.AddTransient<OpenQueueViewModel>();
            sc.AddTransient<ParsingEditorViewModel>();
            sc.AddTransient<PutMessageViewModel>();
            sc.AddTransient<QueueManagerViewModel>();
            sc.AddTransient<SettingsViewModel>();

            return sc;



        }


    }
}
