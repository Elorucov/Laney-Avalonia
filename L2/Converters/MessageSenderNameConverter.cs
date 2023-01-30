using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using ELOR.Laney.Extensions;
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

    public sealed class MessageInChatConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            InlineCollection ic = new InlineCollection();
            if (value != null && value is MessageViewModel msg) {
                string sender = VKAPIHelper.GetSenderNameShort(msg);
                string text = msg.ToString();

                if (!String.IsNullOrEmpty(sender)) {
                    Run r = new Run {
                        Text = sender,
                    };
                    r.RegisterThemeResource(Run.ForegroundProperty, "VKTextPrimaryBrush");
                    ic.Add(r);
                }
                if (!String.IsNullOrEmpty(text)) {
                    Run r = new Run {
                        Text = text,
                    };
                    ic.Add(r);
                }
            }
            return ic;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return AvaloniaProperty.UnsetValue;
        }
    }
}