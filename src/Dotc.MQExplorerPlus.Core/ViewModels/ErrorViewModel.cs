#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using Dotc.Common;
using Dotc.Mvvm;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public class ErrorViewModel : ObservableObject
    {

        public ErrorViewModel()
        {
            ErrorMessages = new ObservableCollection<string>();
            BindingOperations.EnableCollectionSynchronization(ErrorMessages, new object());
            CloseCommand = new RelayCommand(Close, () => true);
            _hasErrors = false;
        }

        public ICommand CloseCommand { get; private set; }

        private void Close()
        {
            HasErrors = false;
            ErrorMessages.Clear();
        }

        private bool _hasErrors;
        public bool HasErrors
        {
            get{return _hasErrors;}
            protected set { SetPropertyAndNotify(ref _hasErrors, value); }
        }

        public ObservableCollection<string> ErrorMessages { get; private set; }

        public void AddErrorMessage(string message)
        {
            ErrorMessages.Add(message);
            HasErrors = true;
        }

    }
}
