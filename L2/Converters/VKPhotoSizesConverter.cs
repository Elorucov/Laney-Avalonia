﻿using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public class VKPhotoSizesConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is IPreview preview) {
                int width = 360;
                Int32.TryParse((string)parameter, out width);
                return preview.GetSizeAndUriForThumbnail(width).Uri;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}