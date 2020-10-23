#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Windows;
using System.Windows.Threading;
using Dotc.MQExplorerPlus.Core;
using Dotc.MQExplorerPlus.Core.Models;
using static System.FormattableString;
using System.Runtime.ExceptionServices;
using System.Security;

namespace Dotc.MQExplorerPlus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : System.Windows.Application, IDisposable
    {

        private Bootstrapper _bootStrapper;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            WeakEventManager<App, DispatcherUnhandledExceptionEventArgs>
                .AddHandler(this, "DispatcherUnhandledException", AppDispatcherUnhandledException);

            WeakEventManager<AppDomain, UnhandledExceptionEventArgs>
                .AddHandler(AppDomain.CurrentDomain, "UnhandledException", AppDomainUnhandledException);


            _bootStrapper = new Bootstrapper();
            _bootStrapper.Run();



        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _bootStrapper.Shutdown();
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleError(e.ExceptionObject as Exception);
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = HandleError(e.Exception);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private bool HandleError(Exception ex)
        {
            try
            {
                if (ex == null) return true;

                ex.Log();

                var comException = ex as System.Runtime.InteropServices.COMException;

                if (comException != null && comException.ErrorCode == -2147221040)  /* CLIPBRD_E_CANT_OPEN */
                    return true;

                if (ex is NotImplementedException)
                {
                    System.Windows.MessageBox.Show(Invariant($"Error: {ex.Message}")
                    , ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }

                System.Windows.MessageBox.Show(Invariant($"Fatal Error: {ex.Message}\n\nThe application will quit.")
                    , ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);

                System.Windows.Application.Current.Shutdown();

                return false;
            }
            catch (Exception)
            {
                Environment.FailFast("An error occured while reporting an error.", ex);
                return false;
            }

        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _bootStrapper.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
