using Avalonia.Media.Imaging;
using ELOR.Laney.Controls;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ELOR.Laney.Extensions {
    public static class CommonExtensions {
        public static string HResultHEX(this Exception ex) {
            return $"0x{ex.HResult.ToString("x8")}";
        }

        public static bool HasFlag(this int num, int flag) {
            return (flag & num) != 0;
        }

        public static Avalonia.Size ToAvaloniaSize(this System.Drawing.Size size) {
            return new Avalonia.Size(size.Width, size.Height);
        }

        public static string ToFileSize(this ulong b) {
            if (b < 1024) {
                return $"{b} B";
            }
            if (b < 1048576) {
                return $"{Math.Round((double)b / 1024, 1)} Kb";
            }
            if (b < 1073741824) {
                return $"{Math.Round((double)b / 1048576, 1)} Mb";
            }
            return $"{Math.Round((double)b / 1073741824, 1)} Gb";
        }

        public static Dictionary<string, string> ParseQuery(this string query) {
            var result = new Dictionary<string, string>();
            var qparams = query.Split('&');
            foreach (var q in qparams) {
                var keyvalue = q.Split('=');
                result.Add(keyvalue[0], WebUtility.UrlDecode(keyvalue[1]));
            }
            return result;
        }

        public static PhotoSizes GetSizeAndUriForThumbnail(this IPreview preview, int maxWidth = 360) {
            maxWidth = Convert.ToInt32(maxWidth * App.Current.DPI);
            PhotoSizes ps = null;
            if (preview is Photo p) {
                foreach (PhotoSizes s in CollectionsMarshal.AsSpan(p.Sizes)) {
                    ps = s;
                    if (s.Width >= maxWidth) break; // да, выбирать будем первую фотку с шириной больше maxWidth
                }
            } else if (preview is Video v) {
                foreach (PhotoSizes s in CollectionsMarshal.AsSpan(v.Image)) {
                    ps = s;
                    if (s.Width >= maxWidth) break;
                }
            } else if (preview is Document d && d.Preview != null) {
                foreach (PhotoSizes s in CollectionsMarshal.AsSpan(d.Preview.Photo.Sizes)) {
                    ps = s;
                    if (s.Width >= maxWidth) break;
                }
            }
            if (ps != null) {
                Debug.WriteLine($"GetSizeAndUriForThumbnail: Requested {maxWidth}, found {ps.Width}");
            } else {
                Debug.WriteLine($"GetSizeAndUriForThumbnail: Requested {maxWidth} but not found!");
            }
            return ps;
        }

        public static StickerImage GetSizeAndUriForThumbnail(this Sticker sticker, double maxWidth = MessageBubble.STICKER_WIDTH) {
            maxWidth = maxWidth * App.Current.DPI;
            StickerImage ps = null;
            foreach (var s in CollectionsMarshal.AsSpan(sticker.Images)) {
                ps = s;
                if (s.Width >= maxWidth) break; // да, выбирать будем первую фотку с шириной больше maxWidth
            }
            if (ps != null) {
                Debug.WriteLine($"GetSizeAndUriForThumbnail (sticker): Requested {maxWidth}, found {ps.Width}");
            } else {
                Debug.WriteLine($"GetSizeAndUriForThumbnail (sticker): Requested {maxWidth} but not found!");
            }
            return ps;
        }

        public static async Task<Bitmap> TryGetBitmapFromStreamAsync(this Stream stream, int decodeWidth = 0) {
            try {
                if (decodeWidth > 0) {
                    decodeWidth = Convert.ToInt32(decodeWidth * App.Current.DPI);
                    return await Task.Run(() => Bitmap.DecodeToWidth(stream, decodeWidth));
                } else {
                    return new Bitmap(stream);
                }
            } catch (ArgumentNullException) {
                Log.Warning("TryGetBitmapFromStream: data in stream is not a bitmap!");
                return null;
            } catch (Exception ex) {
                Log.Error(ex, "TryGetBitmapFromStream error!");
                return null;
            }
        }

        // Возвращает имя с одной буквой фамилии (например, Данил Н.)
        public static string NameWithFirstLetterSurname(this User user) {
            string lastName = user.LastName;
            if (!String.IsNullOrEmpty(lastName)) lastName = lastName[0].ToString() + ".";
            return $"{user.FirstName} {lastName}".Trim();
        }
    }
}