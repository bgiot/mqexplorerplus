using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dotc.Common
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetPropertyAndNotify<T>(ref T storage, T value, [CallerMemberNameAttribute] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var eventHandler = PropertyChanged;
            eventHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
