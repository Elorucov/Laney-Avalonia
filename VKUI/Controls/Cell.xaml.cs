using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using System;

namespace VKUI.Controls {
    public class Cell : TemplatedControl {
        #region Template controls

        bool isTemplateLoaded = false;
        ContentPresenter AfterControl;

        #endregion

        #region Properties

        public static readonly StyledProperty<Control> BeforeProperty =
            AvaloniaProperty.Register<Cell, Control>(nameof(Before));

        public static readonly StyledProperty<string> HeaderProperty =
            AvaloniaProperty.Register<Cell, string>(nameof(Header));

        public static readonly StyledProperty<string> SubtitleProperty =
            AvaloniaProperty.Register<Cell, string>(nameof(Subtitle));

        public static readonly StyledProperty<object> AfterProperty =
            AvaloniaProperty.Register<Cell, object>(nameof(After));

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

        public object After {
            get => GetValue(AfterProperty);
            set => SetValue(AfterProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            AfterControl = e.NameScope.Find<ContentPresenter>(nameof(AfterControl));

            isTemplateLoaded = true;
            CheckAfterValue();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (!isTemplateLoaded) return;

            if (change.Property == AfterProperty) CheckAfterValue();
        }

        private void CheckAfterValue() {
            if (After == null) {
                AfterControl.Content = null;
            } else if (After is Control control) {
                AfterControl.Content = control;
            } else if (After is string text) {
                AfterControl.Content = new TextBlock {
                    Text = text
                };
            } else {
                throw new ArgumentException("Wrong value type! Required Conrol or string", nameof(After));
            }
        }

        #endregion
    }
}
