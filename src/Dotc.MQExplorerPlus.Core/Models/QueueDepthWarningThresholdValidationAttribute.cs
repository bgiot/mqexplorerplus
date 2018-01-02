#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel.DataAnnotations;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public class QueueDepthWarningThresholdValidationAttribute : ValidationAttribute
    {

        public override bool IsValid(object value)
        {
            return IsValid((string) value);
        }

        public static bool IsValid(string value)
        {
            return QueueDepthValidator.IsThresholdValid((string) value);
        }
    }
}
