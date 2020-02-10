using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AeroPlayer
{
    public class PropertyObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        }

    }
}
