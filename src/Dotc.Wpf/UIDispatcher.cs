using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Dotc.Wpf
{
    public static class UIDispatcher
    {

        private static Dispatcher _internalUiDispatcher;
        public static Dispatcher MainDispatcher
        {
            get { return _internalUiDispatcher ?? Application.Current?.Dispatcher; }
            set { _internalUiDispatcher = value; }
        }

        

        public static void Execute(Action action)
        {
            var dispatcher = MainDispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(action);
            }
            else
            {
                action.Invoke();
            }
        }

        public static T Execute<T>(Func<T> action)
        {
            var dispatcher = MainDispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                return dispatcher.Invoke(action);
            }
            else
            {
                return action.Invoke();
            }
        }

        public static async Task ExecuteAsync(Action action)
        {
            var dispatcher = MainDispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                await dispatcher.InvokeAsync(action);
            }
            else
            {
                await Task.Run(action);
            }
        }
    }
}
