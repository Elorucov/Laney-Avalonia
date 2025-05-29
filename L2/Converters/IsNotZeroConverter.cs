using Avalonia.Data.Converters;
using Avalonia;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public class IsNotZeroConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // decimal is not working.
            if (value is sbyte sb) return sb != 0;
            if (value is byte ub) return ub != 0;
            if (value is short ss) return ss != 0;
            if (value is ushort us) return us != 0;
            if (value is int si) return si != 0;
            if (value is uint ui) return ui != 0;
            if (value is long sl) return sl != 0;
            if (value is long ul) return ul != 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}
