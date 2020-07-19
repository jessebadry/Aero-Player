using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace AeroPlayer.ViewModels.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
   
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Console.WriteLine("Converting " + (bool)value);
            if ((bool)value)
                
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
