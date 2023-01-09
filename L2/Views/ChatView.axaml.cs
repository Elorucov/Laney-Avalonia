using Avalonia;
using Avalonia.Controls;
using System;
using ELOR.Laney.ViewModels;
using System.Threading.Tasks;
using Serilog;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Diagnostics;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using System.Collections.Generic;
using ELOR.Laney.Controls;
using ELOR.Laney.Extensions;
using System.Linq;
using ELOR.VKAPILib.Objects;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        ChatViewModel Chat { get; set; }

        public ChatView() {
            InitializeComponent();
            BackButton.Click += (a, b) => BackButtonClick?.Invoke(this, null);
            DataContextChanged += ChatView_DataContextChanged;
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            PinnedMessageButton.Click += PinnedMessageButton_Click;

            itemsPresenter.GotFocus += ItemsPresenter_GotFocus;
            itemsPresenter.LostFocus += ItemsPresenter_LostFocus;
        }

        public event EventHandler BackButtonClick;
        public void ChangeBackButtonVisibility(bool isVisible) {
            BackButton.IsVisible = isVisible;
        }

        private void ChatView_DataContextChanged(object sender, EventArgs e) {
            currentFocused = null;

            if (Chat != null) {
                Chat.ScrollToMessageRequested -= ScrollToMessage;
                Chat.MessagesChunkLoaded -= TrySaveScroll;
            }

            Chat = DataContext as ChatViewModel;
            Chat.ScrollToMessageRequested += ScrollToMessage;
            Chat.MessagesChunkLoaded += TrySaveScroll;
        }

        double oldScrollViewerHeight = 0;
        bool needToSaveScroll = false;
        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            double trigger = 40;
            double h = scrollViewer.Extent.Height - scrollViewer.DesiredSize.Height;
            double y = scrollViewer.Offset.Y;
            dbgScrollInfo.Text = $"{Math.Round(y)}/{h}";
            if (h < trigger) return;

            if (needToSaveScroll && oldScrollViewerHeight != h) {
                double diff = h - oldScrollViewerHeight;
                double newpos = y + diff;

                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                try {
                    if (scrollViewer.CheckAccess()) {
                        while (scrollViewer.Offset.Y != newpos) {
                            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newpos);
                            await Task.Delay(32).ConfigureAwait(false);
                        }
                    }
                } catch (Exception ex) {
                    Log.Error(ex, "Unable to save scroll position after old messages are loaded!");
                }
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

                needToSaveScroll = false;
                oldScrollViewerHeight = h;
                return;
            }

            oldScrollViewerHeight = h;
            if (y < trigger) {
                Chat.LoadPreviousMessages();
            } else if (y > h - trigger) {
                Chat.LoadNextMessages();
            }
        }

        private void TrySaveScroll(object sender, bool e) {
            if (e) return; // нужно "сохранить" скролл при прогрузке предыдущих сообщений.
            needToSaveScroll = true;
        }

        private void ScrollToMessage(object sender, int messageId) {
            if (Chat.DisplayedMessages == null) return;
            itemsPresenter.ScrollIntoView(Chat.DisplayedMessages.IndexOf(Chat.DisplayedMessages?.GetById(messageId)));
        }

        private void PinnedMessageButton_Click(object sender, RoutedEventArgs e) {
            Chat.GoToMessage(Chat.PinnedMessage);
        }

        #region Messages list focus

        List<MessageBubble> messageBubbles = new List<MessageBubble>();
        MessageBubble currentFocused = null;

        private void ItemsPresenter_GotFocus(object sender, GotFocusEventArgs e) {
            var element = FocusManager.Instance?.Current;
            if (element != null) {
                string name = (element as Control).Name;
                Debug.WriteLine($"{element.GetType()}: {name}");

                itemsPresenter.FindVisualChildrenByType<MessageBubble>(messageBubbles);
                if (messageBubbles.Count > 0) {
                    if (currentFocused == null) currentFocused = messageBubbles.LastOrDefault();
                    FocusManager.Instance?.Focus(currentFocused, NavigationMethod.Directional, e.KeyModifiers);

                    itemsPresenter.KeyDown += ItemsPresenter_KeyDown;
                }
            }
        }

        private void ItemsPresenter_LostFocus(object sender, RoutedEventArgs e) {
            itemsPresenter.KeyDown -= ItemsPresenter_KeyDown;
            messageBubbles.Clear();
            // currentFocused = null;
        }

        private void ItemsPresenter_KeyDown(object sender, KeyEventArgs e) {
            if (currentFocused == null || messageBubbles.Count == 0) return;
            int index = messageBubbles.IndexOf(currentFocused);
            if (e.Key == Key.Up) {
                if (index > 0) {
                    currentFocused = messageBubbles.ElementAt(index - 1);
                    FocusManager.Instance?.Focus(currentFocused, NavigationMethod.Directional, e.KeyModifiers);
                    itemsPresenter.ScrollIntoView(Chat.DisplayedMessages.IndexOf(currentFocused.Message));
                }
            } else if (e.Key == Key.Down) {
                if (index < messageBubbles.Count - 1) {
                    currentFocused = messageBubbles.ElementAt(index + 1);
                    FocusManager.Instance?.Focus(currentFocused, NavigationMethod.Directional, e.KeyModifiers);
                    itemsPresenter.ScrollIntoView(Chat.DisplayedMessages.IndexOf(currentFocused.Message));
                }
            }
        }

        #endregion
    }
}