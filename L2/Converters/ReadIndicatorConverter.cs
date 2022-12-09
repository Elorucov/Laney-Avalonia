using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.ViewModels.Controls;
using System;
using System.Globalization;
using VKUI.Controls;

namespace ELOR.Laney.Converters {
    public class ReadIndicatorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is MessageVMState state) {
                switch (state) {
                    case MessageVMState.Loading: return VKIconNames.Icon16ClockOutline;
                    case MessageVMState.Read: return VKIconNames.Icon16CheckDoubleOutline;
                    case MessageVMState.Unread: return VKIconNames.Icon16CheckOutline;
                    case MessageVMState.Deleted: return VKIconNames.Icon16DeleteOutline;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}