using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using System.Collections.Generic;

namespace VKUI.Utils {
    internal static class UIExtensions {
        internal static void FindLogicalChildrenByType<T>(this Control control, List<T> found) {
            var children = control.GetLogicalChildren();
            foreach (var child in children) {
                if (child is T el) found.Add(el);
                (child as Control).FindLogicalChildrenByType<T>(found);
            }
        }

        internal static void FindVisualChildrenByType<T>(this Control control, List<T> found) {
            var children = control.GetVisualChildren();
            foreach (var child in children) {
                if (child is T el) found.Add(el);
                (child as Control).FindVisualChildrenByType<T>(found);
            }
        }
    }
}