using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;

namespace ELOR.Laney.Controls.Attachments {
    public class BasicAttachment : TemplatedControl {

        #region Properties

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<BasicAttachment, string>(nameof(Title));

        public string Title {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly StyledProperty<string> SubtitleProperty =
            AvaloniaProperty.Register<BasicAttachment, string>(nameof(Subtitle));

        public string Subtitle {
            get => GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public static readonly StyledProperty<string> IconProperty =
            AvaloniaProperty.Register<BasicAttachment, string>(nameof(Icon));

        public string Icon {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        #region Events

        public event EventHandler<RoutedEventArgs> Click;

        #endregion

        #region Template elements

        Button BAButton;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            BAButton = e.NameScope.Find<Button>(nameof(BAButton));
            BAButton.Click += BAButton_Click;
            Unloaded += BasicAttachment_Unloaded;
        }

        private void BAButton_Click(object sender, RoutedEventArgs e) {
            Click?.Invoke(this, e);
        }

        private void BasicAttachment_Unloaded(object sender, RoutedEventArgs e) {
            BAButton.Click -= BAButton_Click;
            Unloaded -= BasicAttachment_Unloaded;
        }

        #endregion
    }
}