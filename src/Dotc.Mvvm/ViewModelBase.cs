using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Dotc.Mvvm
{

    public abstract class ViewModelBase : ValidatableBindableBase
    {

        protected virtual RelayAsyncCommand CreateAsyncCommand(Action<CancellationToken> execute, Action<RunWorkerCompletedEventArgs> postAction = null)
        {
            var asyncCmd = new RelayAsyncCommand(execute, ()=> !IsCmdExecuting, postAction);
            return AddEventsToAsyncCommand(asyncCmd);
        }
        protected virtual RelayAsyncCommand CreateAsyncCommand (Action<CancellationToken> execute, Func<bool> canExecute, Action<RunWorkerCompletedEventArgs> postAction = null)
        {
            var asyncCmd = new RelayAsyncCommand(execute, () => !IsCmdExecuting && canExecute(), postAction);
            return AddEventsToAsyncCommand(asyncCmd);
        }
        private RelayAsyncCommand AddEventsToAsyncCommand( RelayAsyncCommand cmd )
        {
            WeakEventManager<RelayAsyncCommand, EventArgs>
                .AddHandler(cmd, "Started", Cmd_Started);
            WeakEventManager<RelayAsyncCommand, EventArgs>
                .AddHandler(cmd, "Ended", Cmd_Ended);
            return cmd;
        }

        private void Cmd_Started(object sender, EventArgs args)
        {
            IsCmdExecuting = true;
        }

        private void Cmd_Ended(object sender, EventArgs args)
        {
            IsCmdExecuting = false;
        }

        protected RelayCommand CreateCommand (Action execute, Func<bool> canExecute = null)
        {
            if (canExecute == null)
                return new RelayCommand(execute);
            else
                return new RelayCommand(execute, canExecute);
        }

        private bool _isCmdExecuting;
        public bool IsCmdExecuting
        {
            get { return _isCmdExecuting; }
            set
            {
                if (SetPropertyAndNotify(ref _isCmdExecuting, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

    }
}
