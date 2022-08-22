using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;

namespace VKUI.Controls {
    public sealed class Placeholder : TemplatedControl {
        public Placeholder() { }

        #region Properties

        public static readonly StyledProperty<Control> IconProperty =
            AvaloniaProperty.Register<Placeholder, Control>(nameof(Icon));

        public static readonly StyledProperty<string> HeaderProperty =
            AvaloniaProperty.Register<Placeholder, string>(nameof(Header));

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<Placeholder, string>(nameof(Text));

        public static readonly StyledProperty<Control> ActionProperty =
            AvaloniaProperty.Register<Placeholder, Control>(nameof(Action));

        public Control Icon {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Header {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Text {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public Control Action {
            get => GetValue(ActionProperty);
            set => SetValue(ActionProperty, value);
        }

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == IconProperty) {
                if (Icon == null) return;
                if (Icon is VKIcon || Icon is Avatar) return; // valid
                throw new ArgumentException($"Value must be VKIcon or Avatar, not {Icon.GetType()}", nameof(Icon));
            }
        }
    }
}
