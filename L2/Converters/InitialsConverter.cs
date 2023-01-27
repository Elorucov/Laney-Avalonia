using Avalonia.Data.Converters;
using Avalonia;
using System;
using System.Globalization;
using ELOR.Laney.Extensions;

namespace ELOR.Laney.Converters {
    internal class InitialsConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is string name) {
                return name.GetInitials();
            }
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}