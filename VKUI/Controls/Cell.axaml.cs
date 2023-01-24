using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;
using System.Diagnostics.Metrics;

namespace VKUI.Controls {
    public class Cell : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<Control> BeforeProperty =
            AvaloniaProperty.Register<Cell, Control>(nameof(Before));

        public static readonly StyledProperty<string> HeaderProperty =
            AvaloniaProperty.Register<Cell, string>(nameof(Header));

        public static readonly StyledProperty<string> SubtitleProperty =
            AvaloniaProperty.Register<Cell, string>(nameof(Subtitle));

        public static readonly StyledProperty<Control> AfterProperty =
            AvaloniaProperty.Register<Cell, Control>(nameof(After));

        public Control Before {
            get => GetValue(BeforeProperty);
            set => SetValue(BeforeProperty, value);
        }

        public string Header {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Subtitle {
            get => GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public Control After {
            get => GetValue(AfterProperty);
            set => SetValue(AfterProperty, value);
        }

        #endregion
    }
}
