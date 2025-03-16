using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace ELOR.Laney.Helpers {
    public class ListBoxItemClickHandler<T> {
        ListBox listBox;
        Action<T> onItemClickEvent;

        public ListBoxItemClickHandler(ListBox target, Action<T> onItemClick) {
            listBox = target;
            onItemClickEvent = onItemClick;

            listBox.AddHandler(InputElement.PointerReleasedEvent, PointerUp, RoutingStrategies.Tunnel);
            listBox.GotFocus += ListBox_GotFocus;
            listBox.LostFocus += ListBox_LostFocus;

            listBox.Unloaded += (a, b) => {
                listBox.RemoveHandler(InputElement.PointerReleasedEvent, PointerUp);
                listBox.GotFocus -= ListBox_GotFocus;
                listBox.LostFocus -= ListBox_LostFocus;
            };
        }

        private void PointerUp(object sender, PointerReleasedEventArgs e) {
            var obj = (T)listBox.SelectedItem;
            if (obj != null) onItemClickEvent?.Invoke(obj);
        }

        private void ListBox_GotFocus(object sender, GotFocusEventArgs e) {
            listBox.KeyUp += ListBox_KeyUp;
        }

        private void ListBox_LostFocus(object sender, RoutedEventArgs e) {
            listBox.KeyUp -= ListBox_KeyUp;
        }

        private void ListBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter) return;
            var el = TopLevel.GetTopLevel(listBox).FocusManager.GetFocusedElement();
            if (el is Control c && c.DataContext != null && c.DataContext is T obj) onItemClickEvent?.Invoke(obj);
        }
    }
}
