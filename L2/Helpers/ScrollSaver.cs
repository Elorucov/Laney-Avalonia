using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ELOR.Laney.Helpers {
    public interface IContainsScrollSaverList {
        long Id { get; }
        // bool ShouldTriggerEventInsteadScrollDown { get; }
    }

    public class ScrollSaverExtendChangedEventArgs {
        public bool Handled { get; set; }
    }

    // Класс, который сохраняет и восстанавливает позицию скролла при смене DataContext
    public class ScrollSaver {
        ScrollViewer scroll;
        long Id { get { return scroll.DataContext is IContainsScrollSaverList ? (scroll.DataContext as IContainsScrollSaverList).Id : 0; } }
        string SId { get { return Id == 0 ? "N/A" : Id.ToString(); } }

        Dictionary<long, double> savedExtend = new Dictionary<long, double>();
        Dictionary<long, double> savedOffset = new Dictionary<long, double>();

        public delegate void ExtendChangedHandler(ScrollSaver sender, ScrollSaverExtendChangedEventArgs args);
        public event ExtendChangedHandler ExtendChanged;

        public ScrollSaver(ScrollViewer sv) {
            scroll = sv;
            scroll.PropertyChanged += Scroll_PropertyChanged;
            // scroll.EffectiveViewportChanged += Scroll_EffectiveViewportChanged;
            scroll.ScrollChanged += Scroll_ScrollChanged;
            scroll.DataContextChanged += Scroll_DataContextChanged;
            scroll.Unloaded += Scroll_Unloaded;
        }

        long oldId = 0;
        private void Scroll_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
            if (e.Property.Name == nameof(ScrollViewer.Extent)) {
#if DEBUG
                Debug.WriteLine($"ExtendChanged: vh={scroll.Viewport.Height}, eh={scroll.Extent.Height}, vo={scroll.Offset.Y}, Id {SId}");
#endif
                if (Id != oldId) { // Признак того, что DataContext сменился, а значит, надо вернуться к сохранённой позиции.
                    double newOffset = scroll.ScrollBarMaximum.Y;
                    if (savedOffset.ContainsKey(Id)) {
                        newOffset = savedOffset[Id];
                        scroll.Offset = new Vector(scroll.Offset.X, newOffset);
                    } else {
                        ScrollSaverExtendChangedEventArgs args = new ScrollSaverExtendChangedEventArgs();
                        ExtendChanged?.Invoke(this, args);
                        if (!args.Handled) scroll.Offset = new Vector(scroll.Offset.X, newOffset);
                    }

                    oldId = Id;
                }
            }
        }

        //private void Scroll_EffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e) {
        //    var v = e.EffectiveViewport;
        //    Debug.WriteLine($"EffectiveViewportChanged: top={v.Top}, bottom={v.Bottom}, y={v.Y}, vh={scroll.Viewport.Height}, vo={scroll.Offset.Y}, Id {SId}");
        //}


        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            var od = e.OffsetDelta;
            var vd = e.ViewportDelta;
#if DEBUG
            Debug.WriteLine($"ScrollChanged: offsetD={od.Y}, viewportD={vd.Y}, vh={scroll.Viewport.Height}, vo={scroll.Offset.Y}, Id {SId}");
#endif
            if (oldId == Id) {
                SaveOffset(oldId, scroll.Offset.Y);
            }
        }

        private void Scroll_DataContextChanged(object sender, EventArgs e) {
#if DEBUG
            Debug.WriteLine($"DataContextChanged: vh={scroll.Viewport.Height}, eh={scroll.Extent.Height}, vo={scroll.Offset.Y}, Id {SId}");
#endif
            if (oldId != Id) {  // Происходит смена DataContext, надо сохранить позицию.
                SaveExtend(oldId, scroll.Extent.Height);
                SaveOffset(oldId, scroll.Offset.Y);
            }
        }

        private void Scroll_Unloaded(object sender, RoutedEventArgs e) {
            scroll.PropertyChanged -= Scroll_PropertyChanged;
            // scroll.EffectiveViewportChanged -= Scroll_EffectiveViewportChanged;
            scroll.ScrollChanged -= Scroll_ScrollChanged;
            scroll.DataContextChanged -= Scroll_DataContextChanged;
            scroll.Unloaded -= Scroll_Unloaded;
        }

        private void SaveExtend(long id, double value) {
            if (savedExtend.ContainsKey(id)) {
                savedExtend[id] = value;
            } else {
                savedExtend.Add(id, value);
            }
        }

        private void SaveOffset(long id, double value) {
            if (savedOffset.ContainsKey(id)) {
                savedOffset[id] = value;
            } else {
                savedOffset.Add(id, value);
            }
        }
    }
}