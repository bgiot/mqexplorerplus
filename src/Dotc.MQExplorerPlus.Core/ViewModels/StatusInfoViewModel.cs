#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using Dotc.Mvvm;

namespace Dotc.MQExplorerPlus.Core.ViewModels
{
    public interface IStatusInfo
    {
        StatusInfoViewModel StatusInfoViewModel { get; }
    }
    public abstract class StatusInfoViewModel : BindableBase
    {
        private DateTime _lastUpdateTs;

        protected StatusInfoViewModel()
        {
            _lastUpdateTs = DateTime.Now;
        }

        public DateTime LastUpdateTimestamp
        {
            get { return _lastUpdateTs; }
            set { SetPropertyAndNotify(ref _lastUpdateTs, value); }
        }
    }
}
