using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using System;
using System.Globalization;
using VKUI.Controls;

namespace ELOR.Laney.Converters {
    public class DocumentIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value is Document d) {
                Border b = new Border {
                    Width = 48, Height = 48,
                    Background = VKAPIHelper.GetDocumentIconBackground(d.Type),
                    CornerRadius = new CornerRadius(4)
                };
                if (d.Preview != null) {
                    new System.Action(async () => await b.SetImageBackgroundAsync(d.GetSizeAndUriForThumbnail(b.Width, b.Height).Uri, b.Width, b.Height))();
                } else {
                    b.Child = new VKIcon {
                        Foreground = new SolidColorBrush(Colors.White),
                        Id = VKAPIHelper.GetDocumentIcon(d.Type),
                        Width = 28, Height = 28,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                }
                return b;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}