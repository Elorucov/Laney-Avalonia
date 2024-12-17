using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ELOR.Laney.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels;
using ELOR.Laney.ViewModels.Controls;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        ChatViewModel Chat { get; set; }
        ScrollViewer MessagesListScrollViewer;
        DispatcherTimer markReadTimer;
        int scrollToMessageIndex = -1;

        MessageViewModel FirstVisible { get => MessagesListScrollViewer?.GetDataContextAt<MessageViewModel>(new Point(64, 0)); }
        MessageViewModel LastVisible { get => MessagesListScrollViewer?.GetDataContextAt<MessageViewModel>(new Point(64, MessagesListScrollViewer.DesiredSize.Height - 5)); }

        public ChatView() {
            InitializeComponent();
            MultiMsgContextButton.CommandParameter = MultiMsgContextButton;

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
                var ss = new ScrollSaver(MessagesListScrollViewer);
                ss.ExtendChanged += (c, d) => { 
                    if (scrollToMessageIndex >= 0) {
                        Debug.WriteLine($"ScrollSaver.ExtendChanged: scmi={scrollToMessageIndex}");
                        d.Handled = true;
                        MessagesList.ScrollIntoView(scrollToMessageIndex);
                    }
                };
            };

            BackButton.Click += (a, b) => BackButtonClick?.Invoke(this, null);
            DataContextChanged += ChatView_DataContextChanged;
            LoadingSpinner.PropertyChanged += LoadingSpinner_PropertyChanged;

            DebugOverlay.IsVisible = Settings.ShowDebugCounters;
            Settings.SettingChanged += Settings_SettingChanged;

            Root.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            DropArea.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            DropArea.AddHandler(DragDrop.DropEvent, OnDrop);

            TopDropArea.AddHandler(DragDrop.DragEnterEvent, OnDragEnterIntoArea);
            BottomDropArea.AddHandler(DragDrop.DragEnterEvent, OnDragEnterIntoArea);
            TopDropArea.AddHandler(DragDrop.DragLeaveEvent, OnDragLeaveFromArea);
            BottomDropArea.AddHandler(DragDrop.DragLeaveEvent, OnDragLeaveFromArea);
            TopDropArea.AddHandler(DragDrop.DropEvent, OnDropOnArea);
            BottomDropArea.AddHandler(DragDrop.DropEvent, OnDropOnArea);
        }

        Stopwatch sw = null;
        private void CVI_Initialized(object sender, EventArgs e) {
            canTriggerLoadingMessages = false;
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
                canTriggerLoadingMessages = true;
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

        bool canTriggerLoadingMessages = false;
        private async void ChatView_DataContextChanged(object sender, EventArgs e) {
            if (MessagesListScrollViewer != null) MessagesListScrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            sw = null;
            totalDisplayedMessagesForScrollFix = 0;
            if (Chat != null) {
                Chat.ScrollToMessageRequested -= ScrollToMessage;
                Chat.MessagesChunkLoaded -= TrySaveScroll;
                Chat.ReceivedMessages.CollectionChanged -= ReceivedMessages_CollectionChanged;
            }

            Chat = DataContext as ChatViewModel;
            if (Chat != null) {
                Chat.ScrollToMessageRequested += ScrollToMessage;
                Chat.MessagesChunkLoaded += TrySaveScroll;
                Chat.ReceivedMessages.CollectionChanged += ReceivedMessages_CollectionChanged;
            }

            await Dispatcher.UIThread.InvokeAsync(() => {
                if (MessagesListScrollViewer != null) MessagesListScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            });
        }

        private async void ReceivedMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (!VKSession.GetByDataContext(this).Window.IsActive) return;
            if (e.Action == NotifyCollectionChangedAction.Add && autoScrollToLastMessage && Chat.DisplayedMessages != null && Chat.DisplayedMessages.Count > 0) {
                ObservableCollection<MessageViewModel> received = sender as ObservableCollection<MessageViewModel>;
                await Task.Delay(10); // ибо id-ы разные почему-то...
                int lastReceivedId = received.LastOrDefault()?.ConversationMessageId ?? 0;
                var lastDisplayed = Chat.DisplayedMessages.Last;
                int lastDisplayedId = lastDisplayed?.ConversationMessageId ?? 0;
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

                int lastReceivedId = Chat.ReceivedMessages.LastOrDefault()?.ConversationMessageId ?? 0;
                if (msg.ConversationMessageId == lastReceivedId) {
                    // Принудительно скроллим вниз
                    double h = MessagesListScrollViewer.Extent.Height - MessagesListScrollViewer.DesiredSize.Height;
                    ForceScroll(h);
                    // MessagesListScrollViewer.ScrollToEnd();

                    Log.Information($"Scroll to message \"{msg.ConversationMessageId}\" done.");
                }
            }
        }

        private async void ForceScroll(double y) {
            byte retries = 0;
            while (MessagesListScrollViewer.Offset.Y < y - 2 || MessagesListScrollViewer.Offset.Y > y + 2) {
                MessagesListScrollViewer.Offset = new Vector(0, y);
                await Task.Yield();
                retries++;
                if (retries >= 5) break;
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

            if (needToSaveScroll && !canTriggerLoadingMessages && oldScrollViewerHeight != h) {
                double diff = h - oldScrollViewerHeight;
                double newpos = y + diff;

                int tries = 0;
                MessagesListScrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    while (tries < 10) {
                        try {
                            ForceScroll(newpos);
                        } catch (Exception ex) {
                            Log.Error(ex, "CheckScroll: Unable to save scroll position!");
                        }
                        await Task.Yield();
                        tries++;
                    }
                    if (MessagesListScrollViewer.Offset.Y != newpos) {
                        Log.Error("CheckScroll: Unable to save scroll position after 10 times!");
                    } else {
                        Log.Information($"CheckScroll: Scroll position saved successfully! Tries: {tries}");
                    }
                });
                MessagesListScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

                needToSaveScroll = false;
                oldScrollViewerHeight = h;
                return;
            }

            oldScrollViewerHeight = h;
            if (y < trigger) {
                if (canTriggerLoadingMessages) Chat.LoadPreviousMessages();
                autoScrollToLastMessage = false;
            } else if (y > h - trigger) {
                if (Chat.DisplayedMessages.Last == null || Chat.LastMessage == null) return;
                bool needLoadNextMessages = Chat.DisplayedMessages.Last.ConversationMessageId != Chat.LastMessage.ConversationMessageId;
                if (needLoadNextMessages) {
                    if (canTriggerLoadingMessages) Chat.LoadNextMessages();
                } else {
                    autoScrollToLastMessage = true;
                }
            } else {
                autoScrollToLastMessage = false;
            }
            dbgScrAuto.Text = $"{autoScrollToLastMessage}";

            await Task.Delay(15); // надо
            CheckFirstAndLastDisplayedMessages();
        }

        private void TrySaveScroll(object sender, bool e) {
            if (e) return; // нужно "сохранить" скролл при прогрузке предыдущих сообщений.
            needToSaveScroll = true;
        }

        private void ScrollToMessage(object sender, int messageId) {
            if (Chat.DisplayedMessages == null) return;
            var msg = Chat.DisplayedMessages?.GetById(messageId);
            int index = Chat.DisplayedMessages.IndexOf(msg);
            Log.Information($"ScrollToMessage: cmid={messageId}; index={index}");

            // проверка, что мы хотим прокрутиться до последнего сообщения
            // зачем? потому что, если последнее сообщение по размерам больше, чем 
            // viewport, то будет показываться только начало этого сообщения
            // а мы хотим самый низ - поэтому так
            scrollToMessageIndex = index;
            if (index != Chat.DisplayedMessages.Count - 1)
                MessagesList.ScrollIntoView(scrollToMessageIndex);
            else
                MessagesList.Scroll.Offset = MessagesList.Scroll.Offset.WithY(MessagesList.Scroll.Extent.Height);
        }

        private void CheckFirstAndLastDisplayedMessages() {
            MessageViewModel fv = FirstVisible;
            MessageViewModel lv = LastVisible;
            if (Settings.ShowDebugCounters) {
                tmsgId.Text = fv?.ConversationMessageId.ToString() ?? "N/A";
                bmsgId.Text = lv?.ConversationMessageId.ToString() ?? "N/A";
            }

            UpdateDateUnderHeader(fv);
            if (Chat?.DisplayedMessages?.Count > 0) {
                HopNavContainer.IsVisible = !autoScrollToLastMessage;
            } else {
                HopNavContainer.IsVisible = false;
            }
            try {
                if (MessagesListScrollViewer?.Extent.Height <= MessagesListScrollViewer?.DesiredSize.Height) HopNavContainer.IsVisible = false;
            } catch (Exception ex) {
                Log.Error(ex, "Failed to check messages list's ScrollViewer!");
                HopNavContainer.IsVisible = false;
            }
        }

        private void UpdateDateUnderHeader(MessageViewModel msg) {
            if (msg == null) {
                TopDateContainer.IsVisible = false;
                return;
            }

            TopDate.Text = msg.SentTime.ToHumanizedDateString();
            TopDateContainer.IsVisible = true;
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

        #region Buttons events

        private void PinnedMessageButton_Click(object sender, RoutedEventArgs e) {
            Chat.GoToMessage(Chat.PinnedMessage);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e) {
            if (DemoMode.IsEnabled) return;
            Window mainWindow = TopLevel.GetTopLevel(this) as Window;
            SearchInChatWindow window = new SearchInChatWindow(VKSession.GetByDataContext(this), Chat.PeerId, mainWindow);
            window.Show();
        }

        private void OnSuggestedStickerClicked(object? sender, RoutedEventArgs e) {
            Button button = sender as Button;
            Sticker sticker = button.DataContext as Sticker;
            if (sticker == null) return;

            Chat?.Composer.SendSticker(sticker.StickerId);
        }

        #endregion

        #region Mark message as read

        private void MarkReadTimer_Tick(object sender, EventArgs e) {
            TryMarkAsRead(LastVisible);
            if (Chat.UnreadReactions != null) TryMarkReactionsAsRead();
        }

        bool isMarking = false;
        private async void TryMarkAsRead(MessageViewModel msg) {
            if (DemoMode.IsEnabled) return;
            if (isMarking || msg == null || msg.IsOutgoing || msg.State != MessageVMState.Unread) return;
            isMarking = true;

            try {
                var session = VKSession.GetByDataContext(this);
                bool result = await session.API.Messages.MarkAsReadAsync(session.GroupId, Chat.PeerId, msg.ConversationMessageId);
                await Task.Delay(1000);
            } catch (Exception ex) {
                Log.Error(ex, $"Unable to mark message (peer: {msg.PeerId}; cmid: {msg.ConversationMessageId}) as read!");
            } finally {
                isMarking = false;
            }
        }

        bool isMarking2 = false;
        private async void TryMarkReactionsAsRead() {
            if (DemoMode.IsEnabled) return;
            if (isMarking2 || FirstVisible == null || LastVisible == null) return;
            var fid = FirstVisible.ConversationMessageId;
            var lid = LastVisible.ConversationMessageId;
            var visibleMessages = Chat.DisplayedMessages.Select(m => m.ConversationMessageId)
                .Where(id => id >= fid && id <= lid && Chat.UnreadReactions.Contains(id)).ToList();
            if (visibleMessages == null || visibleMessages.Count == 0) return;

            isMarking2 = true;
            try {
                Log.Information($"About to mark reactions in messages (cmids {String.Join(',', visibleMessages)}) as read...");
                var session = VKSession.GetByDataContext(this);
                bool result = await session.API.Messages.MarkReactionsAsReadAsync(session.GroupId, Chat.PeerId, visibleMessages);
                await Task.Delay(1000);
            } catch (Exception ex) {
                Log.Error(ex, $"Unable to mark reactions in messages (cmids {String.Join(',', visibleMessages)}) as read!");
            } finally {
                isMarking2 = false;
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

        #region Drag'n'drop

        private async void OnDragEnter(object sender, DragEventArgs e) {
            DropArea.IsVisible = true;

            try {
                var files = e.Data.GetFiles();
                var type = files.GetFilesType();
                BottomDropArea.Tag = type;
                switch (type) {
                    case DroppedFilesType.OnlyPhotos:
                        Grid.SetRowSpan(TopDropArea, 1);
                        BottomDropArea.IsVisible = true;
                        BottomDropIcon.Id = VKIconNames.Icon56GalleryOutline;
                        BottomDropText.Text = Assets.i18n.Resources.drop_photos_quick;
                        TopDropText.Text = Assets.i18n.Resources.drop_photos_file;
                        break;
                    case DroppedFilesType.OnlyVideos:
                        Grid.SetRowSpan(TopDropArea, 1);
                        BottomDropArea.IsVisible = true;
                        BottomDropIcon.Id = VKIconNames.Icon56VideoOutline;
                        BottomDropText.Text = Assets.i18n.Resources.drop_videos_quick;
                        TopDropText.Text = Assets.i18n.Resources.drop_videos_file;
                        break;
                    case DroppedFilesType.Mixed:
                        Grid.SetRowSpan(TopDropArea, 2);
                        BottomDropArea.IsVisible = false;
                        TopDropText.Text = Assets.i18n.Resources.drop_without_compression_desc;
                        break;
                }
            } catch (Exception ex) {
                DropArea.IsVisible = false;
                await ExceptionHelper.ShowErrorDialogAsync(VKSession.GetByDataContext(this).ModalWindow, ex, true);
            }
        }

        private void OnDragLeave(object sender, DragEventArgs e) {
            DropArea.IsVisible = false;
        }

        private void OnDrop(object sender, DragEventArgs e) {
            DropArea.IsVisible = false;
        }

        private void OnDragEnterIntoArea(object sender, DragEventArgs e) {
            Border border = sender as Border;
            border.Classes.Add("DropTargetHover");
        }

        private void OnDragLeaveFromArea(object sender, DragEventArgs e) {
            Border border = sender as Border;
            border.Classes.Remove("DropTargetHover");
        }

        private async void OnDropOnArea(object sender, DragEventArgs e) {
            VKSession session = VKSession.GetByDataContext(this);
            Border border = sender as Border;
            border.Classes.Remove("DropTargetHover");

            var files = e.Data.GetFiles().Take(10);
            if (border.Name == "TopDropArea") {
                foreach (IStorageFile file in files) {
                    Chat.Composer.Attachments.Add(new OutboundAttachmentViewModel(session, file, Constants.FileUploadCommand));
                    await Task.Delay(500);
                }
            } else if (border.Name == "BottomDropArea") {
                DroppedFilesType type = (DroppedFilesType)BottomDropArea.Tag;
                int utype = Constants.FileUploadCommand;
                switch (type) {
                    case DroppedFilesType.OnlyPhotos: utype = Constants.PhotoUploadCommand; break;
                    case DroppedFilesType.OnlyVideos: utype = Constants.VideoUploadCommand; break;
                    default: utype = Constants.FileUploadCommand; break;
                }

                foreach (IStorageFile file in files) {
                    Chat.Composer.Attachments.Add(new OutboundAttachmentViewModel(session, file, utype));
                    await Task.Delay(500);
                }
            }
        }

        #endregion
    }
}