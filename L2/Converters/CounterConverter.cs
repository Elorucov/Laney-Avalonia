using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.Core.Localization;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public class CounterConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string word = parameter.ToString();
            if (value is int num) {
                return Localizer.GetDeclensionFormatted(num, word);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}