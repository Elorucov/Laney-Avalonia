using Avalonia;
using ELOR.VKAPILib.Objects;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

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

        public static PhotoSizes GetSizeAndUriForThumbnail(this IPreview preview) {
            int maxWidth = 360; // TODO: учитывать системное масштабирование, но оно прописано у окна,
                                // коих может быть несколько в разных мониторах с разным масштабированием.
            PhotoSizes ps = null;
            if (preview is Photo p) {
                foreach (PhotoSizes s in CollectionsMarshal.AsSpan(p.Sizes)) {
                    ps = s;
                    if (s.Width > maxWidth) break; // да, выбирать будем первую фотку с шириной больше maxWidth
                }
            } else if (preview is Video v) {
                foreach (PhotoSizes s in CollectionsMarshal.AsSpan(v.Image)) {
                    ps = s;
                    if (s.Width > maxWidth) break;
                }
            } else if (preview is Document d && d.Preview != null) {
                foreach (PhotoSizes s in CollectionsMarshal.AsSpan(d.Preview.Photo.Sizes)) {
                    ps = s;
                    if (s.Width > maxWidth) break;
                }
            }
            return ps;
        }
    }
}