using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using VKUI.Controls;

namespace VKUI.Popups {
    public sealed class VKUIFlyout : PopupFlyoutBase {
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<VKUIFlyout, object>(nameof(Content));

        public static readonly StyledProperty<Thickness> PaddingProperty =
            AvaloniaProperty.Register<VKUIFlyout, Thickness>(nameof(Padding));

        [Content]
        public object Content {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public Thickness Padding {
            get => GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        protected override Control CreatePresenter() {
            return new VKUIFlyoutPresenter {
                [!ContentControl.ContentProperty] = this[!ContentProperty],
                [!PaddingProperty] = this[!PaddingProperty]
            };
        }
    }
}