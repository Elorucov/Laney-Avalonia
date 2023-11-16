using Avalonia;
using Avalonia.Data.Converters;
using ELOR.Laney.Controls;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System;
using System.Globalization;

namespace ELOR.Laney.Converters {
    public class VKPhotoSizesConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null) {
                if (value is IPreview preview) {
                    double width = 0, height = 0;

                    if (parameter is string size) {
                        var s = size.Split('x');
                        Double.TryParse(s[0], out width);
                        Double.TryParse(s[1], out height);
                    }

                    return preview.GetSizeAndUriForThumbnail(width, height).Uri;
                } else if (value is Sticker sticker) {
                    double width = MessageBubble.BUBBLE_FIXED_WIDTH;
                    Double.TryParse((string)parameter, out width);
                    return sticker.GetSizeAndUriForThumbnail(width).Uri;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}