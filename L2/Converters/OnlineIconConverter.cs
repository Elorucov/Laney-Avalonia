using Avalonia;
using Avalonia.Data.Converters;
using ELOR.VKAPILib.Objects;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public sealed class OnlineIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is UserOnlineInfo info) {
                if (!info.Visible) return null;
                if (!info.IsOnline) return null;
                string icon = info.IsMobile ? "OnlineMobileIcon" : "OnlineIcon";
                return Application.Current.Resources[icon];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}