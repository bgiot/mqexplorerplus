#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.Mvvm;
using System;
using System.Timers;
using System.Windows;

namespace Dotc.MQExplorerPlus.Core.Services
{
    public sealed class CountdownService : BindableBase, IDisposable
    {
        private readonly Timer _countDownDelayTimer;
        private int _currentCountdown;
        private int _countdownStart;
        private bool _isOn;

        public CountdownService(int countDownStart)
        {
            _countDownDelayTimer = new Timer(1000);
            WeakEventManager<Timer, ElapsedEventArgs>
                .AddHandler(_countDownDelayTimer, "Elapsed", _countDownDelayTimer_Elapsed);
            _countDownDelayTimer.Enabled = false;
            CountdownStart = countDownStart;
            GC.SuppressFinalize(this);
        }

        public event EventHandler Elapsed;

        public int CountdownStart
        {
            get { return _countdownStart; }
            set
            {
                if (SetPropertyAndNotify(ref _countdownStart, value))
                {
                    CurrentCountdown = value;
                }
            }
        }

        public void ResetCountdown()
        {
            CurrentCountdown = CountdownStart;
        }
        public int CurrentCountdown
        {
            get { return _currentCountdown; }
            private set { SetPropertyAndNotify(ref _currentCountdown, value); }
        }


        private void _countDownDelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrentCountdown--;
            if (CurrentCountdown == 0)
            {
                var handler = Elapsed as EventHandler;
                if (handler != null) handler.Invoke(this, EventArgs.Empty);
                ResetCountdown();
            }
        }


        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                if (SetPropertyAndNotify(ref _isOn, value))
                {
                    ResetCountdown();
                    _countDownDelayTimer.Enabled = _isOn;
                }
            }
        }

        public void Allow(bool resetCountdown = false)
        {
            if (resetCountdown) ResetCountdown();
            _countDownDelayTimer.Enabled = _isOn;
        }

        public void Disable()
        {
            _countDownDelayTimer.Enabled = false;
        }

        public void Dispose()
        {
            _countDownDelayTimer.Dispose();
        }
    }
}
