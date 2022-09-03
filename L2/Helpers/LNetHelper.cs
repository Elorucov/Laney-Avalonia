using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ELOR.Laney.Core.Network;
using Serilog;
using System;
using System.IO;
using System.Net.Http;

namespace ELOR.Laney.Helpers {
    public static class LNetHelper {
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
    }
}