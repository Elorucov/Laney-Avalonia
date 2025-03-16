using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System.Linq;

namespace ELOR.Laney {
    public class UGCStickerPresenter : TemplatedControl {

        #region Properties

        public static readonly StyledProperty<UGCSticker> StickerProperty =
            AvaloniaProperty.Register<UGCStickerPresenter, UGCSticker>(nameof(Sticker));

        public UGCSticker Sticker {
            get => GetValue(StickerProperty);
            set => SetValue(StickerProperty, value);
        }

        #endregion

        #region Template elements

        Border StickerView;

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            StickerView = e.NameScope.Find<Border>(nameof(StickerView));
            isUILoaded = true;
            Render();
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == StickerProperty) {
                Render();
            }
        }

        private async void Render() {
            if (!isUILoaded || Sticker == null || Sticker.Images == null || Sticker.Images.Count == 0) return;
            await StickerView.SetImageBackgroundAsync(Sticker.Images.LastOrDefault().Uri, Width, Height);
        }
    }
}