﻿#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.MQExplorerPlus.Core.Controllers;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public sealed class MainViewModel : ViewModel
    {
        public MainViewModel(IMainView view, IApplicationController appController) : base(view, appController)
        {
            ViewService = App.ViewService;
        }

        public IViewService ViewService { get; private set; }

    }
}
