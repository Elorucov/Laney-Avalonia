using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ELOR.Laney.Extensions;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public sealed class AvatarGradientConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int id) {
                return id.GetGradient();
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}