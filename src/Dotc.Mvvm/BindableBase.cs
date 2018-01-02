using System;
using System.Linq.Expressions;
using System.Windows.Threading;
using Dotc.Common;

namespace Dotc.Mvvm
{
    public abstract class BindableBase : ObservableObject
    {

        protected Dispatcher CurrentDispatcher { get; private set; }


        protected BindableBase()
        {
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
        }


        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            base.OnPropertyChanged(propertyName);
        }

    }
}
