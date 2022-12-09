using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.ViewModels.Controls;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public class ReadIndicatorVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is MessageViewModel msg) return msg.IsOutgoing;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}