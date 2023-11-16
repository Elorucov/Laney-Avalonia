using Avalonia.Media.Imaging;
using ELOR.Laney.Core.Network;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public class BitmapManager {
        // static ConcurrentDictionary<string, WriteableBitmap> cachedImages = new ConcurrentDictionary<string, WriteableBitmap>();
        static ConcurrentDictionary<string, Bitmap> cachedImages = new ConcurrentDictionary<string, Bitmap>();
        static ConcurrentDictionary<string, ManualResetEventSlim> nowLoading = new ConcurrentDictionary<string, ManualResetEventSlim>();
        const int cachesLimit = 500;

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

        public static async Task<Bitmap> TryGetCachedBitmapAsync(Uri uri, double decodeWidth = 0, double decodeHeight = 0) {
            if (uri == null) return null;

            string url = uri.AbsoluteUri;
            if (decodeWidth > 0) { // DPI aware
                decodeWidth = Convert.ToInt32(decodeWidth * App.Current.DPI);
            }
            if (decodeHeight > 0) { // DPI aware
                decodeHeight = Convert.ToInt32(decodeHeight * App.Current.DPI);
            }

            bool needResize = decodeWidth > 0 && decodeHeight > 0;
            string key = url;

            if (!cachedImages.ContainsKey(key)) {
                if (uri.Scheme == "avares") {
                    var lbitmap = AssetsManager.GetBitmapFromUri(uri);
                    return needResize ? GetResizedBitmap(lbitmap, decodeWidth, decodeHeight) : lbitmap;
                } else {
                    // WriteableBitmap bitmap = null;
                    Bitmap bitmap = null;
                    if (nowLoading.ContainsKey(key)) {
                        var lmres = nowLoading[key];
                        Log.Information($"TryGetCachedBitmapAsync: The bitmap with key \"{key}\" is currently downloading by another instance. Waiting...");
                        await Task.Factory.StartNew(lmres.Wait).ConfigureAwait(true);
                        return needResize ? GetResizedBitmap(cachedImages[key], decodeWidth, decodeHeight) : cachedImages[key];
                    }
                    ManualResetEventSlim mres = new ManualResetEventSlim();
                    bool isAdded = nowLoading.TryAdd(key, mres);
                    if (!isAdded) Log.Warning($"TryGetCachedBitmapAsync: cannot add MRES \"{key}\"!");
                    var response = await LNet.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    Stream stream = new MemoryStream(bytes);
                    if (bytes.Length == 0) throw new Exception("Image length is 0!");

                    // bitmap = WriteableBitmap.Decode(stream);
                    bitmap = new Bitmap(stream);

                    if (cachedImages.Count == cachesLimit) {
                        var first = cachedImages.First();
                        // WriteableBitmap outwb = null;
                        Bitmap outwb = null;
                        bool isRemoved = cachedImages.Remove(first.Key, out outwb);
                        if (!isRemoved) Log.Warning($"TryGetCachedBitmapAsync: cannot remove bitmap \"{first.Key}\"!");
                        first.Value.Dispose();
                    }
                    if (!cachedImages.ContainsKey(key)) {
                        bool isAdded2 = cachedImages.TryAdd(key, bitmap);
                        if (!isAdded2) Log.Warning($"TryGetCachedBitmapAsync: cannot add bitmap \"{key}\"!");
                    }
                    await stream.FlushAsync();
                    ManualResetEventSlim outmres = null;
                    bool isRemoved2 = nowLoading.Remove(key, out outmres);
                    if (!isRemoved2) Log.Warning($"TryGetCachedBitmapAsync: cannot remove MRES \"{key}\"!");
                    mres.Set();
                    // mres.Dispose();
                    return needResize ? GetResizedBitmap(bitmap, decodeWidth, decodeHeight) : bitmap;
                }
            } else {
                return needResize ? GetResizedBitmap(cachedImages[key], decodeWidth, decodeHeight) : cachedImages[key];
            }
        }

        private static Bitmap GetResizedBitmap(Bitmap bitmap, double dw, double dh) {
            var iw = bitmap.Size.Width;
            var ih = bitmap.Size.Height;

            if (dw == 0 || dh == 0 || iw == 0 || ih == 0) {
                Log.Verbose($"GetResizedBitmap: bitmap size is {iw}x{ih}, container size is {dw}x{dh}. One of these parameters is zero, returning original bitmap");
                return bitmap;
            }


            double sw = 0, sh = 0;
            sw = dw / iw;
            sh = dh / ih;
            double zf = Math.Max(sw, sh); // zoom

            double rw = Math.Ceiling(iw * zf);
            double rh = Math.Ceiling(ih * zf);

            Log.Verbose($"GetResizedBitmap: bitmap size is {iw}x{ih}, container size is {dw}x{dh}, resized to {rw}x{rh}.");

            // github.com/AvaloniaUI/Avalonia/issues/8444
            return bitmap.CreateScaledBitmap(new Avalonia.PixelSize((int)rw, (int)rh));
        }

        public static async Task<Bitmap> GetBitmapAsync(Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            if (source == null) return null; // Бывают всякие аватарки и фотки без url)))
            string key = source.AbsoluteUri;

            try {
                return await TryGetCachedBitmapAsync(source, decodeWidth, decodeHeight);
            } catch (Exception ex) {
                if (nowLoading.ContainsKey(key)) {
                    nowLoading[key].Set();
                    nowLoading[key].Dispose();
                    ManualResetEventSlim outmres = null;
                    bool isRemoved2 = nowLoading.Remove(key, out outmres);
                    if (!isRemoved2) Log.Warning($"GetBitmapAsync: cannot remove MRES for \"{key}\"!");
                }
                Log.Error(ex, $"GetBitmapAsync error! Source: {key}, ({decodeWidth}x{decodeHeight})");
                return null;
            }
        }
    }
}