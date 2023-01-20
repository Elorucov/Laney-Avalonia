using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using VKUI.Controls;

namespace VKUI.Popups {
    public sealed class VKUIFlyout : FlyoutBase {
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<VKUIFlyout, object>(nameof(Content));

        [Content]
        public object Content {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        protected override Control CreatePresenter() {
            return new VKUIFlyoutPresenter {
                [!ContentControl.ContentProperty] = this[!ContentProperty]
            };
        }
    }
}
