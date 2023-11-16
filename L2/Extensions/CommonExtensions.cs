using Avalonia;
using Avalonia.Media.Imaging;
using ELOR.Laney.Controls;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public static Size GetOriginalSize(this IPreview preview) {
            if (preview is Photo p) {
                var msp = p.MaximalSizedPhoto;
                return new Size(msp.Width, msp.Height);
            } else if (preview is Video v) {
                return new Size(v.Width, v.Height);
            } else if (preview is Document d && d.Preview?.Photo != null && d.Preview.Photo.Sizes.Count > 0) {
                var s = d.Preview.Photo.Sizes.LastOrDefault();
                new Size(s.Width, s.Height);
            }
            return new Size(0, 0);
        }

        public static PhotoSizes GetSizeAndUriForThumbnail(this IPreview preview, double maxWidth, double maxHeight) {
            double mw = maxWidth * App.Current.DPI;
            double mh = maxHeight * App.Current.DPI;
            PhotoSizes ps = new PhotoSizes {
                Url = null
            };
            if (preview is Photo p) {
                if (p.Sizes == null || p.Sizes.Count == 0) {
                    Log.Warning($"{p} have no sizes and links!");
                    return ps;
                }
                foreach (PhotoSizes s in CollectionsMarshal.AsSpan(p.Sizes)) {
                    ps = s;
                    if (ElorMath.IsLargeOrEqualThanMax(s.Width, s.Height, maxWidth, maxHeight)) break;
                }
            } else if (preview is Video v) {
                if (v.Image == null || v.Image.Count == 0) {
                    Log.Warning($"Preview for {v} have no sizes and links!");
                    return ps;
                }
                var vprevs = v.Image.Where(vi => vi.WithPadding == 0).ToList();
                if (vprevs.Count == 0) vprevs = v.Image;
                foreach (PhotoSizes s in CollectionsMarshal.AsSpan(vprevs)) {
                    ps = s;
                    if (ElorMath.IsLargeOrEqualThanMax(s.Width, s.Height, maxWidth, maxHeight)) break;
                }
            } else if (preview is Document d && d.Preview != null) {
                if (d.Preview.Photo.Sizes == null || d.Preview.Photo.Sizes.Count == 0) {
                    Log.Warning($"Preview for {d} have no sizes and links!");
                    return ps;
                }
                foreach (PhotoSizes s in CollectionsMarshal.AsSpan(d.Preview.Photo.Sizes)) {
                    // у файлов-фото с разрешением, например, 720x1280 (cкриншоты на телефонах)
                    // у размеров s и m ширина больше высоты!
                    if (s.Type == "s" || s.Type == "m") continue; 
                    ps = s;
                    if (ElorMath.IsLargeOrEqualThanMax(s.Width, s.Height, maxWidth, maxHeight)) break;
                }
            }
            if (ps != null) {
                Debug.WriteLine($"GetSizeAndUriForThumbnail: Requested {maxWidth}x{maxHeight}, found {ps.Width}x{ps.Height}");
            } else {
                Debug.WriteLine($"GetSizeAndUriForThumbnail: Requested {maxWidth}x{maxHeight}, but nothing found!");
            }
            return ps;
        }

        public static StickerImage GetSizeAndUriForThumbnail(this Sticker sticker, double maxWidth = MessageBubble.STICKER_WIDTH) {
            maxWidth = maxWidth * App.Current.DPI;
            if (sticker.IsPartial) {
                int[] sizes = new int[] { 64, 128, 256, 352, 512 };
                int size = 0;
                foreach (int s in sizes) {
                    size = s;
                    if (s >= maxWidth) break;
                }
                return new StickerImage {
                    Width = size,
                    Height = size,
                    Url = $"https://vk.com/sticker/1-{sticker.StickerId}-{size}b" // TODO: theme!
                };
            } else {
                StickerImage ps = null;
                foreach (var s in CollectionsMarshal.AsSpan(sticker.ImagesWithBackground)) {
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

        public static bool IsUser(this long id) {
            return (id >= 1 && id < 1.9e9) || (id >= 200e9 && id < 100e10);
        }

        public static bool IsGroup(this long id) {
            return id > -1e9 && id <= -1;
        }

        public static bool IsChat(this long id) {
            return id > 2e9 && id < 2e9 + 1e8;
        }
    }
}