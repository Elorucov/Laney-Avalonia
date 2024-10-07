using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.Extensions;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public class DateConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is DateTime dateTime) {
                return parameter != null && parameter is string s && s == "t"
                    ? dateTime.ToHumanizedTimeOrDateString() 
                    : dateTime.ToHumanizedDateString();
            }
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }

    public class TimeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is TimeSpan ts) return ts.ToTimeWithHourIfNeeded();
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}