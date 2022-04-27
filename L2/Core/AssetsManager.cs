using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;

namespace ELOR.Laney.Core {
    public static class AssetsManager {
        static IAssetLoader assets;

        public static Bitmap GetBitmapFromUri(Uri uri) {
            Stream stream = OpenAsset(uri);
            return new Bitmap(stream);
        }

        private static Stream OpenAsset(Uri uri) {
            if (assets == null) assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            return assets.Open(uri);
        }
    }
}