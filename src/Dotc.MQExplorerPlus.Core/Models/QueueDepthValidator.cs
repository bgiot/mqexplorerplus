#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public static class QueueDepthValidator
    {

        public const string RegexPattern = @"^\d+$|^\d{1,2}%$|^100%$";

        private static Regex Validator = new Regex(RegexPattern);
        public static bool IsThresholdValid(string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            var result = Validator.IsMatch(value);
            return result;
        }

        public static bool IsDepthOverThreshold(int depth, int maxDepth, string threshold)
        {
            if (string.IsNullOrEmpty(threshold))
                return false;

            if (!QueueDepthWarningThresholdValidationAttribute.IsValid(threshold))
                return false;

            if (threshold.EndsWith("%", StringComparison.Ordinal))
            {
                var percentage = int.Parse(threshold.TrimEnd('%'), CultureInfo.InvariantCulture);
                if (percentage == 0) return false;

                var cutoff = maxDepth * (percentage / 100d);
                return depth > cutoff;
            }
            else
            {
                var cutoff = int.Parse(threshold, CultureInfo.InvariantCulture);
                if (cutoff == 0) return false;
                return depth > cutoff;
            }
        }
    }
}
