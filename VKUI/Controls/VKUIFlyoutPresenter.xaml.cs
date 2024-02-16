using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace VKUI.Controls {
    public sealed class VKUIFlyoutPresenter : ContentControl {
        public VKUIFlyoutPresenter() { }

        #region Properties

        public static readonly StyledProperty<Control> AboveProperty =
            AvaloniaProperty.Register<VKUIFlyoutPresenter, Control>(nameof(Above));

        public Control Above {
            get => GetValue(AboveProperty);
            set => SetValue(AboveProperty, value);
        }

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
        }
    }
}