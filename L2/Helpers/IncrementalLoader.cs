using Avalonia.Controls;
using System;

namespace ELOR.Laney.Helpers {
    public class IncrementalLoader {
        public const double SV_END_DISTANCE = 192;

        ScrollViewer scrollViewer;
        Action action;
        double triggerOffset;

        public IncrementalLoader(ScrollViewer sv, Action act, double trigOff = SV_END_DISTANCE) {
            scrollViewer = sv;
            action = act;
            triggerOffset = trigOff;
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            scrollViewer.SizeChanged += ScrollViewer_SizeChanged;
            scrollViewer.Loaded += ScrollViewer_Loaded;
            scrollViewer.Unloaded += ScrollViewer_Unloaded;
        }

        private void ScrollViewer_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            new IncrementalLoader(scrollViewer, action, triggerOffset);
        }

        private void ScrollViewer_Unloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            scrollViewer.SizeChanged -= ScrollViewer_SizeChanged;
            scrollViewer.Unloaded -= ScrollViewer_Unloaded;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            TryTriggerAction();
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) {
            TryTriggerAction();
        }

        private void TryTriggerAction() {
            double h = scrollViewer.Extent.Height - scrollViewer.DesiredSize.Height;
            double y = scrollViewer.Offset.Y;
            if (h < triggerOffset) return;

            if (y > h - triggerOffset) action.Invoke();
        }
    }
}
