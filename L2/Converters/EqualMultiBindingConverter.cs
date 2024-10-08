using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace ELOR.Laney.Converters {
    // Returns true if all values is same object.
    internal class EqualMultiBindingConverter : IMultiValueConverter {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
            bool ka = values.All(o => o == values[0]);
            if (ka) {
                string str = String.Join(", ", values);
                Debug.WriteLine($"EqualMultiBindingConverter: {str}");
            }
            return ka;
        }
    }
}
