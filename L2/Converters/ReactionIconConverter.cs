using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.Core;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public class ReactionIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not int) return null;
            int id = (int)value;
            return new Uri(CacheManager.GetStaticReactionUrl(id));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }

    public class SelectedReactionConverter : IMultiValueConverter {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Count < 2 || values[0] is not int || values[1] is not int) return false;
            int id = (int)values[0];
            int sid = (int)values[1];
            return id == sid;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}