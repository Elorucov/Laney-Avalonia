using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using ELOR.Laney.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ELOR.Laney.Helpers {
    public class ListBoxCustomVirtualization {
        ListBox listBox;
        ScrollViewer scroll { get => listBox.Scroll as ScrollViewer; }

        double oldo = 0; // old offset
        double oldh = 0; // old height

        ICustomVirtalizedListItem FirstVisible { get => GetElement(new Point(64, scroll.Offset.Y)); }
        ICustomVirtalizedListItem LastVisible { get => GetElement(new Point(64, scroll.Offset.Y + scroll.DesiredSize.Height)); }

        public ListBoxCustomVirtualization(ListBox target) {
            listBox = target;
            scroll.ScrollChanged += Scroll_ScrollChanged;
            scroll.SizeChanged += Scroll_SizeChanged;
            listBox.Unloaded += (a, b) => {
                scroll.ScrollChanged -= Scroll_ScrollChanged;
                scroll.SizeChanged -= Scroll_SizeChanged;
                currentlyDisplaying = null;
            };
        }

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            ShowOrHideItems();
            oldo = scroll.Offset.Y;
        }

        private void Scroll_SizeChanged(object sender, SizeChangedEventArgs e) {
            ShowOrHideItems();
            oldh = scroll.Extent.Height;
        }

        public void EnforceProcessVirtualization() {
            ShowOrHideItems();
        }

        List<ICustomVirtalizedListItem> currentlyDisplaying = new List<ICustomVirtalizedListItem>();

        private void ShowOrHideItems() {
            if (oldh == 0 && scroll.Extent.Height > 0 && scroll.Offset.Y == 0) return; // нужно.
            var fv = FirstVisible; 
            var lv = LastVisible;
            if (fv == null && lv == null) return;

            List<ICustomVirtalizedListItem> newCD = new List<ICustomVirtalizedListItem>();

            int fi = -1; 
            int li = -1;
            if (fv != null) fi = listBox.IndexFromContainer((fv as Control).Parent as Control);
            if (lv != null) li = listBox.IndexFromContainer((lv as Control).Parent as Control);
            int c = listBox.Items.Count - 1;

            if (fi == -1) fi = Math.Max(0, li - 8);
            if (li == -1) li = Math.Min(c, fi + 8);

            if (fi > 0) fi = fi - 1;
            if (li < c) li = li + 1;

            if (li > fi) {
                for (int i = fi; i <= li; i++) {
                    ListBoxItem lbi = (ListBoxItem)listBox.ContainerFromIndex(i);
                    var item = (ICustomVirtalizedListItem)lbi.Presenter.Child;
                    if (item != null) {
                        Debug.WriteLine($"Index {i} added to new.");
                        newCD.Add(item);
                    }
                }
            }

            foreach (var oldi in CollectionsMarshal.AsSpan(currentlyDisplaying)) {
                if (!newCD.Contains(oldi)) oldi.OnDisappearedFromScreen();
            }

            foreach (var newi in CollectionsMarshal.AsSpan(newCD)) {
                newi.OnAppearedOnScreen();
            }

            currentlyDisplaying = newCD;
        }

        private double GetOffsetTop() {
            double val = scroll.Offset.Y - scroll.DesiredSize.Height;
            return Math.Max(scroll.Offset.Y, scroll.DesiredSize.Height);
        }

        private ICustomVirtalizedListItem GetElement(Point point) {
            StackPanel panel = listBox.ItemsPanelRoot as StackPanel;
            Control el = panel.GetVisualAt(point) as Control;
            if (el == null) return null;
            ICustomVirtalizedListItem target = null;
            while (target == null) {
                target = el as ICustomVirtalizedListItem;
                if (target == null) {
                    el = (Control)el.Parent;
                    if (el == null || el is ListBoxItem) break;
                } else {
                    break;
                }
            }
            
            return target;
        }
    }
}
