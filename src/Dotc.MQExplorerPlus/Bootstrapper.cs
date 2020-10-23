#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Windows;
using Dotc.MQ;
using Dotc.MQ.Websphere;
using Dotc.MQExplorerPlus.Core;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.MQExplorerPlus.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Dotc.MQExplorerPlus
{
    public sealed class Bootstrapper : IDisposable
    {
        public Bootstrapper()
        {
            GC.SuppressFinalize(this);
        }

        public  void Run()
        {

            System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var splash = new SplashScreenManager();
            splash.Show();

            var sc = new ServiceCollection();
            sc.AddMqExplorerPlusCoreServices();
            AddDependencies(sc);
            var sp = sc.BuildServiceProvider();

            MainController = sp.GetRequiredService<IApplicationController>();
            IShellView shell = MainController.Run();

            if (shell != null && shell is ShellWindow)
            {
                System.Windows.Application.Current.MainWindow = (ShellWindow)shell;
                System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                System.Windows.Application.Current.MainWindow.Show();
                splash.Close();
                System.Windows.Application.Current.MainWindow.Activate();
            }
            else
            {
                System.Windows.Application.Current.Shutdown();
            }


        }

        private void AddDependencies(IServiceCollection sc)
        {
            sc.AddSingleton<IQueueManagerFactory, WsQueueManagerFactory>();
            sc.AddSingleton<IViewService, ViewService>();
            sc.AddSingleton<IMessageService, MessageService>();
            sc.AddSingleton<ISettingsProvider, SettingsProvider>();
            sc.AddSingleton<IFileDialogService, FileDialogService>();

            sc.AddSingleton<IMainView, MainView>();
            sc.AddSingleton<IShellView, ShellWindow>();
            sc.AddSingleton<IWelcomeView, WelcomeView>();

            sc.AddTransient<IAboutView, AboutView>();
            sc.AddTransient<IChannelResetParametersView, ChannelResetParametersView>();
            sc.AddTransient<IChannelResolveParametersView, ChannelResolveParametersView>();
            sc.AddTransient<IChannelStopParametersView, ChannelStopParametersView>();
            sc.AddTransient<IDumpCreationSettingsView, DumpCreationSettingsView>();
            sc.AddTransient<IDumpLoadSettingsView, DumpLoadSettingsView>();
            sc.AddTransient<IExportMessagesSettingsView, ExportMessagesSettingsView>();
            sc.AddTransient<IMessageListView, MessageListView>();
            sc.AddTransient<IOpenQueueManagerView, OpenQueueManagerView>();
            sc.AddTransient<IOpenQueueView, OpenQueueView>();
            sc.AddTransient<IParsingEditorView, ParsingEditorView>();
            sc.AddTransient<IPutMessageView, PutMessageView>();
            sc.AddTransient<IQueueManagerView, QueueManagerView>();
            sc.AddTransient<ISettingsView, SettingsView>();

        }

        public void Shutdown()
        {
            MainController.Shutdown();
        }

        public IApplicationController MainController
        { get; private set; }


        public void Dispose()
        {
        }
    }
}
