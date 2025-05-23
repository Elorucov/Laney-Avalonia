﻿using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class AssetsManager {
        static readonly Dictionary<string, string> _loadedContents = new Dictionary<string, string>();

        public static async Task<string> GetStringFromUriAsync(Uri uri) {
            if (_loadedContents.ContainsKey(uri.AbsoluteUri)) return _loadedContents[uri.AbsoluteUri];

            Stream str = OpenAsset(uri);
            using StreamReader sr = new StreamReader(str, Encoding.UTF8);
            string content = await sr.ReadToEndAsync();
            _loadedContents.Add(uri.AbsoluteUri, content);
            return content;
        }

        public static Bitmap GetBitmapFromUri(Uri uri) {
            Stream stream = OpenAsset(uri);
            var b = new Bitmap(stream);
            return b;
        }

        public static WriteableBitmap GetWBitmapFromUri(Uri uri) {
            Stream stream = OpenAsset(uri);
            return WriteableBitmap.Decode(stream);
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

        // Required for encryption/decryption some critical data (for example access_token),
        // also needed for some integrity checks.
        public static byte[] BinaryPayload = {
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x20, 0x23, 0x23, 0x23, 0x23, 0x20, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23,
            0x20, 0x23, 0x23, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23,
            0x20, 0x23, 0x20, 0x23, 0x20, 0x20, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x23, 0x20, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x23, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23,
            0x20, 0x23, 0x23, 0x23, 0x23, 0x20, 0x20, 0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x23, 0x20, 0x20, 0x23, 0x23,
            0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23, 0x20, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x2A, 0x2A, 0x23, 0x2A, 0x2A, 0x23, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x2A, 0x20, 0x20, 0x2A, 0x20, 0x20, 0x2A, 0x23,
            0x20, 0x23, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x2A, 0x20, 0x20, 0x20, 0x20, 0x20, 0x2A, 0x23,
            0x20, 0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x20, 0x23, 0x2A, 0x20, 0x20, 0x20, 0x2A, 0x23, 0x23,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x23, 0x20, 0x23, 0x23, 0x2A, 0x20, 0x2A, 0x23, 0x23, 0x23,
            0x20, 0x23, 0x23, 0x23, 0x23, 0x23, 0x20, 0x20, 0x23, 0x23, 0x23, 0x2A, 0x23, 0x23, 0x23, 0x23,
            0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D,
            0x20, 0x4C, 0x41, 0x4E, 0x45, 0x59, 0x20, 0x42, 0x59, 0x20, 0x45, 0x4C, 0x4F, 0x52, 0x2E, 0x20,
            0x45, 0x6E, 0x63, 0x6F, 0x64, 0x69, 0x6E, 0x67, 0x20, 0x57, 0x69, 0x6E, 0x31, 0x32, 0x35, 0x31,
            0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D,
            0xCE, 0x2C, 0x20, 0xE2, 0xFB, 0x20, 0xE2, 0xF1, 0xB8, 0x2D, 0xF2, 0xE0, 0xEA, 0xE8, 0x20, 0x20,
            0xF1, 0xF3, 0xEC, 0xE5, 0xEB, 0xE8, 0x20, 0xED, 0xE0, 0xE9, 0xF2, 0xE8, 0x20, 0x20, 0x20, 0x20,
            0xEF, 0xE0, 0xF1, 0xF5, 0xE0, 0xEB, 0xEA, 0xF3, 0x20, 0xE2, 0x20, 0xFD, 0xF2, 0xEE, 0xEC, 0x20,
            0xE1, 0xE8, 0xED, 0xE0, 0xF0, 0xED, 0xE8, 0xEA, 0xE5, 0x21, 0x20, 0xD5, 0xEE, 0xF7, 0xF3, 0x20,
            0xE2, 0xFB, 0xF0, 0xE0, 0xE7, 0xE8, 0xF2, 0xFC, 0x20, 0xE1, 0xEB, 0xE0, 0xE3, 0xEE, 0x2D, 0x20,
            0xE4, 0xE0, 0xF0, 0xED, 0xEE, 0xF1, 0xF2, 0xFC, 0x20, 0xEC, 0xEE, 0xE8, 0xEC, 0x20, 0x20, 0x20,
            0xE4, 0xF0, 0xF3, 0xE7, 0xFC, 0xFF, 0xEC, 0x20, 0xE2, 0x20, 0xC2, 0xCA, 0x20, 0xE7, 0xE0, 0x20,
            0xEF, 0xEE, 0xE4, 0xE4, 0xE5, 0xF0, 0xE6, 0xEA, 0xF3, 0x20, 0xE8, 0x20, 0xEF, 0xEE, 0x2D, 0x20,
            0xEC, 0xEE, 0xF9, 0xFC, 0x20, 0xEC, 0xED, 0xE5, 0x20, 0xE2, 0x20, 0xF0, 0xE0, 0xE7, 0x2D, 0x20,
            0xF0, 0xE0, 0xE1, 0xEE, 0xF2, 0xEA, 0xE5, 0x2C, 0x20, 0xE0, 0x20, 0xF2, 0xE0, 0xEA, 0x2D, 0x20,
            0xE6, 0xE5, 0x20, 0xE2, 0xE0, 0xF1, 0x2C, 0x20, 0xEF, 0xEE, 0xEB, 0xFC, 0xE7, 0xEE, 0x2D, 0x20,
            0xE2, 0xE0, 0xF2, 0xE5, 0xEB, 0xFF, 0xEC, 0x20, 0x4C, 0x61, 0x6E, 0x65, 0x79, 0x2C, 0x20, 0x20,
            0xE7, 0xE0, 0x20, 0xF2, 0xEE, 0x2C, 0x20, 0xF7, 0xF2, 0xEE, 0x20, 0xE2, 0xFB, 0xE1, 0x2D, 0x20,
            0xF0, 0xE0, 0xEB, 0xE8, 0x20, 0xFD, 0xF2, 0xEE, 0x20, 0xE7, 0xE0, 0xEC, 0xE5, 0xF7, 0xE0, 0x2D,
            0xF2, 0xE5, 0xEB, 0xFC, 0xED, 0xEE, 0xE5, 0x20, 0xEF, 0xF0, 0xE8, 0xEB, 0xEE, 0xE6, 0xE5, 0x2D,
            0xED, 0xE8, 0xE5, 0x21, 0x20, 0x32, 0x20, 0xE3, 0xEE, 0xE4, 0xE0, 0x20, 0xF0, 0xE0, 0xE7, 0x2D,
            0xF0, 0xE0, 0xE1, 0xEE, 0xF2, 0xEA, 0xE8, 0x20, 0xF1, 0xEA, 0xE2, 0xEE, 0xE7, 0xFC, 0x20, 0x20,
            0xF2, 0xF0, 0xF3, 0xE4, 0xED, 0xEE, 0xF1, 0xF2, 0xE8, 0x20, 0xE8, 0x20, 0xEF, 0xF0, 0xE5, 0x2D,
            0xEE, 0xE4, 0xEE, 0xEB, 0xE5, 0xED, 0xE8, 0xFF, 0x2C, 0x20, 0xE8, 0x20, 0xED, 0xE0, 0xEA, 0xEE,
            0xED, 0xE5, 0xF6, 0x2D, 0xF2, 0xE0, 0xEA, 0xE8, 0x20, 0xEF, 0xE5, 0xF0, 0xE2, 0xFB, 0xE9, 0x20,
            0xEF, 0xF3, 0xE1, 0xEB, 0xE8, 0xF7, 0xED, 0xFB, 0xE9, 0x20, 0xF0, 0xE5, 0xEB, 0xE8, 0xE7, 0x20,
            0xE1, 0xE5, 0xF2, 0xE0, 0x2D, 0xE2, 0xE5, 0xF0, 0xF1, 0xE8, 0xE8, 0x21, 0x20, 0x20, 0x20, 0x20,
            0xCA, 0xF1, 0xF2, 0xE0, 0xF2, 0xE8, 0x2C, 0x20, 0xEF, 0xEE, 0xE4, 0xEE, 0xE1, 0xED, 0xE0, 0xFF,
            0xEF, 0xE0, 0xF1, 0xF5, 0xE0, 0xEB, 0xEA, 0xE0, 0x20, 0xE5, 0xF1, 0xF2, 0xFC, 0x20, 0xE8, 0x20,
            0xE2, 0x20, 0xF1, 0xF2, 0xE0, 0xF0, 0xEE, 0xE9, 0x20, 0x55, 0x57, 0x50, 0x2D, 0x20, 0x20, 0x20,
            0xE2, 0xE5, 0xF0, 0xF1, 0xE8, 0xE8, 0x2C, 0x20, 0xEF, 0xEE, 0xE8, 0xF9, 0xE8, 0xF2, 0xE5, 0x2E,
            0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D,
            0xC2, 0xF1, 0xB8, 0x2D, 0xE5, 0xF9, 0xB8, 0x20, 0xEB, 0xFE, 0xE1, 0xEB, 0xFE, 0x20, 0xE5, 0xB8,
            0x20, 0x20, 0x20, 0x20, 0xCD, 0xE0, 0xF1, 0xF2, 0xFE, 0x20, 0xCC, 0x2E, 0x20, 0x20, 0x20, 0x20,
            0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D,
            0xCA, 0xF1, 0xF2, 0xE0, 0xF2, 0xE8, 0x2C, 0x20, 0xE5, 0xF1, 0xEB, 0xE8, 0x20, 0xF5, 0xEE, 0x2D,
            0xF0, 0xEE, 0xF8, 0xEE, 0x20, 0xE7, 0xED, 0xE0, 0xE5, 0xF2, 0xE5, 0x20, 0x43, 0x23, 0x20, 0xE8,
            0x41, 0x76, 0x61, 0x6C, 0x6F, 0x6E, 0x69, 0x61, 0x2C, 0x20, 0xF3, 0x20, 0xE2, 0xE0, 0xF1, 0x20,
            0xE5, 0xF1, 0xF2, 0xFC, 0x20, 0x4D, 0x61, 0x63, 0x20, 0xE8, 0x20, 0xF5, 0xEE, 0xF2, 0xE8, 0x2D,
            0xF2, 0xE5, 0x20, 0xEF, 0xF0, 0xE8, 0xF1, 0xEE, 0xE5, 0xE4, 0xE8, 0xED, 0xE8, 0xF2, 0xFC, 0x2D,
            0xF1, 0xFF, 0x20, 0xEA, 0x20, 0xF0, 0xE0, 0xE7, 0xF0, 0xE0, 0xE1, 0xEE, 0xF2, 0xEA, 0xE5, 0x20,
            0x4C, 0x61, 0x6E, 0x65, 0x79, 0x2C, 0x20, 0xED, 0xE0, 0xEF, 0xE8, 0xF8, 0xE8, 0xF2, 0xE5, 0x20,
            0xF0, 0xE0, 0xE7, 0xF0, 0xE0, 0xE1, 0xEE, 0xF2, 0xF7, 0xE8, 0xEA, 0xF3, 0x3A, 0x20, 0x20, 0x20,
            0x76, 0x6B, 0x2E, 0x63, 0x6F, 0x6D, 0x2F, 0x65, 0x6C, 0x6F, 0x72, 0x75, 0x63, 0x6F, 0x76, 0x20,
            0x26, 0x20, 0x74, 0x2E, 0x6D, 0x65, 0x2F, 0x65, 0x6C, 0x6F, 0x72, 0x75, 0x63, 0x6F, 0x76, 0x2E,
            0xCE, 0xE1, 0xFF, 0xE7, 0xE0, 0xF2, 0xE5, 0xEB, 0xFC, 0xED, 0xEE, 0x20, 0xEE, 0xF2, 0x2D, 0x20,
            0xEF, 0xF0, 0xE0, 0xE2, 0xFC, 0xF2, 0xE5, 0x20, 0xF5, 0xFD, 0xF8, 0xF2, 0xE5, 0xE3, 0x20, 0x20,
            0x23, 0xEF, 0xE0, 0xF1, 0xF5, 0xE0, 0xEB, 0xEA, 0xE0, 0x4C, 0x32, 0x2E, 0x20, 0x20, 0x20, 0x20
        };
    }
}