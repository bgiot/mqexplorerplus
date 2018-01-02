#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Windows.Input;

namespace Dotc.MQExplorerPlus.Core.Services
{
    public interface IKeyboardCommands
    {
        ICommand F5Command { get; }
        ICommand CtlF5Command { get; }
    }
}
