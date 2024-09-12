using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace EntryEditor.Models.Converters
{
    internal sealed class ModifiedDateConverter : IValueConverter
    {
        static StringBuilder stringBuilder = new();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = string.Empty;

            if(parameter is not null)
            {
                stringBuilder.Clear();

                stringBuilder.Append("Modified: ");
                stringBuilder.Append((value ?? "default_value") + ". ");
                var isCorrect = bool.Parse(parameter.ToString());

                if (!isCorrect)
                {
                    stringBuilder.Append("Leaving fields empty is not recommended.");
                }

                result = stringBuilder.ToString();
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
