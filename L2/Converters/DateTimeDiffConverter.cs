using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.Extensions;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public sealed class DateTimeDiffConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is DateTime dateTime) {
                string diff = dateTime.ToDiffStringShort();
                if (!String.IsNullOrEmpty(diff)) return $" · {diff}";
            }
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}