using Avalonia;
using Avalonia.Controls;
using System;
using ELOR.Laney.ViewModels;
using System.Threading.Tasks;
using Serilog;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Diagnostics;
using System.Collections.Generic;
using ELOR.Laney.Controls;
using ELOR.Laney.Extensions;
using System.Linq;
using ELOR.Laney.ViewModels.Controls;
using ELOR.Laney.Helpers;
using Avalonia.Controls.Presenters;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        ChatViewModel Chat { get; set; }
        ScrollViewer MessagesListScrollViewer;
        ItemsPresenter MessagesListItemsPresenter;

        public ChatView() {
            InitializeComponent();

            MessagesList.Loaded += (a, b) => {
                MessagesListScrollViewer = MessagesList.Scroll as ScrollViewer;
                MessagesListScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                MessagesListScrollViewer.GotFocus += MessagesList_GotFocus;

                MessagesListItemsPresenter = MessagesList.GetFirstVisualChildrenByType<ItemsPresenter>();
            };


            Unloaded += (a, b) => MessagesListScrollViewer.KeyDown -= MessagesList_KeyDown;
            BackButton.Click += (a, b) => BackButtonClick?.Invoke(this, null);
            DataContextChanged += ChatView_DataContextChanged;
            PinnedMessageButton.Click += PinnedMessageButton_Click;
        }

        public event EventHandler BackButtonClick;
        public void ChangeBackButtonVisibility(bool isVisible) {
            BackButton.IsVisible = isVisible;
        }

        private void ChatView_DataContextChanged(object sender, EventArgs e) {
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
            double h = MessagesListScrollViewer.Extent.Height - MessagesListScrollViewer.DesiredSize.Height;
            double y = MessagesListScrollViewer.Offset.Y;
            dbgScrollInfo.Text = $"{Math.Round(y)}/{h}";
            if (h < trigger) return;

            if (needToSaveScroll && oldScrollViewerHeight != h) {
                double diff = h - oldScrollViewerHeight;
                double newpos = y + diff;

                MessagesListScrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                try {
                    if (MessagesListScrollViewer.CheckAccess()) {
                        while (MessagesListScrollViewer.Offset.Y != newpos) {
                            MessagesListScrollViewer.Offset = new Vector(MessagesListScrollViewer.Offset.X, newpos);
                            await Task.Delay(32).ConfigureAwait(false);
                        }
                    }
                } catch (Exception ex) {
                    Log.Error(ex, "Unable to save scroll position after old messages are loaded!");
                }
                MessagesListScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

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
            MessagesList.ScrollIntoView(Chat.DisplayedMessages.IndexOf(Chat.DisplayedMessages?.GetById(messageId)));
        }

        private void PinnedMessageButton_Click(object sender, RoutedEventArgs e) {
            Chat.GoToMessage(Chat.PinnedMessage);
        }

        private void ChatView_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (MessagesListItemsPresenter != null)
                MessagesListItemsPresenter.Width = e.NewSize.Width; // костыль, который фиксит очень странный баг.

            if (e.NewSize.Width >= 512) {
                MessagesCommandsRoot.Classes.Clear();
            } else {
                if (!MessagesCommandsRoot.Classes.Contains("CompactMsgCmd")) MessagesCommandsRoot.Classes.Add("CompactMsgCmd");
            }
        }

        #region Messages list focus

        // При фокусе на список сообщений фокусируемся на последнее сообщение
        // (в будущем надо на последнее видимое).
        private void MessagesList_GotFocus(object sender, GotFocusEventArgs e) {
            List<ListBoxItem> messageUIs = new List<ListBoxItem>();
            var element = FocusManager.Instance?.Current;
            if (element != null && e.NavigationMethod == NavigationMethod.Tab) {
                Debug.WriteLine($"Focused on {FocusManager.Instance.Current}");
                string name = (element as Control).Name;
                Debug.WriteLine($"{element.GetType()}: {name}");

                MessagesList.FindVisualChildrenByType(messageUIs);
                if (messageUIs.Count > 0) {
                    FocusManager.Instance?.Focus(messageUIs.LastOrDefault(), NavigationMethod.Directional, e.KeyModifiers);
                }
                MessagesListScrollViewer.KeyDown += MessagesList_KeyDown;
            }
        }

        // А ещё родной ListBox не умеет скроллить список при навигации кнопками вверх/вниз.
        private async void MessagesList_KeyDown(object sender, KeyEventArgs e) {
            if (FocusManager.Instance == null) return;
            if (e.Key == Key.Up || e.Key == Key.Down) {
                await Task.Delay(10); // надо, чтобы в FocusManager.Instance.Current был актуальный контрол
                Debug.WriteLine($"Focused on {FocusManager.Instance.Current}");
                MessageViewModel msg = (FocusManager.Instance.Current as Control).DataContext as MessageViewModel;
                if (msg != null) MessagesList.ScrollIntoView(Chat.DisplayedMessages.IndexOf(msg));
            }
        }

        #endregion

        #region Context menu for message

        private void ChatViewItem_ContextRequested(object sender, ContextRequestedEventArgs e) {
            ChatViewItem cvi = sender as ChatViewItem;
            MessageViewModel message = cvi?.DataContext as MessageViewModel;
            if (message == null) return;

            // При ПКМ на сообщение оно выделяется, перед этим очищая Chat.SelectedMessages.
            // Пока что будем ещё раз чистить Chat.SelectedMessages. ¯\_(ツ)_/¯
            Chat.SelectedMessages.Clear();

            ContextMenuHelper.ShowForMessage(message, Chat, cvi);
        }

        #endregion
    }
}