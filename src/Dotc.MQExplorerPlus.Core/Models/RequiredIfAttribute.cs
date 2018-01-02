#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security;

namespace Dotc.MQExplorerPlus.Core.Models
{

    public sealed class RequiredIfNotEmptyAttribute : ValidationAttribute
    {
        public string PropertyName { get; }

        public RequiredIfNotEmptyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var isRequired = IsRequired(validationContext);

            if (isRequired)
            {
                var s = value as SecureString;
                if (s != null && s.Length == 0)
                    return new ValidationResult(ErrorMessage);

                if (string.IsNullOrEmpty(Convert.ToString(value, CultureInfo.InvariantCulture)))
                    return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }

        private bool IsRequired(ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(PropertyName);
            var currentValue = property.GetValue(validationContext.ObjectInstance, null);

            if (currentValue is SecureString)
            {
                return ((SecureString) currentValue).Length > 0;
            }

            var s = currentValue as string;
            return !string.IsNullOrEmpty(s);
        }
    }


    public sealed class RequiredIfAttribute : ValidationAttribute
    {
        public string PropertyName { get; }
        public object[] Values { get; }

        public RequiredIfAttribute(string propertyName, params object[] equalsValues)
        {
            PropertyName = propertyName;
            Values = equalsValues;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var isRequired = IsRequired(validationContext);

            if (isRequired)
            {
                var ss = value as SecureString;
                if (ss != null && ss.Length == 0)
                    return new ValidationResult(ErrorMessage);

                if (string.IsNullOrEmpty(Convert.ToString(value, CultureInfo.InvariantCulture)))
                    return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }

        private bool IsRequired(ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(PropertyName);
            var currentValue = property.GetValue(validationContext.ObjectInstance, null);

            return Values.Contains(currentValue);
        }
    }
}
