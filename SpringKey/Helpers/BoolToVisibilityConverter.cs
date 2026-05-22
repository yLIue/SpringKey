using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SpringKey.Helpers
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            bool invert = parameter is string s && s == "Invert";
            return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility vis && vis == Visibility.Visible;
        }
    }
}
