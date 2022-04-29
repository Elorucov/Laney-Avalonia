using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace VKUI.Controls {
    public sealed class Avatar : TemplatedControl {
        public Avatar() { }

        #region Properties

        public static readonly StyledProperty<Uri> ImageUriProperty =
            AvaloniaProperty.Register<Avatar, Uri>(nameof(ImageUri));

        public static readonly StyledProperty<string> InitialsProperty =
            AvaloniaProperty.Register<Avatar, string>(nameof(Initials));

        public Uri ImageUri {
            get => GetValue(ImageUriProperty);
            set => SetValue(ImageUriProperty, value);
        }

        public string Initials {
            get => GetValue(InitialsProperty);
            set => SetValue(InitialsProperty, value);
        }

        #endregion

        #region Template elements

        Ellipse ImageEllipse;

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            ImageEllipse = e.NameScope.Find<Ellipse>(nameof(ImageEllipse));
            SetImage();
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change) {
            base.OnPropertyChanged(change);

            if (change.Property == ImageUriProperty || change.Property == BoundsProperty) {
                SetImage();
            }
        }

        byte[] imageBytes;
        Uri currentImageUri;
        HttpClient httpClient = new HttpClient();

        private async void SetImage() {
            if (ImageEllipse == null) return;
            double size = Math.Min(Bounds.Width, Bounds.Height);
            ImageEllipse.Width = size;
            ImageEllipse.Height = size;

            if (ImageUri == null) {
                ImageEllipse.Fill = null;
                return;
            }
            if (currentImageUri == ImageUri) {
                Draw(); // событие SetImage может быть вызван из-за изменения размера
                return;
            }
            currentImageUri = ImageUri;

            // TODO: В будущем сделать публичное свойство ImageBytes,
            // чтобы приложение само скачивало картинку по-своему
            // (например, Laney через LNet) и отдавало Avatar-у
            // только саму картинку.
            try {
                var response = await VKUITheme.Current.WebRequestCallback?.Invoke(currentImageUri);
                imageBytes = await response.Content.ReadAsByteArrayAsync();
            } catch (Exception ex) {
                Debug.WriteLine($"Cannot get an image! 0x{ex.HResult.ToString("x8")}: {ex.Message}");
            }
            Draw();
        }

        private void Draw() {
            if (imageBytes == null) return;
            Stream stream = new MemoryStream(imageBytes);
            Bitmap bitmap = new Bitmap(stream);
            ImageEllipse.Fill = new ImageBrush(bitmap) { BitmapInterpolationMode = BitmapInterpolationMode.HighQuality };
        }
    }
}