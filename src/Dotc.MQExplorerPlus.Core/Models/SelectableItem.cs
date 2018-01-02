#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.Mvvm;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public class SelectableItem : BindableBase
    {

        private ISelectedEventHandler _eventHandler;

        private bool _selected;
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (SetPropertyAndNotify(ref _selected, value))
                {
                    _eventHandler?.SelectedChanged(this);
                }
            }
        }

        internal void Attach(ISelectedEventHandler eventHandler)
        {
            _eventHandler = eventHandler;
        }

        internal void Detach()
        {
            _eventHandler = null;
        }
    }
}
