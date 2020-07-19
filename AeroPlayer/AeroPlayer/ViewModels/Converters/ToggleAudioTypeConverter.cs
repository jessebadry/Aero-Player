using AeroPlayerService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;

namespace AeroPlayer.ViewModels.Converters
{
    [ValueConversion(typeof(PlayLoop), typeof(string))]
    class ToggleAudioTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = Path.GetFullPath(Path.Join("Images", Enum.GetName(typeof(PlayLoop), value) + ".jpg"));
            return path;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
