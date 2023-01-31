using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ELOR.Laney.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ELOR.Laney.Helpers {

    // Да, есть свойство AutoScrollToSelectedItem у ListBox,
    // но это работает только, как вы поняли, при выборе,
    // а не при фокусировки во время навигации кнопками
    // при SelectionMode="Toggle"
    public class ListBoxAutoScrollHelper : IDisposable {
        ListBox listBox;
        ScrollViewer scroll { get { return listBox.Scroll as ScrollViewer; } }

        public bool ScrollToLastItemAfterTabFocus { get; set; }

        public ListBoxAutoScrollHelper(ListBox target) {
            listBox = target;

            scroll.GotFocus += Scroll_GotFocus;
            scroll.LostFocus += Scroll_LostFocus;
            listBox.Unloaded += (a, b) => Dispose();
        }

        private void Scroll_GotFocus(object sender, GotFocusEventArgs e) {
            Debug.WriteLine($"Focused to ListBox's ScrollViewer");
            if (ScrollToLastItemAfterTabFocus) {
                var element = FocusManager.Instance?.Current;
                if (element != null && e.NavigationMethod == NavigationMethod.Tab) {
                    Debug.WriteLine($"Focused on {FocusManager.Instance.Current}");
                    List<ListBoxItem> lvis = new List<ListBoxItem>();
                    listBox.FindVisualChildrenByType(lvis);
                    if (lvis.Count > 0) {
                        FocusManager.Instance?.Focus(lvis.LastOrDefault(), NavigationMethod.Directional, e.KeyModifiers);
                    }
                }
            }
            scroll.KeyDown += Scroll_KeyDown;
        }

        private void Scroll_LostFocus(object sender, RoutedEventArgs e) {
            scroll.KeyDown -= Scroll_KeyDown;
        }

        private async void Scroll_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Up || e.Key == Key.Down) {
                await Task.Delay(10); // надо, чтобы в FocusManager.Instance.Current был актуальный контрол
                if (FocusManager.Instance == null) return;
                Debug.WriteLine($"Focused on {FocusManager.Instance.Current}");
                object itemDC = (FocusManager.Instance.Current as Control).DataContext;
                if (itemDC != null) {
                    var enumerator = listBox.Items.GetEnumerator();
                    int index = 0;
                    enumerator.Reset();
                    while (enumerator.MoveNext()) {
                        if (itemDC == enumerator.Current) {
                            break;
                        }
                        index++;
                    }
                    Debug.WriteLine($"Index for focused element in ListBox: {index}");
                    if (index > 0) {
                        listBox.ScrollIntoView(index);
                    }
                }
            }
        }

        bool disposed = false;
        public void Dispose() {
            if (disposed) return;
            disposed = true;
            scroll.GotFocus -= Scroll_GotFocus;
            scroll.LostFocus -= Scroll_LostFocus;
        }
    }
}