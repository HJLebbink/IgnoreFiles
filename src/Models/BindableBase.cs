﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IgnoreFiles.Models
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool Set<T>(ref T value, T newValue, IEqualityComparer<T> comparer = null, [CallerMemberName] string propertyName = null)
        {
            IEqualityComparer<T> comparerToUse = comparer ?? EqualityComparer<T>.Default;

            if (!comparerToUse.Equals(value, newValue))
            {
                value = newValue;
                this.OnPropertyChangedCore(propertyName);
                return true;
            }

            return false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChangedCore(propertyName);
        }

        private void OnPropertyChangedCore(string propertyName)
        {
            Debug.Assert(propertyName != null, "Property name cannot be null");
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}