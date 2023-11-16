using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Network;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Extensions {
    public static class LNetExtensions {
        public static void SetUriSourceAsync(this Image image, Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            try {
                new Thread(async () => {
                    Bitmap bitmap = await BitmapManager.GetBitmapAsync(source, decodeWidth, decodeHeight);
                    await Dispatcher.UIThread.InvokeAsync(() => image.Source = bitmap);
                }).Start();
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static void SetUriSourceAsync(this ImageBrush imageBrush, Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            try {
                new Thread(async () => {
                    Bitmap bitmap = await BitmapManager.GetBitmapAsync(source, decodeWidth, decodeHeight);
                    await Dispatcher.UIThread.InvokeAsync(() => imageBrush.Source = bitmap);
                }).Start();
            } catch (Exception ex) {
                Log.Error(ex, "SetUriSourceAsync error!");
            }
        }

        public static void SetImageAsync(this Avatar avatar, Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            try {
                avatar.Image = null;
                new Thread(async () => {
                    Bitmap bitmap = await BitmapManager.GetBitmapAsync(source, decodeWidth, decodeHeight);
                    await Dispatcher.UIThread.InvokeAsync(() => avatar.Image = bitmap);
                }).Start();
            } catch (Exception ex) {
                Log.Error(ex, "SetImageAsync error!");
            }
        }

        public static void SetImageFillAsync(this Shape shape, Uri source, double decodeWidth = 0, double decodeHeight = 0) {
            try {
                new Thread(async () => {
                    Bitmap bitmap = await BitmapManager.GetBitmapAsync(source, decodeWidth, decodeHeight);
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