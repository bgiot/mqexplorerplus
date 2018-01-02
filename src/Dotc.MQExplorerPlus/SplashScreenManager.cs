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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Dotc.MQExplorerPlus.Views;

namespace Dotc.MQExplorerPlus
{
    public class SplashScreenManager
    {
        private AutoResetEvent WaitForCreation { get; set; }
        private EventHandler SplashCloseEvent { get; set; }
        public void Show()
        {
            WaitForCreation = new AutoResetEvent(false);

            ThreadStart showSplash =
                () =>
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                    (Action)(() =>
                    {

                        var splash = new SplashScreenWindow();

                        SplashCloseEvent += (s, e) => splash.Dispatcher.InvokeShutdown();
                        splash.Show();
                        WaitForCreation.Set();

                    }));

                    Dispatcher.Run();
                };

            var thread = new Thread(showSplash) { Name = "Splash Thread", IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            WaitForCreation.WaitOne();
        }

        public void Close()
        {
            SplashCloseEvent?.Invoke(this, new EventArgs());
        }
    }
}
