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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Extensions {
    public static class LNetExtensions {
        public static async void SetUriSourceAsync(this Image image, Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            try {
                Bitmap bitmap = await BitmapManager.GetBitmapAsync(source, decodeWidth, decodeHeight);
                image.Source = bitmap;
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static async void SetUriSourceAsync(this ImageBrush imageBrush, Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            try {
                Bitmap bitmap = await BitmapManager.GetBitmapAsync(source, decodeWidth, decodeHeight);
                imageBrush.Source = bitmap;
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static async void SetImageAsync(this Avatar avatar, Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            try {
                avatar.Image = null;
                Bitmap bitmap = await BitmapManager.GetBitmapAsync(source, decodeWidth, decodeHeight);
                avatar.Image = bitmap;
            } catch (Exception ex) {
                Log.Error(ex, "SetImageAsync error!");
            }
        }

        public static async void SetImageFillAsync(this Shape shape, Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            try {
                Bitmap bitmap = await BitmapManager.GetBitmapAsync(source, decodeWidth, decodeHeight);
                shape.Fill = new ImageBrush(bitmap) {
                    AlignmentX = AlignmentX.Center,
                    AlignmentY = AlignmentY.Center,
                    Stretch = Stretch.UniformToFill
                };
            } catch (Exception ex) {
                Log.Error(ex, "SetImageFillAsync error!");
            }
        }

        public static async Task<bool> SetImageBackgroundAsync(this Control control, Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            if (control == null) return false;
            try {
                Bitmap bitmap = await BitmapManager.GetBitmapAsync(source, decodeWidth, decodeHeight);
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