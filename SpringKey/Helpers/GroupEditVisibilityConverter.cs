using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SpringKey.Helpers
{
    public class GroupEditVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return Visibility.Collapsed;
            var itemName = values[0].ToString();
            var editingName = values[1].ToString();
            bool isEditing = values.Length > 2 && values[2] is bool b && b;
            bool match = isEditing && itemName == editingName;
            bool isButton = parameter is string s && s == "Button";
            bool invert = !isButton && parameter is string si && si == "Invert";
            if (isButton)
            {
                bool isSpecial = itemName == "全部" || itemName == "未分类";
                return (match || isSpecial) ? Visibility.Collapsed : Visibility.Visible;
            }
            return (match ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
