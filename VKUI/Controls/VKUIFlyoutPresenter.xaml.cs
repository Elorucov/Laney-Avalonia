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

        public FlyoutBase ParentFlyout { get; set; }

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            StackPanel dismissArea = e.NameScope.Find<StackPanel>("DismissArea");
            dismissArea.PointerReleased += DismissArea_PointerReleased;

            base.OnApplyTemplate(e);
        }

        private void DismissArea_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e) {
            StackPanel dismissArea = sender as StackPanel;
            dismissArea.PointerReleased -= DismissArea_PointerReleased;
            if (ParentFlyout != null) ParentFlyout.Hide();
        }
    }
}