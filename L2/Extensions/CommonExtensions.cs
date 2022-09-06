using System;

namespace ELOR.Laney.Extensions {
    public static class CommonExtensions {
        public static string HResultHEX(this Exception ex) {
            return $"0x{ex.HResult.ToString("x8")}";
        }

        public static Avalonia.Size ToAvaloniaSize(this System.Drawing.Size size) {
            return new Avalonia.Size(size.Width, size.Height);
        }
    }
}