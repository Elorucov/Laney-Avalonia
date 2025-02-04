using Avalonia.Media.Imaging;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Helpers;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class BitmapManager {
        // static ConcurrentDictionary<string, WriteableBitmap> cachedImages = new ConcurrentDictionary<string, WriteableBitmap>();
        static ConcurrentDictionary<string, Bitmap> cachedImages = new ConcurrentDictionary<string, Bitmap>();
        static ConcurrentDictionary<string, ManualResetEventSlim> nowLoading = new ConcurrentDictionary<string, ManualResetEventSlim>();
        const int cachesLimit = 500;

        public static async void ClearCachedImages() {
            long ram1 = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64;
            double ram1mb = (double)ram1 / 1048576;
            // RAMInfo.Text = $"{Math.Round(rammb, 1)} Mb";

            lock (cachedImages) {
                foreach (var ci in cachedImages) {
                    ci.Value.Dispose();
                }
            }
            cachedImages.Clear();
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Aggressive);

            await Task.Delay(250); // память может уменьшаться не сразу, и в лог попадёт неверное значение, если не прописать Delay.

            long ram2 = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64;
            double ram2mb = (double)ram2 / 1048576;

#if !MAC
            Log.Information($"ClearCachedImages: RAM usage before cleaning is {Math.Round(ram1mb, 1)} Mb, after cleaning is {Math.Round(ram2mb, 1)} Mb.");
#endif
        }

        private static async Task<Bitmap> TryGetCachedBitmapAsync(Uri uri, double decodeWidth = 0, double decodeHeight = 0) {
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
                    return needResize ? await GetResizedBitmap(lbitmap, decodeWidth, decodeHeight) : lbitmap;
                } else {
                    // WriteableBitmap bitmap = null;
                    Bitmap bitmap = null;
                    if (nowLoading.ContainsKey(key)) {
                        var lmres = nowLoading[key];
                        if (Settings.BitmapManagerLogs) Log.Information($"TryGetCachedBitmapAsync: The bitmap with key \"{key}\" is currently downloading by another instance. Waiting...");
                        await Task.Factory.StartNew(lmres.Wait).ConfigureAwait(true);
                        return needResize ? await GetResizedBitmap(cachedImages[key], decodeWidth, decodeHeight) : cachedImages[key];
                    }
                    using ManualResetEventSlim mres = new ManualResetEventSlim();
                    bool isAdded = nowLoading.TryAdd(key, mres);
                    if (!isAdded) Log.Warning($"TryGetCachedBitmapAsync: cannot add MRES \"{key}\"!");

                    using var response = await LNet.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    using Stream stream = new MemoryStream(bytes);
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
                    return needResize ? await GetResizedBitmap(bitmap, decodeWidth, decodeHeight) : bitmap;
                }
            } else {
                return needResize ? await GetResizedBitmap(cachedImages[key], decodeWidth, decodeHeight) : cachedImages[key];
            }
        }

        private static async Task<Bitmap> GetResizedBitmap(Bitmap bitmap, double dw, double dh) {
            var iw = bitmap == null ? 0 : bitmap.Size.Width;
            var ih = bitmap == null ? 0 : bitmap.Size.Height;

            if (dw == 0 || dh == 0 || iw == 0 || ih == 0) {
                if (Settings.BitmapManagerLogs) Log.Information($"GetResizedBitmap: bitmap size is {iw}x{ih}, container size is {dw}x{dh}. One of these parameters is zero, returning original bitmap.");
                return bitmap;
            }

            double rw = 0, rh = 0;
            ElorMath.Resize(iw, ih, dw, dh, out rw, out rh);

            if (Settings.BitmapManagerLogs) Log.Information($"GetResizedBitmap: bitmap size is {iw}x{ih}, container size is {dw}x{dh}, resized to {rw}x{rh}.");

            // https://github.com/AvaloniaUI/Avalonia/issues/8444
            Bitmap rbitmap = null;
            await Task.Factory.StartNew(() => {
                rbitmap = bitmap.CreateScaledBitmap(new Avalonia.PixelSize((int)rw, (int)rh));
            }).WaitAsync(new CancellationTokenSource().Token);
            return rbitmap;
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