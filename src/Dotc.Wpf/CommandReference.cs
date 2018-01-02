using System;
using System.Windows;
using System.Windows.Input;

namespace Dotc.Wpf
{
    public class CommandReference : Freezable, ICommand
    {
        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register("Command", typeof(ICommand), 
            typeof(CommandReference), new PropertyMetadata(OnCommandChanged));
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        #region ICommand Members
        public bool CanExecute(object parameter)
        {
            return Command != null && Command.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            Command.Execute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commandReference = d as CommandReference;
            var oldCommand = e.OldValue as ICommand;
            var newCommand = e.NewValue as ICommand;
            if (oldCommand != null && commandReference != null)
            {
                oldCommand.CanExecuteChanged -= commandReference.CanExecuteChanged;
            }
            if (newCommand != null && commandReference != null)
            {
                newCommand.CanExecuteChanged += commandReference.CanExecuteChanged;
            }
        }
        #endregion
        #region Freezable
        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
