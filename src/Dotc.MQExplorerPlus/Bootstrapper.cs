#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dotc.MQ.Websphere;
using Dotc.MQExplorerPlus.Core;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.MQExplorerPlus.Views;
using Dotc.Wpf;


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

            InitMef();

            MainController = CompositionHost.GetInstance<IApplicationController>();
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


        private  void InitMef()
        {

                var catalog = new AggregateCatalog();

                catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(IApplicationController).Assembly));
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(WsQueueManagerFactory).Assembly));

                var container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection);
                CompositionHost.Initialize(container);


        }


        public void Shutdown()
        {
            MainController.Shutdown();
        }

        public IApplicationController MainController
        { get; private set; }


        public void Dispose()
        {
            CompositionHost.Release();
        }
    }
}
