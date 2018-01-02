using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dotc.Mvvm
{

    public sealed class CancelAsyncCommand : ICommand, IDisposable
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _commandExecuting;

        public CancellationToken Token => _cts.Token;

        public void NotifyCommandStarting()
        {
            _commandExecuting = true;
            if (!_cts.IsCancellationRequested)
                return;
            _cts = new CancellationTokenSource();
            RaiseCanExecuteChanged();
        }

        public void NotifyCommandFinished()
        {
            _commandExecuting = false;
            RaiseCanExecuteChanged();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return _commandExecuting && !_cts.IsCancellationRequested;
        }

        void ICommand.Execute(object parameter)
        {
            _cts.Cancel();
            RaiseCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
    public sealed class RelayAsyncCommand : ICommand, IDisposable
    {
        public event EventHandler Started;
        public event EventHandler Ended;

        private readonly Action<RunWorkerCompletedEventArgs> _postAction;
        private readonly CancelAsyncCommand _cancelCommand;
        private readonly Action<CancellationToken> _execute;
        private readonly Func<bool> _canExecute;

        public bool IsExecuting { get; private set; }

        public RelayAsyncCommand(Action<CancellationToken> execute, Action<RunWorkerCompletedEventArgs> postAction = null)
            : this(execute, null, postAction)
        {
        }
        public RelayAsyncCommand(Action<CancellationToken> execute, Func<bool> canExecute, Action<RunWorkerCompletedEventArgs> postAction = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _canExecute = canExecute;
            _postAction = postAction;
            _cancelCommand = new CancelAsyncCommand();
        }

        public ICommand CancelCommand => _cancelCommand;

        public bool CanExecute(object parameter)
        {
            return ((_canExecute == null || _canExecute()) && (!IsExecuting));
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        public async void Execute(object parameter)
        {

            IsExecuting = true;
            Started?.Invoke(this, EventArgs.Empty);

            await Task.Run(() =>
                {
                    _execute(_cancelCommand.Token);
                }, _cancelCommand.Token)
            .ContinueWith(t =>
            {

                var error = t.Exception?.InnerException;

                if (_postAction != null)
                {
                    _postAction.Invoke(new RunWorkerCompletedEventArgs(null, error, t.IsCanceled));
                    error = null;
                }
                IsExecuting = false;
                Ended?.Invoke(this, EventArgs.Empty);

                if (_postAction == null && error != null)
                {
                    throw error;
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cancelCommand.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
