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
        static Dictionary<string, Bitmap> cachedImages = new Dictionary<string, Bitmap>();
        const int cachesLimit = 500;

        public static async Task<Bitmap> TryGetCachedBitmapAsync(Uri uri) {
            string url = uri.AbsoluteUri;
            if (!cachedImages.ContainsKey(url)) {
                var response = await LNet.GetAsync(uri);
                var bytes = await response.Content.ReadAsByteArrayAsync();
                Stream stream = new MemoryStream(bytes);
                if (bytes.Length == 0) throw new Exception("Image length is 0.");
                Bitmap bitmap = new Bitmap(stream);
                if (cachedImages.Count == cachesLimit) cachedImages.Remove(cachedImages.First().Key);
                if (!cachedImages.ContainsKey(url)) {
                    cachedImages.Add(url, bitmap);
                }
                return bitmap;
            } else {
                return cachedImages[url];
            }
        }

        public static async Task<HttpResponseMessage> SendRequestToAPIViaLNetAsync(Uri uri, Dictionary<string, string> parameters, Dictionary<string, string> headers) {
            return await LNet.PostAsync(uri, parameters, headers);
        }

        public static async Task<Bitmap> GetBitmapAsync(Uri source) {
            try {
                return await TryGetCachedBitmapAsync(source);
            } catch (Exception ex) {
                Log.Error(ex, "GetBitmapAsync error!");
                return null;
            }
        }

        public static async void SetUriSourceAsync(this Image image, Uri source) {
            try {
                image.Source = await TryGetCachedBitmapAsync(source);
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static async void SetUriSourceAsync(this ImageBrush imageBrush, Uri source) {
            try {
                imageBrush.Source = await TryGetCachedBitmapAsync(source);
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static async void SetImageFillAsync(this Shape shape, Uri source) {
            try {
                Bitmap bitmap = await TryGetCachedBitmapAsync(source);
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
                Bitmap bitmap = await TryGetCachedBitmapAsync(source);
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