﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using ELOR.Laney.Helpers;
using System;
using System.Globalization;
using ELOR.VKAPILib.Objects;
using ELOR.Laney.Extensions;
using VKUI.Controls;
using Avalonia.Layout;
using Avalonia.Media;

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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    b.SetImageBackgroundAsync(d.GetSizeAndUriForThumbnail(48).Uri, 48);
#pragma warning restore CS4014
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