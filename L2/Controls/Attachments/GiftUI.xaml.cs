using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;

namespace ELOR.Laney.Controls.Attachments {
    public class GiftUI : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<Gift> GiftProperty =
            AvaloniaProperty.Register<GiftUI, Gift>(nameof(Gift));

        public Gift Gift {
            get => GetValue(GiftProperty);
            set => SetValue(GiftProperty, value);
        }

        #endregion

        #region Template elements

        Rectangle GiftImage;

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            GiftImage = e.NameScope.Find<Rectangle>(nameof(GiftImage));
            isUILoaded = true;
            Render();
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == GiftProperty) {
                Render();
            }
        }

        private void Render() {
            if (!isUILoaded || Gift == null) return;
            GiftImage.SetImageFillAsync(Gift.ThumbUri, GiftImage.Width, GiftImage.Height);
        }
    }
}