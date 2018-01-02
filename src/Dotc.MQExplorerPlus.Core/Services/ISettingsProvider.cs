#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Models;

namespace Dotc.MQExplorerPlus.Core.Services
{
    public interface ISettingsProvider
    {
        IUserSettings ReadUserSettings();
        void WriteUserSettings(IUserSettings settings);

        IAppSettings GetAppSettings();

    }
}
