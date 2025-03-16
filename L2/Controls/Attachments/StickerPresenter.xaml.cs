using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Skia.Lottie;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ELOR.Laney.Controls.Attachments {
    public class StickerPresenter : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<Sticker> StickerProperty =
            AvaloniaProperty.Register<StickerPresenter, Sticker>(nameof(Sticker));

        public Sticker Sticker {
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
            if (!isUILoaded || Sticker == null) return;
            await StickerView.SetImageBackgroundAsync(Sticker.GetSizeAndUriForThumbnail(this.Width).Uri, Width, Height);
            // ImageLoader.SetBackgroundSource(StickerView, Sticker.GetSizeAndUriForThumbnail(this.Width).Uri);

            if (Settings.AnimateStickers && !String.IsNullOrEmpty(Sticker.AnimationUrl)) {
                await Task.Delay(250); // надо
                var uri = new Uri(Sticker.AnimationUrl);
                var file = await CacheManager.GetFileFromCacheAsync(uri);
                if (file) {
                    string local = $"file://{Path.Combine(App.LocalDataPath, "cache", uri.Segments.Last()).Replace("\\", "/")}";
                    Lottie ls = new Lottie(new Uri("file://")) { // разраб либы не прописал конструктор public Lottie() без параметров, пришлось костылить.
                        Stretch = Stretch.Uniform,
                        StretchDirection = StretchDirection.Both,
                        RepeatCount = 4,
                        Path = local
                    };
                    StickerView.Child = ls;
                    StickerView.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
    }
}