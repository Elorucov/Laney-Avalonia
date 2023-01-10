using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using VKUI.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Linq;
using VKUI.Utils;

namespace VKUI.Popups {
    public sealed class ActionSheet : FlyoutBase {
        public ActionSheet() {
            _items = new List<ActionSheetItem>();
        }

        private List<ActionSheetItem> _items;

        public List<ActionSheetItem> Items {
            get => _items;
        }

        public bool CloseAfterClick { get; set; } = true;

        StackPanel itemsPanel;
        private List<Button> itemsButtons = new List<Button>();
        protected override Control CreatePresenter() {
            itemsPanel = new StackPanel { 
                Margin = new Thickness(0, 4, 0, 4),
            };

            foreach (ActionSheetItem item in _items) {
                if (item.Before == null && item.Header == null) { // Экстравагатным образом добавляем сепаратор
                    Rectangle separator = new Rectangle();
                    separator.Classes.Add("ActionSheetSeparator");
                    itemsPanel.Children.Add(separator);
                    continue;
                }
                item.Click += Item_Click;
                itemsPanel.Children.Add(item);
            }

            return new VKUIFlyoutPresenter {
                Content = itemsPanel
            };
        }

        protected override void OnOpened() {
            base.OnOpened();
            itemsPanel.FindVisualChildrenByType(itemsButtons);
            FocusManager.Instance?.Focus(itemsButtons.FirstOrDefault());
            itemsPanel.KeyDown += Items_KeyDown;
        }

        protected override void OnClosed() {
            base.OnClosed();
            itemsPanel.KeyDown -= Items_KeyDown;
        }

        private void Item_Click(object? sender, RoutedEventArgs e) {
            if (CloseAfterClick) Hide();
        }

        private void Items_KeyDown(object sender, KeyEventArgs e) {
            Debug.WriteLine($"Action sheet navigation: {e.Key}");
            if (FocusManager.Instance?.Current != null && FocusManager.Instance.Current is Button current) {
                int index = itemsButtons.IndexOf(current);
                if (index < 0) return;
                if (e.Key == Key.Up && index > 0) {
                    FocusManager.Instance.Focus(itemsButtons.ElementAt(index - 1), NavigationMethod.Directional);
                } else if (e.Key == Key.Down && index < itemsButtons.Count - 1) {
                    FocusManager.Instance.Focus(itemsButtons.ElementAt(index + 1), NavigationMethod.Directional);
                }
            }
        }
    }
}