using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ELOR.Laney.Core.Network;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ELOR.Laney.Extensions {
    public static class LNetExtensions {
        static Dictionary<string, byte[]> cachedImages = new Dictionary<string, byte[]>();
        const int cachesLimit = 500;

        public static async Task<byte[]> TryGetCachedImageAsync(Uri uri) {
            string url = uri.AbsoluteUri;
            if (!cachedImages.ContainsKey(url)) {
                var response = await LNet.GetAsync(uri);
                var bytes = await response.Content.ReadAsByteArrayAsync();
                if (cachedImages.Count == cachesLimit) cachedImages.Remove(cachedImages.First().Key);
                if (!cachedImages.ContainsKey(url)) cachedImages.Add(url, bytes); // надо
                return bytes;
            } else {
                return cachedImages[url];
            }
        }

        public static async Task<HttpResponseMessage> SendRequestToAPIViaLNetAsync(Uri uri, Dictionary<string, string> parameters, Dictionary<string, string> headers) {
            return await LNet.PostAsync(uri, parameters, headers);
        }

        public static async void SetUriSourceAsync(this Image image, Uri source) {
            try {
                // TODO: cache!
                HttpResponseMessage response = await LNet.GetAsync(source);
                var bytes = await response.Content.ReadAsByteArrayAsync();
                Stream stream = new MemoryStream(bytes);
                Bitmap bitmap = new Bitmap(stream);
                image.Source = bitmap;
            } catch (Exception ex) {
                Log.Error(ex, "SetImageSourceAsync error!");
            }
        }

        public static async void SetImageFillAsync(this Shape shape, Uri source) {
            try {
                // TODO: cache!
                HttpResponseMessage response = await LNet.GetAsync(source);
                var bytes = await response.Content.ReadAsByteArrayAsync();
                Stream stream = new MemoryStream(bytes);
                if (bytes.Length == 0) return;
                Bitmap bitmap = new Bitmap(stream);
                shape.Fill = new ImageBrush(bitmap) {
                    BitmapInterpolationMode = BitmapInterpolationMode.HighQuality,
                    AlignmentX = AlignmentX.Center,
                    AlignmentY = AlignmentY.Center,
                    Stretch = Stretch.UniformToFill
                };
            } catch (Exception ex) {
                Log.Error(ex, "SetImageFillAsync error!");
            }
        }

        public static async void SetImageBackgroundAsync(this Border control, Uri source) {
            if (control == null) return;
            try {
                // TODO: cache!
                HttpResponseMessage response = await LNet.GetAsync(source);
                var bytes = await response.Content.ReadAsByteArrayAsync();
                Stream stream = new MemoryStream(bytes);
                Bitmap bitmap = new Bitmap(stream);
                control.Background = new ImageBrush(bitmap) {
                    BitmapInterpolationMode = BitmapInterpolationMode.HighQuality,
                    AlignmentX = AlignmentX.Center,
                    AlignmentY = AlignmentY.Center,
                    Stretch = Stretch.UniformToFill
                };
            } catch (Exception ex) {
                Log.Error(ex, "SetImageBackgroundAsync error!");
            }
        }
    }
}