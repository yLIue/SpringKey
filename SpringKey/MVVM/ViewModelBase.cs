using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpringKey.MVVM
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly SynchronizationContext? _syncContext = SynchronizationContext.Current;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler == null) return;

            var args = new PropertyChangedEventArgs(propertyName ?? string.Empty);

            if (_syncContext != null)
            {
                _syncContext.Post(_ => handler(this, args), null);
            }
            else
            {
                handler(this, args);
            }
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
