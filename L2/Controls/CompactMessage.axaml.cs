using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels.Controls;

namespace ELOR.Laney.Controls {
    public class CompactMessage : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<MessageViewModel> MessageProperty =
            AvaloniaProperty.Register<CompactMessage, MessageViewModel>(nameof(Message));

        public MessageViewModel Message {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly StyledProperty<bool> IsSentTimeVisibleProperty =
            AvaloniaProperty.Register<CompactMessage, bool>(nameof(IsSentTimeVisible));

        public bool IsSentTimeVisible {
            get => GetValue(IsSentTimeVisibleProperty);
            set => SetValue(IsSentTimeVisibleProperty, value);
        }

        #endregion

        #region Template elements

        Border ImagePreview;

        #endregion

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            ImagePreview = e.NameScope.Find<Border>(nameof(ImagePreview));
            isUILoaded = true;
            CheckImages();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == MessageProperty) {
                if (Message == null) return;
                CheckImages();
            }
        }

        private async void CheckImages() {
            if (!isUILoaded || Message == null || Message.PreviewImageUri == null) return;
            await ImagePreview.SetImageBackgroundAsync(Message.PreviewImageUri);
        }
    }
}
