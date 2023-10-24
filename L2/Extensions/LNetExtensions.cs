using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Network;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Extensions {
    public static class LNetExtensions {
        static Dictionary<string, WriteableBitmap> cachedImages = new Dictionary<string, WriteableBitmap>();
        static Dictionary<string, ManualResetEventSlim> nowLoading = new Dictionary<string, ManualResetEventSlim>();
        const int cachesLimit = 100;

        public static void ClearCachedImages() {
            lock (cachedImages) {
                foreach (var ci in cachedImages) {
                    ci.Value.Dispose();
                }
            }
            cachedImages.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

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
                WriteableBitmap bitmap = null;
                if (uri.Scheme == "avares") {
                    bitmap = AssetsManager.GetWBitmapFromUri(uri, decodeWidth);
                    if (cachedImages.Count == cachesLimit) {
                        var first = cachedImages.First();
                        first.Value.Dispose();
                        cachedImages.Remove(first.Key);
                    }
                    if (!cachedImages.ContainsKey(key)) {
                        cachedImages.Add(key, bitmap);
                    }
                    return bitmap;
                } else {
                    if (nowLoading.ContainsKey(key)) {
                        var lmres = nowLoading[key];
                        Log.Information($"TryGetCachedBitmapAsync: The bitmap with key {key} is currently downloading by another instance. Waiting...");
                        await Task.Factory.StartNew(lmres.Wait).ConfigureAwait(true);
                        Log.Information($"TryGetCachedBitmapAsync: The bitmap with key {key} is downloaded.");
                        return cachedImages[key];
                    }
                    ManualResetEventSlim mres = new ManualResetEventSlim();
                    nowLoading.Add(key, mres);
                    var response = await LNet.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    Stream stream = new MemoryStream(bytes);
                    if (bytes.Length == 0) throw new Exception("Image length is 0!");

                    bitmap = decodeWidth > 0
                        ? await Task.Run(() => WriteableBitmap.DecodeToWidth(stream, decodeWidth, BitmapInterpolationMode.MediumQuality))
                        : WriteableBitmap.Decode(stream);

                    if (cachedImages.Count == cachesLimit) {
                        var first = cachedImages.First();
                        first.Value.Dispose();
                        cachedImages.Remove(first.Key);
                    }
                    if (!cachedImages.ContainsKey(key)) {
                        cachedImages.Add(key, bitmap);
                    }
                    await stream.FlushAsync();
                    nowLoading.Remove(key);
                    mres.Set();
                    // mres.Dispose();
                    return bitmap;
                }
            } else {
                return cachedImages[key];
            }
        }

        public static async Task<Bitmap> GetBitmapAsync(Uri source, int decodeWidth = 0) {
            if (source == null) return null; // Бывают всякие аватарки и фотки без url)))

            string key = decodeWidth > 0
                ? Convert.ToBase64String(Encoding.UTF8.GetBytes(source.AbsoluteUri)) + "||" + decodeWidth
                : source.AbsoluteUri;

            try {
                return await TryGetCachedBitmapAsync(source, decodeWidth);
            } catch (Exception ex) {
                if (nowLoading.ContainsKey(key)) {
                    nowLoading[key].Set();
                    nowLoading[key].Dispose();
                    nowLoading.Remove(key);
                }
                Log.Error(ex, $"GetBitmapAsync error! Source: {source.AbsoluteUri},(dw: {decodeWidth})");
                return null;
            }
        }

        public static void SetUriSourceAsync(this Image image, Uri source, int decodeWidth = 0) {
            try {
                new Thread(async () => {
                    Bitmap bitmap = await GetBitmapAsync(source, decodeWidth);
                    await Dispatcher.UIThread.InvokeAsync(() => image.Source = bitmap);
                }).Start();
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static void SetUriSourceAsync(this ImageBrush imageBrush, Uri source, int decodeWidth = 0) {
            try {
                new Thread(async () => {
                    Bitmap bitmap = await GetBitmapAsync(source, decodeWidth);
                    await Dispatcher.UIThread.InvokeAsync(() => imageBrush.Source = bitmap);
                }).Start();
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static void SetImageAsync(this Avatar avatar, Uri source, int decodeWidth = 0) {
            try {
                avatar.Image = null;
                new Thread(async () => {
                    Bitmap bitmap = await GetBitmapAsync(source, decodeWidth);
                    await Dispatcher.UIThread.InvokeAsync(() => avatar.Image = bitmap);
                }).Start();
            } catch (Exception ex) {
                Log.Error(ex, "SetImageAsync error!");
            }
        }

        public static void SetImageFillAsync(this Shape shape, Uri source, int decodeWidth = 0) {
            try {
                new Thread(async () => {
                    Bitmap bitmap = await GetBitmapAsync(source, decodeWidth);
                    await Dispatcher.UIThread.InvokeAsync(() => {
                        shape.Fill = new ImageBrush(bitmap) {
                            AlignmentX = AlignmentX.Center,
                            AlignmentY = AlignmentY.Center,
                            Stretch = Stretch.UniformToFill
                        };
                    });
                }).Start();
            } catch (Exception ex) {
                Log.Error(ex, "SetImageFillAsync error!");
            }
        }

        public static async Task<bool> SetImageBackgroundAsync(this Control control, Uri source, int decodeWidth = 0) {
            if (control == null) return false;
            try {
                Bitmap bitmap = await GetBitmapAsync(source, decodeWidth);
                var brush = new ImageBrush(bitmap) {
                    AlignmentX = AlignmentX.Center,
                    AlignmentY = AlignmentY.Center,
                    Stretch = Stretch.UniformToFill
                };
                if (control is ContentControl cc) {
                    cc.Background = brush;
                } else if (control is Border b) {
                    b.Background = brush;
                } else {
                    return false;
                }
                return true;
            } catch (Exception ex) {
                Log.Error(ex, "SetImageBackgroundAsync error!");
                return false;
            }
        }

        public static async Task<HttpResponseMessage> SendRequestToAPIViaLNetAsync(Uri uri, Dictionary<string, string> parameters, Dictionary<string, string> headers) {
            return await LNet.PostAsync(uri, parameters, headers);
        }
    }
}