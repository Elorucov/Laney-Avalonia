using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using ELOR.Laney.Core.Localization;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class AssetsManager {
         public static async Task<Bitmap> GetBitmapFromUri(Uri uri, int decodeWidth = 0) {
            Stream stream = OpenAsset(uri);
            return decodeWidth > 0
                ? await Task.Run(() => Bitmap.DecodeToWidth(stream, decodeWidth, BitmapInterpolationMode.HighQuality))
                : new Bitmap(stream);
        }

        public static Stream OpenAsset(Uri uri) {
            return AssetLoader.Open(uri);
        }

        public static string GetThemeDependentTrayIcon() {
            var cv = Application.Current.PlatformSettings.GetColorValues();

            string theme = cv.ThemeVariant == PlatformThemeVariant.Light ? "b" : "w";
            string s = $"avares://laney/Assets/Logo/Tray/t32m{theme}.png";
            return s;
        }

        public static void Check(out string result) {
            var a = AssetsManager.OpenAsset(new Uri("avares://laney/Assets/Audio/bb2.mp3"));
            byte[] b = new byte[a.Length];
            a.Read(b);

            byte[] e = null;
            byte[] f = null;
            byte[] ee = null;
            byte[] ff = null;
            var enc = Encoding.ASCII;
            if (Localizer.Instance["lang"] == "ru") {
                e = new byte[14] { b[147], b[476], b[753], b[494], b[447], b[55], b[329], b[250], b[1156], b[163], b[1170], b[607], b[1224], b[934] };
                f = new byte[14] { b[73], b[121], b[154], b[146], b[551], b[350], b[556], b[277], b[54], b[1089], b[1436], b[352], b[348], b[345] };
                enc = Encoding.GetEncoding("windows-1251");
            } else {
                e = new byte[13] { b[791], b[136], b[258], b[165], b[510], b[308], b[329], b[395], b[239], b[680], b[106], b[788], b[937] };
                f = new byte[13] { b[831], b[773], b[138], b[231], b[126], b[80], b[556], b[601], b[343], b[653], b[111], b[533], b[270] };
            }
            ee = Encoding.Convert(enc, Encoding.UTF8, e);
            ff = Encoding.Convert(enc, Encoding.UTF8, f);
            string g = Encoding.UTF8.GetString(ee);
            string h = Encoding.UTF8.GetString(ff);
            result = g + h;
        }
    }
}