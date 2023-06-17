using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        ChatViewModel Chat { get; set; }
        ScrollViewer MessagesListScrollViewer;         // peer id, first visible message id
        Dictionary<int, int> ScrollPositions = new Dictionary<int, int>();
        DispatcherTimer markReadTimer;
        bool canSaveScrollPosition = false;
        ListBoxCustomVirtualization msgsVirtualizer;

        MessageViewModel FirstVisible { get => MessagesListScrollViewer?.GetDataContextAt<MessageViewModel>(new Point(64, 0)); }
        MessageViewModel LastVisible { get => MessagesListScrollViewer?.GetDataContextAt<MessageViewModel>(new Point(64, MessagesListScrollViewer.DesiredSize.Height - 5)); }

        public ChatView() {
            InitializeComponent();

            MessagesList.Loaded += (a, b) => {
                MessagesListScrollViewer = MessagesList.Scroll as ScrollViewer;
                MessagesListScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                MessagesListScrollViewer.PropertyChanged += MessagesListScrollViewer_PropertyChanged;

                if (!Settings.DisableMarkingMessagesAsRead) {
                    markReadTimer = new DispatcherTimer {
                        Interval = TimeSpan.FromSeconds(1)
                    };
                    markReadTimer.Tick += MarkReadTimer_Tick;
                    markReadTimer.Start();
                }

                new ListBoxAutoScrollHelper(MessagesList) {
                    ScrollToLastItemAfterTabFocus = true
                };
                new ItemsPresenterWidthFixer(MessagesList);
                if (Settings.MessagesListVirtualization) msgsVirtualizer = new ListBoxCustomVirtualization(MessagesList);
            };

            BackButton.Click += (a, b) => BackButtonClick?.Invoke(this, null);
            DataContextChanged += ChatView_DataContextChanged;
            PinnedMessageButton.Click += PinnedMessageButton_Click;
            LoadingSpinner.PropertyChanged += LoadingSpinner_PropertyChanged;

            DebugOverlay.IsVisible = Settings.ShowDebugCounters;
            Settings.SettingChanged += Settings_SettingChanged;
        }

        Stopwatch sw = null;
        private void CVI_Initialized(object sender, EventArgs e) {
            if (sw == null) {
                sw = Stopwatch.StartNew();
                Log.Information($"Starting rendering ChatViewItem-s...");
            }
        }

        // Выполняется после apply template у ChatViewItem.
        // Удобно с помощью этого скроллить куда надо.

        int totalDisplayedMessagesForScrollFix = 0;
        int currentScrollFixIteration = 0;

        private void CVI_Loaded(object sender, RoutedEventArgs e) {
            if (Chat == null) return;
            if (totalDisplayedMessagesForScrollFix == 0) totalDisplayedMessagesForScrollFix = Chat.DisplayedMessages.Count;
            currentScrollFixIteration++;
            // Debug.WriteLine($"{currentScrollFixIteration}/{totalDisplayedMessagesForScrollFix}");

            if (totalDisplayedMessagesForScrollFix == currentScrollFixIteration) {
                sw.Stop();
                long ms = sw.ElapsedMilliseconds;
                currentScrollFixIteration = 0;

                Log.Information($"All ChatViewItem are rendered. Count: {totalDisplayedMessagesForScrollFix}, time: {ms} ms.");
                FixScroll();
            }
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

        private async void FixScroll() {
            canSaveScrollPosition = false;

            if (ScrollPositions.ContainsKey(Chat.PeerId)) {
                var fvi = ScrollPositions[Chat.PeerId];
                Log.Information($"Trying to restore scroll position for chat {Chat.PeerId}. First visible message ID: {fvi}");

                var fvm = Chat.DisplayedMessages.Where(m => m.Id == fvi).FirstOrDefault();
                if (fvm != null) {
                    int index = Chat.DisplayedMessages.IndexOf(fvm);

                    byte retries = 20;
                    double y = MessagesListScrollViewer.Offset.Y;
                    while (retries > 0) {
                        await Task.Yield();
                        MessagesList.ScrollIntoView(index);
                        retries--;
                        if (MessagesListScrollViewer.Offset.Y == y) break;
                        y = MessagesListScrollViewer.Offset.Y;
                    }
                    if (y != MessagesListScrollViewer.Offset.Y) Log.Warning($"Cannot scroll to old position! FVI: {fvi}");
                } else {
                    Log.Warning($"Cannot find a message {fvi} in displayed messages! (required to scroll to old position)");
                }

                //ForceScroll(scrollPos.Item2);
            }
            canSaveScrollPosition = true;
            CheckFirstAndLastDisplayedMessages();
        }

        private void ChatView_DataContextChanged(object sender, EventArgs e) {
            sw = null;
            totalDisplayedMessagesForScrollFix = 0;
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
            if (!VKSession.GetByDataContext(this).Window.IsActive) return;
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
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            CheckScroll();
        }

        private void MessagesListScrollViewer_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
            if (e.Property == ScrollViewer.ExtentProperty) CheckScroll();
        }

        private async void CheckScroll() {
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

            oldScrollViewerHeight = h;
            if (y < trigger) {
                // canSaveScrollPosition — признак того, что
                // data context сменился, а интерфейс не прогрузился.
                if (canSaveScrollPosition) Chat.LoadPreviousMessages();
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

            await Task.Delay(32); // надо
            CheckFirstAndLastDisplayedMessages();
        }

        private void TrySaveScroll(object sender, bool e) {
            if (e) return; // нужно "сохранить" скролл при прогрузке предыдущих сообщений.
            needToSaveScroll = true;
        }

        private async void ScrollToMessage(object sender, int messageId) {
            if (Chat.DisplayedMessages == null) return;
            MessagesList.ScrollIntoView(Chat.DisplayedMessages.IndexOf(Chat.DisplayedMessages?.GetById(messageId)));
            if (Settings.MessagesListVirtualization) {
                await Task.Delay(200);
                msgsVirtualizer.EnforceProcessVirtualization();
            }
        }

        private void CheckFirstAndLastDisplayedMessages() {
            MessageViewModel fv = FirstVisible;
            MessageViewModel lv = LastVisible;
            if (Settings.ShowDebugCounters) {
                tmsgId.Text = fv?.Id.ToString() ?? "N/A";
                bmsgId.Text = lv?.Id.ToString() ?? "N/A";
            }

            if (canSaveScrollPosition && fv != null) {
                if (ScrollPositions.ContainsKey(Chat.PeerId)) {
                    ScrollPositions[Chat.PeerId] = fv.Id;
                } else {
                    ScrollPositions.Add(Chat.PeerId, fv.Id);
                }
            }
            UpdateDateUnderHeader(fv);
            HopNavContainer.IsVisible = lv == null || Chat?.LastMessage?.Id != lv.Id;
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

        private void LoadingSpinner_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
            if (e.Property == VKUI.Controls.Spinner.IsVisibleProperty) {
                TopDateContainer.IsVisible = !LoadingSpinner.IsVisible;
                HopNavContainer.IsVisible = !LoadingSpinner.IsVisible;
                if (!LoadingSpinner.IsVisible) CheckFirstAndLastDisplayedMessages();
            }
        }

        private void ChatView_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (e.NewSize.Width >= 512) {
                MessagesCommandsRoot.Classes.Clear();
            } else {
                if (!MessagesCommandsRoot.Classes.Contains("CompactMsgCmd")) MessagesCommandsRoot.Classes.Add("CompactMsgCmd");
            }
        }

        #region Mark message as read

        private void MarkReadTimer_Tick(object sender, EventArgs e) {
            TryMarkAsRead(LastVisible);
        }

        bool isMarking = false;
        private async void TryMarkAsRead(MessageViewModel msg) {
            if (isMarking || msg == null || msg.State != MessageVMState.Unread) return;
            isMarking = true;

            try {
                var session = VKSession.GetByDataContext(this);
                bool result = await session.API.Messages.MarkAsReadAsync(session.GroupId, Chat.PeerId, msg.Id);
                await Task.Delay(1000);
            } catch (Exception ex) {
                Log.Error(ex, $"Unable to mark message (id {msg.Id}) as read!");
            } finally {
                isMarking = false;
            }
        }

        #endregion

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