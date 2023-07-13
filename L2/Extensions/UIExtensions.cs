using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using ELOR.Laney.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        public static LinearGradientBrush GetGradient(this long id) {
            if (id.IsGroup()) id = id * -1;
            long index = id % 6;
            return gradients[(int)index];
        }

        public static void RegisterThemeResource(this Control control, StyledProperty<IBrush> property, string resourceKey) {
            IBrush newBrush = App.GetResource<IBrush>(resourceKey);
            control.SetValue(property, newBrush);

            Action<ThemeVariant> themeChangedAction = new Action<ThemeVariant>((t) => {
                IBrush newBrush = App.GetResource<IBrush>(resourceKey);
                control.SetValue(property, newBrush);
            });
            App.Current.ThemeChangedActions.Add(themeChangedAction);
            control.DetachedFromLogicalTree += (a, b) => App.Current.ThemeChangedActions.Remove(themeChangedAction);
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

        public static T GetDataContextAt<T>(this Control control, Point position) {
            if (control == null) return default;
            Control el = control.GetVisualAt(position) as Control;
            if (el != null && el.DataContext != null && el.DataContext is T target) return target;
            return default;
        }

        #region ScrollViewer specific

        public static void RegisterIncrementalLoadingEvent(this ScrollViewer scrollViewer, Action action, double triggerOffset = IncrementalLoader.SV_END_DISTANCE) {
            new IncrementalLoader(scrollViewer, action, triggerOffset);
        }

        #endregion
    }
}