using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VKUI;
using VKUI.Controls;

namespace ELOR.Laney.Extensions {
    public static class UIExtensions {
        public static void FixDialogWindows(this Window window, WindowTitleBar titleBar, Control content) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Grid.SetRow(content, 1);
                Grid.SetRowSpan(content, 1);
                titleBar.CanShowTitle = true;
                titleBar.CanMove = true;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                titleBar.IsVisible = false;
            }
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

        public static void RegisterThemeResource(this Control control, StyledProperty<IBrush> property, string resourceKey) {
            IBrush newBrush = App.GetResource<IBrush>(resourceKey);
            control.SetValue(property, newBrush);

            Action<VKUIScheme> themeChangedAction = new Action<VKUIScheme>((t) => {
                IBrush newBrush = App.GetResource<IBrush>(resourceKey);
                control.SetValue(property, newBrush);
            });
            App.Current.ThemeChanged.Add(themeChangedAction);
            control.DetachedFromLogicalTree += (a, b) => App.Current.ThemeChanged.Remove(themeChangedAction);
        }

        public static void FindLogicalChildrenByType<T>(this Control control, List<T> found) {
            var children = control.GetLogicalChildren();
            foreach (var child in children) {
                if (child is T el) found.Add(el);
                (child as Control).FindLogicalChildrenByType<T>(found);
            }
        }

        public static void FindVisualChildrenByType<T>(this Control control, List<T> found) {
            var children = control.GetVisualChildren();
            foreach (var child in children) {
                if (child is T el) found.Add(el);
                (child as Control).FindVisualChildrenByType<T>(found);
            }
        }

        public static T GetFirstVisualChildrenByType<T>(this Control control) {
            List<T> found = new List<T>();
            control.FindVisualChildrenByType(found);
            return found.Count > 0 ? found[0] : default;
        }

        #region ScrollViewer specific

        private const double SV_END_DISTANCE = 192;
        private static Dictionary<ScrollViewer, Action> registeredScrollViewers;

        public static void RegisterIncrementalLoadingEvent(this ScrollViewer scrollViewer, Action action) {
            if (registeredScrollViewers == null) registeredScrollViewers = new Dictionary<ScrollViewer, Action>();
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            registeredScrollViewers.Add(scrollViewer, action);
            scrollViewer.Unloaded += (a, b) => {
                registeredScrollViewers.Remove(scrollViewer);
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            };
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            ScrollViewer sv = sender as ScrollViewer;
            double h = sv.Extent.Height - sv.DesiredSize.Height;
            double y = sv.Offset.Y;

            if (y > h - SV_END_DISTANCE) {
                if (registeredScrollViewers.ContainsKey(sv)) {
                    Action act = registeredScrollViewers[sv];
                    act.Invoke();
                } else {
                    Log.Error("Cannot find scroll viewer in registeredScrollViewers!");
                    sv.ScrollChanged -= ScrollViewer_ScrollChanged;
                }
            }
        }

        #endregion
    }
}