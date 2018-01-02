using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dotc.Mvvm
{
    public static class PropertySupport
    {

        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("The expression is not a member access expression", nameof(propertyExpression));
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("The member access expression does not access a property", nameof(propertyExpression));
            }

            var getMethod = property.GetMethod;
            if (getMethod.IsStatic)
            {
                throw new ArgumentException("The referenced property is a static property", nameof(propertyExpression));
            }

            return memberExpression.Member.Name;
        }
    }

}
