using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ELOR.Laney.Extensions {
    public static class UIExtensions {
        public static void FixDecoration(this Window window) {
            window.ExtendClientAreaToDecorationsHint = true;
            window.ExtendClientAreaChromeHints = 
                App.Platform == OSPlatform.Linux ? ExtendClientAreaChromeHints.NoChrome : ExtendClientAreaChromeHints.SystemChrome;
            window.ExtendClientAreaTitleBarHeightHint = 1;
        }

        public static string GetInitials(this string name, bool oneLetter = false) {
            if (String.IsNullOrWhiteSpace(name)) return String.Empty;
            if (oneLetter) return name[0].ToString().ToUpper();
            string[] words = name.Split(" ");
            if (words.Length == 1) return words[0][0].ToString().ToUpper();
            return $"{words[0][0]}{words[1][0]}".ToUpper();
        }

        private static List<LinearGradientBrush> gradients = new List<LinearGradientBrush> {
            // BuildGradientBrush(Color.Parse("#ff8880"), Color.Parse("#e62e6b")),
            BuildGradientBrush(Color.Parse("#ff7583"), Color.Parse("#e52e40")),
            BuildGradientBrush(Color.Parse("#ffbf80"), Color.Parse("#e66b2e")),
            BuildGradientBrush(Color.Parse("#ffd54f"), Color.Parse("#e7a902")),
            BuildGradientBrush(Color.Parse("#6cd97e"), Color.Parse("#12b212")),
            BuildGradientBrush(Color.Parse("#7df1fa"), Color.Parse("#2bb4d6")),
            BuildGradientBrush(Color.Parse("#d3a6ff"), Color.Parse("#8f3fe0")),
        };

        private static LinearGradientBrush BuildGradientBrush(Color start, Color end) {
            LinearGradientBrush lgb = new LinearGradientBrush { 
                StartPoint = RelativePoint.TopLeft,
                EndPoint = RelativePoint.BottomRight
            };
            lgb.GradientStops.Add(new GradientStop { Offset = 0, Color = start });
            lgb.GradientStops.Add(new GradientStop { Offset = 1, Color = end });
            return lgb;
        }

        public static LinearGradientBrush GetGradient(this int id) {
            if (id < 0) id = id * -1;
            int index = id % 6;
            return gradients[index];
        }
    }
}