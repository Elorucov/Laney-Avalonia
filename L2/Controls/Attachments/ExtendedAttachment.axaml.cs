using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using ELOR.Laney.Helpers;
using System;
using System.Threading.Tasks.Dataflow;

namespace ELOR.Laney.Controls.Attachments {
    public class ExtendedAttachment : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<ExtendedAttachment, string>(nameof(Title));

        public string Title {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly StyledProperty<string> SubtitleProperty =
            AvaloniaProperty.Register<ExtendedAttachment, string>(nameof(Subtitle));

        public string Subtitle {
            get => GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public static readonly StyledProperty<Uri> PreviewProperty =
            AvaloniaProperty.Register<ExtendedAttachment, Uri>(nameof(Preview));

        public Uri Preview {
            get => GetValue(PreviewProperty);
            set => SetValue(PreviewProperty, value);
        }

        public static readonly StyledProperty<string> ActionButtonTextProperty =
            AvaloniaProperty.Register<ExtendedAttachment, string>(nameof(ActionButtonText));

        public string ActionButtonText {
            get => GetValue(ActionButtonTextProperty);
            set => SetValue(ActionButtonTextProperty, value);
        }

        #endregion

        #region Events

        public event EventHandler<RoutedEventArgs> Click;
        public event EventHandler<RoutedEventArgs> ActionButtonClick;

        #endregion

        #region Template elements

        Button EAButton;
        Border PreviewImage;
        Button ActionButton;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            EAButton = e.NameScope.Find<Button>(nameof(EAButton));
            PreviewImage = e.NameScope.Find<Border>(nameof(PreviewImage));
            ActionButton = e.NameScope.Find<Button>(nameof(ActionButton));

            EAButton.Click += EAButton_Click;
            ActionButton.Click += ActionButton_Click;
            Unloaded += BasicAttachment_Unloaded;

            if (Preview != null) PreviewImage.SetImageBackgroundAsync(Preview);
        }

        #endregion

        private void EAButton_Click(object sender, RoutedEventArgs e) {
            Click?.Invoke(this, e);
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e) {
            ActionButtonClick?.Invoke(this, e);
        }

        private void BasicAttachment_Unloaded(object sender, RoutedEventArgs e) {
            EAButton.Click -= EAButton_Click;
            ActionButton.Click -= ActionButton_Click;
            Unloaded -= BasicAttachment_Unloaded;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == PreviewProperty) {
                if (Preview != null) PreviewImage.SetImageBackgroundAsync(Preview);
            }
        }
    }
}
