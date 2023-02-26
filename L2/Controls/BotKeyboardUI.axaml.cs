using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using System.Runtime.InteropServices;
using VKUI.Controls;

namespace ELOR.Laney.Controls {
    public class BotKeyboardUI : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<BotKeyboard> KeyboardProperty =
            AvaloniaProperty.Register<BotKeyboardUI, BotKeyboard>(nameof(Keyboard));

        public BotKeyboard Keyboard {
            get => GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        #endregion

        #region Template elements

        StackPanel Root;

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Root = e.NameScope.Find<StackPanel>(nameof(Root));
            isUILoaded = true;
            Render();
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == KeyboardProperty) {
                Render();
            }
        }

        private void Render() {
            if (!isUILoaded) return;
            Root.Children.Clear();
            if (Keyboard == null) return;
            VKAPIHelper.GenerateButtons(Root, Keyboard.Buttons);
        }
    }
}