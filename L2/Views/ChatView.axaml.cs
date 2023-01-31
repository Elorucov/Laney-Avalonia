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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using ELOR.Laney.Core;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        ChatViewModel Chat { get; set; }
        ScrollViewer MessagesListScrollViewer;

        public ChatView() {
            InitializeComponent();

            MessagesList.Loaded += (a, b) => {
                MessagesListScrollViewer = MessagesList.Scroll as ScrollViewer;
                MessagesListScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                new ListBoxAutoScrollHelper(MessagesList) {
                    ScrollToLastItemAfterTabFocus = true
                };
                new ItemsPresenterWidthFixer(MessagesList);
            };

            BackButton.Click += (a, b) => BackButtonClick?.Invoke(this, null);
            DataContextChanged += ChatView_DataContextChanged;
            PinnedMessageButton.Click += PinnedMessageButton_Click;

            DebugOverlay.IsVisible = Settings.ShowDebugCounters;
            Settings.SettingChanged += Settings_SettingChanged;
        }

        private void Settings_SettingChanged(string key, object value) {
            switch (key) {
                case Settings.DEBUG_COUNTERS_CHAT:
                    DebugOverlay.IsVisible = (bool)value;
                    break;
            }
        }

        public event EventHandler BackButtonClick;
        public void ChangeBackButtonVisibility(bool isVisible) {
            BackButton.IsVisible = isVisible;
        }

        private void ChatView_DataContextChanged(object sender, EventArgs e) {
            if (Chat != null) {
                Chat.ScrollToMessageRequested -= ScrollToMessage;
                Chat.MessagesChunkLoaded -= TrySaveScroll;
                Chat.ReceivedMessages.CollectionChanged -= ReceivedMessages_CollectionChanged;
            }

            Chat = DataContext as ChatViewModel;
            Chat.ScrollToMessageRequested += ScrollToMessage;
            Chat.MessagesChunkLoaded += TrySaveScroll;
            Chat.ReceivedMessages.CollectionChanged += ReceivedMessages_CollectionChanged;
        }

        private async void ReceivedMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add && autoScrollToLastMessage) {
                ObservableCollection<MessageViewModel> received = sender as ObservableCollection<MessageViewModel>;
                await Task.Delay(10); // ибо id-ы разные почему-то...
                int lastReceived = received.LastOrDefault().Id;
                int lastDisplayed = Chat.DisplayedMessages.Last.Id;
                if (lastReceived == lastDisplayed) {
                    MessagesList.ScrollIntoView(Chat.DisplayedMessages.Count - 1);

                    // TODO: починить момент, когда после изменения сообщения
                    // после loading-cостояния надо ещё раз скроллить до конца.
                }
            }
        }

        double oldScrollViewerHeight = 0;
        bool needToSaveScroll = false;
        bool autoScrollToLastMessage = false;
        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            double trigger = 40;
            double h = MessagesListScrollViewer.Extent.Height - MessagesListScrollViewer.DesiredSize.Height;
            double y = MessagesListScrollViewer.Offset.Y;
            dbgScrV.Text = $"{h}";
            dbgScrO.Text = $"{Math.Round(y)}";
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
                autoScrollToLastMessage = false;
            } else if (y > h - trigger) {
                if (Chat.DisplayedMessages.Last == null || Chat.LastMessage == null) return;
                bool needLoadNextMessages = Chat.DisplayedMessages.Last.Id != Chat.LastMessage.Id;
                if (needLoadNextMessages) {
                    Chat.LoadNextMessages();
                } else {
                    autoScrollToLastMessage = true;
                }
            } else {
                autoScrollToLastMessage = false;
            }
            dbgScrAuto.Text = $"{autoScrollToLastMessage}";
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
            if (e.NewSize.Width >= 512) {
                MessagesCommandsRoot.Classes.Clear();
            } else {
                if (!MessagesCommandsRoot.Classes.Contains("CompactMsgCmd")) MessagesCommandsRoot.Classes.Add("CompactMsgCmd");
            }
        }

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