using System;
using System.Windows.Input;

namespace Dotc.Mvvm
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public static void Execute(ICommand cmd, object parameter = null)
        {
            cmd?.Execute(parameter);
        }

        public RelayCommand(Action<T> executeMethod)
            : this(executeMethod, (x) => true)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _canExecute = canExecute;
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


        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    public sealed class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action executeMethod)
        : base( (o) => executeMethod(), (o) => true)
        {
        }

        public RelayCommand(Action executeMethod, Func<bool> canExecuteMethod)
        : base(o => executeMethod(), o => canExecuteMethod())
        {
        }
    }
}
