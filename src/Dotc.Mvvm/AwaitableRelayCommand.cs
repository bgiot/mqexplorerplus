using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dotc.Mvvm
{
    public class AwaitableRelayCommand : AwaitableRelayCommand<object>, IAsyncCommand
    {
        public AwaitableRelayCommand(Func<Task> executeMethod)
            : base(o => executeMethod())
        {
        }

        public AwaitableRelayCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base(o => executeMethod(), o => canExecuteMethod())
        {
        }
    }

    public class AwaitableRelayCommand<T> : IAsyncCommand<T>, ICommand
    {
        private readonly Func<T, Task> _executeMethod;
        private readonly Func<T, bool> _canExecuteMethod;
        private bool _isExecuting;

        public AwaitableRelayCommand(Func<T, Task> executeMethod)
            : this(executeMethod, _ => true)
        {
        }

        public AwaitableRelayCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        {
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        public async Task ExecuteAsync(T obj)
        {
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _executeMethod(obj);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public ICommand Command { get { return this; } }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && _canExecuteMethod((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
