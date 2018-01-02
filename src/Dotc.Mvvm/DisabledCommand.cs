using System;
using System.Windows.Input;

namespace Dotc.Mvvm
{
    public class DisabledCommand : ICommand
    {

        public bool CanExecute(object parameter)
        {
            return false;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
        }
    }
}
