using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public sealed class MessageSenderNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is MessageViewModel msg) {
                return VKAPIHelper.GetSenderNameShort(msg);
            }
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}