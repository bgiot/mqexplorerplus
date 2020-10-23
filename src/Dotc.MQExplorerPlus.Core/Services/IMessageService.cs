#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;

namespace Dotc.MQExplorerPlus.Core.Services
{
    public interface IMessageService
    {

        void ShowMessage(string message);

        void ShowWarning(string message);

        void ShowError(string message);

        bool? ShowQuestion(string message);

        bool ShowYesNoQuestion(string message);
   
        event EventHandler Before;
        event EventHandler After;

    }
}
