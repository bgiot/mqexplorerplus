using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dotc.Mvvm
{
    public abstract class ValidatableBindableBase : BindableBase, INotifyDataErrorInfo
    {
        ErrorsContainer<string> _errorsContainer;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged = delegate { };

        public IEnumerable GetErrors(string propertyName)
        {
            return ErrorsContainer.GetErrors(propertyName);
        }

        public void SetErrors<TProperty>(Expression<Func<TProperty>> propertyExpression, IList<string> errors)
        {
            ErrorsContainer.SetErrors(propertyExpression, errors);
        }

        public bool HasErrors => ErrorsContainer.HasErrors;

        public Dictionary<string, List<string>> GetAllErrors()
        {
            return ErrorsContainer.GetAllErrors();
        }


        protected ErrorsContainer<string> ErrorsContainer
        {
            get
            {
                if (_errorsContainer == null)
                {
                    _errorsContainer =
                        new ErrorsContainer<string>(RaiseErrorsChanged);
                    ValidateProperties();
                }

                return _errorsContainer;
            }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            OnErrorsChanged(new DataErrorsChangedEventArgs(propertyName));
        }

        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
        {
            ErrorsChanged(this, e);
        }

        protected override bool SetPropertyAndNotify<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            var result = propertyName != null && base.SetPropertyAndNotify(ref storage, value, propertyName);

            if (result && !string.IsNullOrEmpty(propertyName))
            {
                ValidateProperty(propertyName);
            }
            return result;
        }

        protected bool ValidateProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var propertyInfo = GetType().GetRuntimeProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException("Invalid property name", propertyName);
            }

            var propertyErrors = new List<string>();
            var isValid = TryValidateProperty(propertyInfo, propertyErrors);
            ErrorsContainer.SetErrors(propertyInfo.Name, propertyErrors);

            return isValid;
        }

        public bool ValidateProperties()
        {
            // Get all the properties decorated with the ValidationAttribute attribute.
            var propertiesToValidate = GetType()
                                                        .GetRuntimeProperties()
                                                        .Where(c => c.GetCustomAttributes(typeof(ValidationAttribute)).Any());

            foreach (var propertyInfo in propertiesToValidate)
            {
                var propertyErrors = new List<string>();
                TryValidateProperty(propertyInfo, propertyErrors);

                // If the errors have changed, save the property name to notify the update at the end of this method.
                ErrorsContainer.SetErrors(propertyInfo.Name, propertyErrors);
            }

            return ErrorsContainer.HasErrors;
        }

        private bool TryValidateProperty(PropertyInfo propertyInfo, List<string> propertyErrors)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this) { MemberName = propertyInfo.Name };
            var propertyValue = propertyInfo.GetValue(this);

            // Validate the property
            var isValid = Validator.TryValidateProperty(propertyValue, context, results);

            if (results.Any())
            {
                propertyErrors.AddRange(results.Select(c => c.ErrorMessage));
            }

            return isValid;
        }

    }
}
