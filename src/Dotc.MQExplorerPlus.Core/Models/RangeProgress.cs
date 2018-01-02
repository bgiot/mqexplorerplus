#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Threading;
using System.Threading.Tasks;
using Dotc.MQ;
using Dotc.Mvvm;

namespace Dotc.MQExplorerPlus.Core.Models
{

    public enum LongRunningState
    {
        Yes,
        No,
        Detect,
    }
    public class RangeProgress : BindableBase
    {
        private int _fromRange;
        private int _toRange;
        private int _current;
        private bool _active;
        private CancellationToken _ct;
        private object _lock = new object();

        public ProgressActiveScope Start(LongRunningState isLongRunning = LongRunningState.Detect)
        {
            return StartInternal(null, null, CancellationToken.None, isLongRunning);
        }

        public ProgressActiveScope Start(CancellationToken ct, LongRunningState isLongRunning = LongRunningState.Detect)
        {
            return StartInternal(null, null, ct, isLongRunning);
        }
        public ProgressActiveScope Start(int fromRange, int toRange, LongRunningState isLongRunning = LongRunningState.Detect)
        {
            return StartInternal(fromRange, toRange, CancellationToken.None, isLongRunning);
        }

        public ProgressActiveScope Start(int fromRange, int toRange, CancellationToken ct, LongRunningState isLongRunning = LongRunningState.Detect)
        {
            return StartInternal(fromRange, toRange, ct, isLongRunning);
        }

        public ProgressActiveScope Start(int toRange, LongRunningState isLongRunning = LongRunningState.Detect)
        {
            return StartInternal(0, toRange, CancellationToken.None, isLongRunning);
        }

        public ProgressActiveScope Start(int toRange, CancellationToken ct, LongRunningState isLongRunning = LongRunningState.Detect)
        {
            return StartInternal(0, toRange, ct, isLongRunning);
        }

        private ProgressActiveScope StartInternal(int? fromRange, int? toRange, CancellationToken ct, LongRunningState isLongRunning)
        {
            if (fromRange.HasValue && toRange.HasValue)
            {
                From = fromRange.Value;
                To = toRange.Value;
                Current = fromRange.Value;
                IsIndeterminate = false;
            }
            else
            {
                From = 0;
                To = 0;
                Current = 0;
                IsIndeterminate = true;
            }


            CancellationToken = ct;
            Title = "Working...";

            return new ProgressActiveScope(this, isLongRunning);
        }

        internal void SetRange(int from, int to)
        {
            From = from;
            To = to;
            Current = from;
            IsIndeterminate = false;
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set { SetPropertyAndNotify(ref _title, value); }
        }

        private bool _isRunningForLong;
        public bool IsRunningForLong
        {
            get { return _isRunningForLong; }
            set { SetPropertyAndNotify(ref _isRunningForLong, value); }
        }

        private bool _isIndeterminate;
        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                SetPropertyAndNotify(ref _isIndeterminate, value);
                OnPropertyChanged(nameof(IsDeterminate));
            }
        }

        public bool IsDeterminate
        {
            get { return !_isIndeterminate; }

        }

        public CancellationToken CancellationToken
        {
            get { return _ct; }
            protected set { _ct = value; }
        }

        public int From
        {
            get { return _fromRange; }
            set { SetPropertyAndNotify(ref _fromRange, value); }
        }

        public int To
        {
            get { return _toRange; }
            set { SetPropertyAndNotify(ref _toRange, value); }
        }

        public int Current
        {
            get { return _current; }
            set
            {
                SetPropertyAndNotify(ref _current, value);
            }
        }

        public bool Active
        {
            get { return _active; }
            internal set
            {
                SetPropertyAndNotify(ref _active, value);
                if (value == false)
                {
                    IsRunningForLong = false;
                }
            }
        }

        public void Next(int step = 1)
        {
            if (!IsIndeterminate && step != 0)
            {
                lock (_lock)
                {
                    Current = Current + step;
                }
            }
        }
    }
}
