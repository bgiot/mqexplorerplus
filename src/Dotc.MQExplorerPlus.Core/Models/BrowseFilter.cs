#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using Dotc.MQ;
using Dotc.Mvvm;
using System.Text.RegularExpressions;

namespace Dotc.MQExplorerPlus.Core.Models
{

    public class BrowseFilter : BindableBase, IBrowseFilter
    {
        private string _pattern;
        private int? _priority;
        private DateTime? _putTsFrom;
        private DateTime? _putTsTo;

        private Regex _patternMatcher;
        private object _lock = new object();

        public string Pattern
        {
            get { return _pattern; }
            set
            {
                SetPropertyAndNotify(ref _pattern, value);
                _patternMatcher = null;
            }
        }

        public int? Priority
        {
            get { return _priority; }
            set { SetPropertyAndNotify(ref _priority, value); }
        }

        public DateTime? PutTimestampFrom
        {
            get { return _putTsFrom; }
            set { SetPropertyAndNotify(ref _putTsFrom, value); }
        }

        public DateTime? PutTimestampTo
        {
            get { return _putTsTo; }
            set { SetPropertyAndNotify(ref _putTsTo, value); }
        }

        private Regex GetPatternMatcher()
        {
            if (_patternMatcher == null)
            {
                lock (_lock)
                {
                    if (_patternMatcher == null)
                    {
                        _patternMatcher = new Regex(_pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    }
                }
            }

            return _patternMatcher;

        }
        public bool IsMatch(IMessage message)
        {

            if (message == null) throw new ArgumentNullException(nameof(message));

            if (
                (PutTimestampFrom.HasValue && message.PutTimestamp < PutTimestampFrom.Value)
                || (PutTimestampTo.HasValue && message.PutTimestamp >= PutTimestampTo.Value.AddDays(1))
                )
            {
                return false;
            }

            if (Priority.HasValue && message.ExtendedProperties.Priority != Priority)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_pattern) && !string.IsNullOrEmpty(message.Text))
            {
                if (!GetPatternMatcher().IsMatch(message.Text))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
