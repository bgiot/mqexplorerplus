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

        void ShowMessage(object owner, string message);

        void ShowWarning(object owner, string message);

        void ShowError(object owner, string message);

        bool? ShowQuestion(object owner, string message);

        bool ShowYesNoQuestion(object owner, string message);
   
        event EventHandler Before;
        event EventHandler After;

    }
}
