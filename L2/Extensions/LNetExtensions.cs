using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Network;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Extensions {
    public static class LNetExtensions {
        static Dictionary<string, Bitmap> cachedImages = new Dictionary<string, Bitmap>();
        const int cachesLimit = 500;

        public static async Task<Bitmap> TryGetCachedBitmapAsync(Uri uri, int decodeWidth = 0) {
            if (uri == null) return null;

            string url = uri.AbsoluteUri;
            if (decodeWidth > 0) { // DPI aware
                decodeWidth = Convert.ToInt32(decodeWidth * App.Current.DPI);
            }

            string key = decodeWidth > 0
                ? Convert.ToBase64String(Encoding.UTF8.GetBytes(url)) + "||" + decodeWidth
                : url;

            if (!cachedImages.ContainsKey(key)) {
                if (uri.Scheme == "avares") {
                    Bitmap bitmap = await AssetsManager.GetBitmapFromUri(uri, decodeWidth);
                    if (cachedImages.Count == cachesLimit) cachedImages.Remove(cachedImages.First().Key);
                    if (!cachedImages.ContainsKey(key)) {
                        cachedImages.Add(key, bitmap);
                    }
                    return bitmap;
                } else {
                    var response = await LNet.GetAsync(uri);
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    Stream stream = new MemoryStream(bytes);
                    if (bytes.Length == 0) throw new Exception("Image length is 0!");

                    Bitmap bitmap = decodeWidth > 0
                        ? await Task.Run(() => Bitmap.DecodeToWidth(stream, decodeWidth, BitmapInterpolationMode.MediumQuality))
                        : new Bitmap(stream);

                    if (cachedImages.Count == cachesLimit) cachedImages.Remove(cachedImages.First().Key);
                    if (!cachedImages.ContainsKey(key)) {
                        cachedImages.Add(key, bitmap);
                    }
                    await stream.FlushAsync();
                    return bitmap;
                }
            } else {
                return cachedImages[key];
            }
        }

        public static async Task<HttpResponseMessage> SendRequestToAPIViaLNetAsync(Uri uri, Dictionary<string, string> parameters, Dictionary<string, string> headers) {
            return await LNet.PostAsync(uri, parameters, headers);
        }

        public static async Task<Bitmap> GetBitmapAsync(Uri source, int decodeWidth = 0) {
            try {
                return await TryGetCachedBitmapAsync(source, decodeWidth);
            } catch (Exception ex) {
                Log.Error(ex, "GetBitmapAsync error!");
                return null;
            }
        }

        public static async void SetUriSourceAsync(this Image image, Uri source, int decodeWidth = 0) {
            try {
                image.Source = await TryGetCachedBitmapAsync(source, decodeWidth);
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static async void SetUriSourceAsync(this ImageBrush imageBrush, Uri source, int decodeWidth = 0) {
            try {
                imageBrush.Source = await TryGetCachedBitmapAsync(source, decodeWidth);
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static async void SetImageAsync(this Avatar avatar, Uri source, int decodeWidth = 0) {
            try {
                avatar.Image = null;
                avatar.Image = await TryGetCachedBitmapAsync(source, decodeWidth);
            } catch (Exception ex) {
                Log.Error(ex, "SetImageAsync error!");
            }
        }

        public static async void SetImageFillAsync(this Shape shape, Uri source, int decodeWidth = 0) {
            try {
                Bitmap bitmap = await TryGetCachedBitmapAsync(source, decodeWidth);
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

        public static async Task<bool> SetImageBackgroundAsync(this Border control, Uri source, int decodeWidth = 0) {
            if (control == null) return false;
            try {
                Bitmap bitmap = await TryGetCachedBitmapAsync(source, decodeWidth);
                control.Background = new ImageBrush(bitmap) {
                    BitmapInterpolationMode = BitmapInterpolationMode.HighQuality,
                    AlignmentX = AlignmentX.Center,
                    AlignmentY = AlignmentY.Center,
                    Stretch = Stretch.UniformToFill
                };
                return true;
            } catch (Exception ex) {
                Log.Error(ex, "SetImageBackgroundAsync error!");
                return false;
            }
        }
    }
}