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
using System.Linq;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        ChatViewModel Chat { get; set; }
        ScrollViewer MessagesListScrollViewer;
        DispatcherTimer markReadTimer;

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

                new ItemsPresenterWidthFixer(MessagesList);
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
            if (MessagesListScrollViewer != null) MessagesListScrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            if (Chat != null) {
                Chat.ReceivedMessages.CollectionChanged -= ReceivedMessages_CollectionChanged;
            }

            Chat = DataContext as ChatViewModel;
            if (Chat != null) {
                Chat.ReceivedMessages.CollectionChanged += ReceivedMessages_CollectionChanged;
            }

            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    if (MessagesListScrollViewer != null) MessagesListScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                });
            })();
        }

        private void ReceivedMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            new System.Action(async () => {
                if (!VKSession.GetByDataContext(this).Window.IsActive) return;
                if (e.Action == NotifyCollectionChangedAction.Add && autoScrollToLastMessage && Chat.DisplayedMessages != null && Chat.DisplayedMessages.Count > 0) {
                    ObservableCollection<MessageViewModel> received = sender as ObservableCollection<MessageViewModel>;
                    await Task.Delay(10); // ибо id-ы разные почему-то...
                    int lastReceivedId = received.LastOrDefault()?.ConversationMessageId ?? 0;
                    var lastDisplayed = Chat.DisplayedMessages.Last;
                    int lastDisplayedId = lastDisplayed?.ConversationMessageId ?? 0;
                    if (lastReceivedId == lastDisplayedId && lastDisplayedId > 0) {
                        MessagesList.ScrollIntoView(Chat.DisplayedMessages.Count - 1);
                        //double h = MessagesListScrollViewer.Extent.Height - MessagesListScrollViewer.DesiredSize.Height;
                        //ForceScroll(h);

                        // После изменения сообщения
                        // после loading-cостояния надо ещё раз скроллить до конца.
                        Log.Information($"Need to scroll to last message again. Message id: {lastDisplayedId}");
                        if (lastDisplayed.State == MessageVMState.Loading) lastDisplayed.PropertyChanged += LastDisplayedMsgPropertyChanged;
                    }
                }
            })();
        }

        private void LastDisplayedMsgPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            new System.Action(async () => {
                if (e.PropertyName == nameof(MessageViewModel.State)) {
                    MessageViewModel msg = sender as MessageViewModel;
                    msg.PropertyChanged -= LastDisplayedMsgPropertyChanged;

                    await Task.Delay(10); // нужно, ибо без этого высота скролла старая.

                    int lastReceivedId = Chat.ReceivedMessages.LastOrDefault()?.ConversationMessageId ?? 0;
                    if (msg.ConversationMessageId == lastReceivedId) {
                        // Принудительно скроллим вниз
                        //double h = MessagesListScrollViewer.Extent.Height - MessagesListScrollViewer.DesiredSize.Height;
                        //ForceScroll(h);
                        MessagesListScrollViewer.ScrollToEnd();

                        Log.Information($"Scroll to message \"{msg.ConversationMessageId}\" done.");
                    }
                }
            })();
        }

        //private void ForceScroll(double y) {
        //    new System.Action(async () => {
        //        byte retries = 0;
        //        while (MessagesListScrollViewer.Offset.Y < y - 2 || MessagesListScrollViewer.Offset.Y > y + 2) {
        //            MessagesListScrollViewer.Offset = new Vector(0, y);
        //            await Task.Yield();
        //            retries++;
        //            if (retries >= 5) break;
        //        }
        //    })();
        //}

        bool autoScrollToLastMessage = false;
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            CheckFirstAndLastDisplayedMessages();
        }

        private void MessagesListScrollViewer_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
            if (e.Property == ScrollViewer.ExtentProperty) CheckFirstAndLastDisplayedMessages();
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
            new System.Action(async () => await Chat.GoToMessageAsync(Chat.PinnedMessage))();
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

            new System.Action(async () => await Chat?.Composer.SendStickerAsync(sticker.StickerId))();
        }

        #endregion

        #region Mark message as read

        // TODO: execute (mark message and reactions as read in one request).
        private void MarkReadTimer_Tick(object sender, EventArgs e) {
            new System.Action(async () => {
                await TryMarkAsReadAsync(LastVisible);
                if (Chat.UnreadReactions != null) await TryMarkReactionsAsReadAsync();
            })();
        }

        bool isMarking = false;
        private async Task TryMarkAsReadAsync(MessageViewModel msg) {
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
        private async Task TryMarkReactionsAsReadAsync() {
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

        private void OnDragEnter(object sender, DragEventArgs e) {
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
                new System.Action(async () => await ExceptionHelper.ShowErrorDialogAsync(VKSession.GetByDataContext(this).ModalWindow, ex, true))();
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

        private void OnDropOnArea(object sender, DragEventArgs e) {
            VKSession session = VKSession.GetByDataContext(this);
            Border border = sender as Border;
            border.Classes.Remove("DropTargetHover");

            var files = e.Data.GetFiles().Take(10);
            if (border.Name == "TopDropArea") {
                new System.Action(async () => {
                    foreach (IStorageFile file in files) {
                        Chat.Composer.Attachments.Add(new OutboundAttachmentViewModel(session, file, Constants.FileUploadCommand));
                        await Task.Delay(500);
                    }
                })();
            } else if (border.Name == "BottomDropArea") {
                DroppedFilesType type = (DroppedFilesType)BottomDropArea.Tag;
                int utype = Constants.FileUploadCommand;
                switch (type) {
                    case DroppedFilesType.OnlyPhotos: utype = Constants.PhotoUploadCommand; break;
                    case DroppedFilesType.OnlyVideos: utype = Constants.VideoUploadCommand; break;
                    default: utype = Constants.FileUploadCommand; break;
                }

                new System.Action(async () => {
                    foreach (IStorageFile file in files) {
                        Chat.Composer.Attachments.Add(new OutboundAttachmentViewModel(session, file, utype));
                        await Task.Delay(500);
                    }
                })();
            }
        }

        #endregion
    }
}