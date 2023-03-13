using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels;
using ELOR.Laney.ViewModels.Controls;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        ChatViewModel Chat { get; set; }
        ScrollViewer MessagesListScrollViewer;                                          // height, offset
        Dictionary<int, Tuple<double, double>> ScrollPositions = new Dictionary<int, Tuple<double, double>>();
        bool canSaveScrollPosinion = false;

        MessageViewModel FirstVisible { get => MessagesListScrollViewer?.GetDataContextAt<MessageViewModel>(new Point(64, 0)); }
        MessageViewModel LastVisible { get => MessagesListScrollViewer?.GetDataContextAt<MessageViewModel>(new Point(64, MessagesListScrollViewer.DesiredSize.Height - 5)); }

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

        private async void ChatView_DataContextChanged(object sender, EventArgs e) {
            canSaveScrollPosinion = false;
            if (Chat != null) {
                Chat.ScrollToMessageRequested -= ScrollToMessage;
                Chat.MessagesChunkLoaded -= TrySaveScroll;
                Chat.ReceivedMessages.CollectionChanged -= ReceivedMessages_CollectionChanged;
            }

            Chat = DataContext as ChatViewModel;
            Chat.ScrollToMessageRequested += ScrollToMessage;
            Chat.MessagesChunkLoaded += TrySaveScroll;
            Chat.ReceivedMessages.CollectionChanged += ReceivedMessages_CollectionChanged;

            if (ScrollPositions.ContainsKey(Chat.PeerId)) {
                var scrollPos = ScrollPositions[Chat.PeerId];
                Log.Information($"Trying to restore scroll position for chat {Chat.PeerId}. H: {scrollPos.Item1}, Y: {scrollPos.Item2}");

                while (true) {
                    await Task.Yield();
                    double h = MessagesListScrollViewer.Extent.Height - MessagesListScrollViewer.DesiredSize.Height;
                    if (h == scrollPos.Item1) break;
                }

                ForceScroll(scrollPos.Item2);
            }

            canSaveScrollPosinion = true;
            CheckFirstAbdLastDisplayedMessages();
        }

        private async void ReceivedMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add && autoScrollToLastMessage && Chat.DisplayedMessages != null && Chat.DisplayedMessages.Count > 0) {
                ObservableCollection<MessageViewModel> received = sender as ObservableCollection<MessageViewModel>;
                await Task.Delay(10); // ибо id-ы разные почему-то...
                int lastReceivedId = received.LastOrDefault()?.Id ?? 0;
                var lastDisplayed = Chat.DisplayedMessages.LastOrDefault();
                int lastDisplayedId = lastDisplayed?.Id ?? 0;
                if (lastReceivedId == lastDisplayedId && lastDisplayedId > 0) {
                    // MessagesList.ScrollIntoView(Chat.DisplayedMessages.Count - 1);
                    double h = MessagesListScrollViewer.Extent.Height - MessagesListScrollViewer.DesiredSize.Height;
                    ForceScroll(h);

                    // После изменения сообщения
                    // после loading-cостояния надо ещё раз скроллить до конца.
                    Log.Information($"Need to scroll to last message again. Message id: {lastDisplayedId}");
                    if (lastDisplayed.State == MessageVMState.Loading) lastDisplayed.PropertyChanged += LastDisplayedMsgPropertyChanged;
                }
            }
        }

        private async void LastDisplayedMsgPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(MessageViewModel.State)) {
                MessageViewModel msg = sender as MessageViewModel;
                msg.PropertyChanged -= LastDisplayedMsgPropertyChanged;

                await Task.Delay(10); // нужно, ибо без этого высота скролла старая.

                int lastReceivedId = Chat.ReceivedMessages.LastOrDefault()?.Id ?? 0;
                if (msg.Id == lastReceivedId) {
                    // Принудительно скроллим вниз
                    double h = MessagesListScrollViewer.Extent.Height - MessagesListScrollViewer.DesiredSize.Height;
                    ForceScroll(h);
                    // MessagesListScrollViewer.ScrollToEnd();

                    Log.Information($"Scroll to message\"{msg.Id}\" done.");
                }
            }
        }

        private async void ForceScroll(double y) {
            while (MessagesListScrollViewer.Offset.Y < y - 2 || MessagesListScrollViewer.Offset.Y > y + 2) {
                MessagesListScrollViewer.Offset = new Vector(0, y);
                await Task.Yield();
            }
        }

        double oldScrollViewerHeight = 0;
        bool needToSaveScroll = false;
        bool autoScrollToLastMessage = false;
        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            double trigger = 160;
            double dh = MessagesListScrollViewer.DesiredSize.Height;
            double h = MessagesListScrollViewer.Extent.Height - dh;
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

            if (canSaveScrollPosinion && h > 0 && y > 0) {
                if (ScrollPositions.ContainsKey(Chat.PeerId)) {
                    ScrollPositions[Chat.PeerId] = new Tuple<double, double>(h, y);
                } else {
                    ScrollPositions.Add(Chat.PeerId, new Tuple<double, double>(h, y));
                }
            }

            oldScrollViewerHeight = h;
            if (y < trigger) {
                // canSaveScrollPosinion — признак того, что
                // data context сменился, а интерфейс не прогрузился.
                if (canSaveScrollPosinion) Chat.LoadPreviousMessages();
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

            CheckFirstAbdLastDisplayedMessages();
        }

        private void TrySaveScroll(object sender, bool e) {
            if (e) return; // нужно "сохранить" скролл при прогрузке предыдущих сообщений.
            needToSaveScroll = true;
        }

        private void ScrollToMessage(object sender, int messageId) {
            if (Chat.DisplayedMessages == null) return;
            MessagesList.ScrollIntoView(Chat.DisplayedMessages.IndexOf(Chat.DisplayedMessages?.GetById(messageId)));
        }

        private void CheckFirstAbdLastDisplayedMessages() {
            MessageViewModel fv = FirstVisible;
            MessageViewModel lv = LastVisible;
            if (Settings.ShowDebugCounters) {
                tmsgId.Text = fv?.Id.ToString() ?? "N/A";
                bmsgId.Text = lv?.Id.ToString() ?? "N/A";
            }
            UpdateDateUnderHeader(fv);
        }

        private void UpdateDateUnderHeader(MessageViewModel msg) {
            if (msg == null) {
                TopDateContainer.IsVisible = false;
                return;
            }

            TopDate.Text = msg.SentTime.ToHumanizedDateString();
            TopDateContainer.IsVisible = true;
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

            ContextMenuHelper.ShowForMessage(message, Chat, cvi);
        }

        #endregion
    }
}