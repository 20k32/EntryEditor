using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace EntryEditor.Models.Converters
{
    internal class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Visibility result = Visibility.Collapsed;
            
            if(value is bool visibility)
            {
                var invert = bool.Parse(parameter.ToString());
                result = visibility == invert
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => value;
    }
}
