#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Threading;
using Dotc.Wpf;
using Nito.AsyncEx;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public sealed class ProgressActiveScope : IDisposable
    {
        private readonly RangeProgress _progress;
        private readonly Timer _delayForLongRunningSetFlag;
        private readonly IDisposableProgress<int> _internalProgress;
        internal ProgressActiveScope(RangeProgress progress, LongRunningState isLongRunning)
        {
            _progress = progress;

            SetActive(true);

            if (isLongRunning == LongRunningState.Yes)
            {
                SetIsRunningForLong(true);
            }
            else
            {
                SetIsRunningForLong(false);
                if (isLongRunning == LongRunningState.Detect)
                {
                    _delayForLongRunningSetFlag = new Timer(_delayForLongRunningSetFlag_Tick, null, 1000, -1);
                }
            }

            _internalProgress = ObservableProgress<int>.CreateForUi((value) =>
                   {
                       _progress.Current = value;
                   });

            GC.SuppressFinalize(this);
        }

        private void _delayForLongRunningSetFlag_Tick(object state)
        {
            SetIsRunningForLong(true);
        }

        private void SetIsRunningForLong(bool value)
        {
            UIDispatcher.Execute(() =>
            {
                _progress.IsRunningForLong = value;

            });
        }

        public void Report(int value)
        {
            _internalProgress.Report(value);
        }

        public void ReportNext(int step = 1)
        {
            Report(_progress.Current + step);
        }

        public IProgress<int> Progress
        {
            get { return _internalProgress; }
        }

        public void SetTitle(string title)
        {
            UIDispatcher.Execute(() =>
            {
                _progress.Title = title;
            });
        }

        private void SetActive(bool active)
        {
            UIDispatcher.Execute(() =>
            {
                _progress.Active = active;
            });
        }

        internal void SetRange(int from, int to)
        {
            UIDispatcher.Execute(() =>
            {
                _progress.SetRange(from, to);
            });
        }

        public CancellationToken CancellationToken
        {
            get { return _progress.CancellationToken; }
        }
        public void Dispose()
        {
            _progress.Active = false;
            _progress.IsRunningForLong = false;
            if (_delayForLongRunningSetFlag != null)
                _delayForLongRunningSetFlag.Dispose();
            _internalProgress.Dispose();
        }
    }
}
