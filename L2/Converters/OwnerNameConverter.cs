using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.Core;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public class OwnerNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is long id) {
                return CacheManager.GetNameOnly(id);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}