using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace Dotc.Mvvm
{
    public class ErrorsContainer<T>
    {
        private static readonly T[] NoErrors = new T[0];
        private readonly Action<string> _raiseErrorsChanged;
        private readonly Dictionary<string, List<T>> _validationResults;


        public ErrorsContainer(Action<string> raiseErrorsChanged)
        {
            if (raiseErrorsChanged == null)
            {
                throw new ArgumentNullException(nameof(raiseErrorsChanged));
            }

            _raiseErrorsChanged = raiseErrorsChanged;
            _validationResults = new Dictionary<string, List<T>>();
        }

        public bool HasErrors => _validationResults.Count != 0;

        public Dictionary<string, List<T>> GetAllErrors()
        {
            return _validationResults;
        }

        public IList<T> GetErrors(string propertyName)
        {
            var localPropertyName = propertyName ?? string.Empty;
            if (_validationResults.ContainsKey(localPropertyName))
                return _validationResults[localPropertyName];
            else
                return NoErrors;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void ClearErrors<TProperty>(Expression<Func<TProperty>> propertyExpression)
        {
            var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            ClearErrors(propertyName);
        }

        public void ClearErrors(string propertyName)
        {
            SetErrors(propertyName, new List<T>());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void SetErrors<TProperty>(Expression<Func<TProperty>> propertyExpression, IList<T> propertyErrors)
        {
            var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            SetErrors(propertyName, propertyErrors);
        }

        public void SetErrors(string propertyName, IList<T> newValidationResults)
        {
            var localPropertyName = propertyName ?? string.Empty;
            var hasCurrentValidationResults = _validationResults.ContainsKey(localPropertyName);
            var hasNewValidationResults = newValidationResults != null && newValidationResults.Any();

            if (hasCurrentValidationResults || hasNewValidationResults)
            {
                if (hasNewValidationResults)
                {
                    _validationResults[localPropertyName] = new List<T>(newValidationResults);
                    _raiseErrorsChanged(localPropertyName);
                }
                else
                {
                    _validationResults.Remove(localPropertyName);
                    _raiseErrorsChanged(localPropertyName);
                }
            }
        }
    }

}
