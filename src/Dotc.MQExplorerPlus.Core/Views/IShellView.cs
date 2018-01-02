#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel;

namespace Dotc.MQExplorerPlus.Core.Views
{
    public interface IShellView : IView
    {
        event CancelEventHandler Closing;
        void Close();
        void Show();
    }
}
