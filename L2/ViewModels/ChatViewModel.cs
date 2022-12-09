using Avalonia.Threading;
using ELOR.Laney.Collections;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Execute;
using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ELOR.Laney.ViewModels {
    public sealed class ChatViewModel : CommonViewModel {
        private VKSession session;

        private PeerType _peerType;
        private int _peerId;
        private string _title;
        private string _subtitle;
        private string _activityStatus;
        private Uri _avatar;
        private bool _isVerified;
        private UserOnlineInfo _online;
        private SortId _sortId;
        private int _unreadMessagesCount;
        private ObservableCollection<MessageViewModel> _receivedMessages = new ObservableCollection<MessageViewModel>();
        private MessagesCollection _displayedMessages;
        private MessageViewModel _pinnedMessage;
        private bool _isMuted;
        private int _inread;
        private int _outread;
        private ChatSettings _csettings;
        private CanWrite _canwrite;
        private BotKeyboard _currentKeyboard;
        private bool _isMarkedAsUnread;
        private bool _isPinned;
        private ObservableCollection<int> _mentions;
        private string _restrictionReason;

        public PeerType PeerType { get { return _peerType; } private set { _peerType = value; OnPropertyChanged(); } }
        public int PeerId { get { return _peerId; } private set { _peerId = value; OnPropertyChanged(); } }
        public string Title { get { return _title; } private set { _title = value; OnPropertyChanged(); OnPropertyChanged(nameof(Initials)); } }
        public string Subtitle { get { return _subtitle; } private set { _subtitle = value; OnPropertyChanged(); } }
        public string ActivityStatus { get { return _activityStatus; } set { _activityStatus = value; OnPropertyChanged(); } }
        public Uri Avatar { get { return _avatar; } private set { _avatar = value; OnPropertyChanged(); } }
        public bool IsVerified { get { return _isVerified; } private set { _isVerified = value; OnPropertyChanged(); } }
        public UserOnlineInfo Online { get { return _online; } set { _online = value; OnPropertyChanged(); } }
        public string Initials { get { return _title.GetInitials(PeerId < 0 || PeerId > 1000000000); } }
        public SortId SortId { get { return _sortId; } set { _sortId = value; OnPropertyChanged(); OnPropertyChanged(nameof(SortIndex)); } }
        public ulong SortIndex { get { return GetSortIndex(); } }
        public int UnreadMessagesCount { get { return _unreadMessagesCount; } private set { _unreadMessagesCount = value; OnPropertyChanged(); } }
        public ObservableCollection<MessageViewModel> ReceivedMessages { get { return _receivedMessages; } }
        public MessagesCollection DisplayedMessages { get { return _displayedMessages; } private set { _displayedMessages = value; OnPropertyChanged(); } }
        public MessageViewModel LastMessage { get { return ReceivedMessages.LastOrDefault(); } }
        public MessageViewModel PinnedMessage { get { return _pinnedMessage; } private set { _pinnedMessage = value; OnPropertyChanged(); } }
        public bool IsMuted { get { return _isMuted; } private set { _isMuted = value; OnPropertyChanged(); } }
        public int InRead { get { return _inread; } private set { _inread = value; OnPropertyChanged(); } }
        public int OutRead { get { return _outread; } private set { _outread = value; OnPropertyChanged(); } }
        public ChatSettings ChatSettings { get { return _csettings; } private set { _csettings = value; OnPropertyChanged(); } }
        public CanWrite CanWrite { get { return _canwrite; } private set { _canwrite = value; OnPropertyChanged(); } }
        public BotKeyboard CurrentKeyboard { get { return _currentKeyboard; } private set { _currentKeyboard = value; OnPropertyChanged(); } }
        public bool IsMarkedAsUnread { get { return _isMarkedAsUnread; } private set { _isMarkedAsUnread = value; OnPropertyChanged(); } }
        public bool IsPinned { get { return _isPinned; } private set { _isPinned = value; OnPropertyChanged(); } }
        public ObservableCollection<int> Mentions { get { return _mentions; } private set { _mentions = value; OnPropertyChanged(); } }
        public string RestrictionReason { get { return _restrictionReason; } private set { _restrictionReason = value; OnPropertyChanged(); } }

        public List<User> MembersUsers { get; private set; } = new List<User>();
        public List<Group> MembersGroups { get; private set; } = new List<Group>();

        private User PeerUser;
        private Group PeerGroup;

        public event EventHandler<int> ScrollToMessageRequested;
        public event EventHandler<bool> MessagesChunkLoaded; // получение сообщений (false - предыдущих, true - следующих)
        public EventHandler<MessageViewModel> MessageAddedToLast;

        public ChatViewModel(VKSession session, int peerId) {
            this.session = session;
            SetUpEvents();
            PeerId = peerId;
            Title = peerId.ToString();
        }

        public ChatViewModel(VKSession session, Conversation c, Message lastMessage = null) {
            this.session = session;
            SetUpEvents();
            Setup(c);
            if (lastMessage != null) {
                MessageViewModel msg = new MessageViewModel(lastMessage, session);
                FixState(msg);
                ReceivedMessages.Add(msg);
            }
        }

        // Вызывается при отображении беседы на окне
        public void OnDisplayed() {
            bool isDisplayedMessagesEmpty = DisplayedMessages == null || DisplayedMessages.Count == 0;
            Log.Information("Chat {0} is opened. isDisplayedMessagesEmpty: {1}", PeerId, isDisplayedMessagesEmpty);
            if (isDisplayedMessagesEmpty) {
                LoadMessages();
            } else {
                // TODO: scroll to in_read message
            }
        }

        private void Setup(Conversation c) {
            PeerId = c.Peer.Id;
            if (SortId?.MajorId != c.SortId.MajorId || SortId?.MinorId != c.SortId.MinorId) SortId = c.SortId; // чтобы не дёргался listbox.
            UnreadMessagesCount = c.UnreadCount;
            CanWrite = c.CanWrite;
            InRead = c.InRead;
            OutRead = c.OutRead;
            IsMuted = c.PushSettings != null && c.PushSettings.DisabledForever ? true : false;
            IsMarkedAsUnread = c.IsMarkedUnread;
            IsPinned = SortId.MajorId > 0 && SortId.MajorId % 16 == 0;

            if (c.Mentions != null) Mentions = new ObservableCollection<int>(c.Mentions);
            if (c.CurrentKeyboard != null && c.CurrentKeyboard.Buttons.Count > 0) CurrentKeyboard = c.CurrentKeyboard;

            if (PeerId > 0 && PeerId < 1000000000) { // User
                PeerType = PeerType.User;
                PeerUser = CacheManager.GetUser(PeerId);
                Title = PeerUser.FullName;
                Avatar = new Uri(PeerUser.Photo200);
                IsVerified = PeerUser.Verified;
                Online = PeerUser.OnlineInfo;
            } else if (PeerId < 0 && PeerId > -1000000000) { // Group
                PeerType = PeerType.Group;
                PeerGroup = CacheManager.GetGroup(PeerId);
                Title = PeerGroup.Name;
                Avatar = new Uri(PeerGroup.Photo200);
                IsVerified = PeerGroup.Verified;
                Subtitle = PeerGroup.Activity.ToLowerInvariant();
            } else if (PeerId > 2000000000) { // Chat
                PeerType = PeerType.Chat;
                ChatSettings = c.ChatSettings;
                Title = ChatSettings.Title;
                Avatar = ChatSettings?.Photo?.Uri;
                if (ChatSettings.PinnedMessage != null) 
                    PinnedMessage = new MessageViewModel(ChatSettings.PinnedMessage, session);
                UpdateSubtitleForChat();
            } else if (PeerId > 1900000000 && PeerId <= 2000000000) { // Contact?
                PeerType = PeerType.Contact;
                Title = $"Contact {PeerId}";
            } else if (PeerId < -2000000000) { // E-mail
                PeerType = PeerType.Email;
                Title = $"E-Mail {PeerId}";
            }

            UpdateRestrictionInfo();
        }

        private void UpdateSubtitleForChat() {
            if (ChatSettings.State == UserStateInChat.In) {
                Subtitle = String.Empty;
                if (ChatSettings.IsDisappearing) Subtitle = $"{Localizer.Instance["casper_chat"].ToLowerInvariant()}, ";
                Subtitle += Localizer.Instance.GetDeclensionFormatted(ChatSettings.MembersCount, "members_sub");
            } else {
                Subtitle = Localizer.Instance[ChatSettings.State == UserStateInChat.Left ? "chat_left" : "chat_kicked"].ToLowerInvariant();
            }
        }

        private void UpdateRestrictionInfo() {
            if (CanWrite.Allowed) {
                RestrictionReason = String.Empty; return;
            }

            if (PeerType == PeerType.Chat) {
                if (ChatSettings.State != UserStateInChat.In) {
                    RestrictionReason = Localizer.Instance[$"chat_{ChatSettings.State.ToString().ToLower()}"];
                } else {
                    // VKAPIHelper.GetUnderstandableErrorMessage(CanWrite.Reason);
                }
            } else if (PeerType == PeerType.User) {
                switch (CanWrite.Reason) {
                    case 18:
                        if (PeerUser.Deactivated == DeactivationState.Deleted) RestrictionReason = Localizer.Instance["user_deleted"];
                        if (PeerUser.Deactivated == DeactivationState.Banned) RestrictionReason = Localizer.Instance["user_blocked"];
                        break;
                    case 900:
                        if (PeerUser.Blacklisted) RestrictionReason = Localizer.Instance.Get("user_blacklisted", PeerUser.Sex);
                        if (PeerUser.BlacklistedByMe) RestrictionReason = Localizer.Instance.Get("user_blacklisted_by_me", PeerUser.Sex);
                        break;
                    default:
                        RestrictionReason = VKAPIHelper.GetUnderstandableErrorMessage(CanWrite.Reason, Localizer.Instance["cannot_write"]);
                        break;
                }
            } else if (PeerType == PeerType.Group) {
                RestrictionReason = VKAPIHelper.GetUnderstandableErrorMessage(CanWrite.Reason, Localizer.Instance["cannot_write"]);
            }
        }

        private void SetUpEvents() {
            // При приёме сообщения обновляем последнее сообщение.
            ReceivedMessages.CollectionChanged += (a, b) => OnPropertyChanged(nameof(LastMessage));

            PropertyChanged += (a, b) => { 
                if (b.PropertyName == nameof(Online)) 
                    Subtitle = VKAPIHelper.GetOnlineInfo(Online, PeerUser.Sex).ToLowerInvariant();
            };

            session.LongPoll.MessageReceived += LongPoll_MessageReceived;
            session.LongPoll.IncomingMessagesRead += LongPoll_MessagesRead;
            session.LongPoll.OutgoingMessagesRead += LongPoll_MessagesRead;
            session.LongPoll.ConversationFlagReset += LongPoll_ConversationFlagReset;
            session.LongPoll.ConversationFlagSet += LongPoll_ConversationFlagSet;
            session.LongPoll.MajorIdChanged += LongPoll_MajorIdChanged;
            session.LongPoll.MinorIdChanged += LongPoll_MinorIdChanged;
        }

        #region Loading messages

        public async void GoToMessage(MessageViewModel message) {
            if (message.Id > 0) {
                MessageViewModel msg = (from m in DisplayedMessages where m.Id == message.Id select m).FirstOrDefault();
                // TODO: искать ещё и в received messages.
                if (msg != null) {
                    ScrollToMessageRequested?.Invoke(this, message.Id);
                } else {
                    LoadMessages(message.Id);
                }
            } else {
                VKUIDialog dlg = new VKUIDialog("Showing message in window is not ready yet!", message.ToString(), VKUIDialog.OkButton, 1);
                await dlg.ShowDialog<short>(VKSession.Main.Window);
            }
        }

        private async void LoadMessages(int startMessageId = -1) {
            if (IsLoading) return;
            Placeholder = null;
            DisplayedMessages?.Clear();

            int count = Constants.MessagesCount;
            try {
                Log.Information("LoadMessages peer: {0}, count: {1}", PeerId, count);
                IsLoading = true;
                int offset = -count / 2;
                MessagesHistoryEx mhr = await session.API.GetHistoryWithMembersAsync(session.GroupId, PeerId, offset, count, startMessageId, false, VKAPIHelper.Fields);
                CacheManager.Add(mhr.Profiles);
                CacheManager.Add(mhr.Groups);
                CacheManager.Add(mhr.MentionedProfiles);
                CacheManager.Add(mhr.MentionedGroups);
                MembersUsers = mhr.Profiles;
                MembersGroups = mhr.Groups;
                Setup(mhr.Conversation);
                mhr.Messages.Reverse();
                DisplayedMessages = new MessagesCollection(MessageViewModel.BuildFromAPI(mhr.Messages, session, FixState));

                await Task.Delay(100);
                if (startMessageId > 0) ScrollToMessageRequested?.Invoke(this, startMessageId);
                if (startMessageId == -1) {
                    ScrollToMessageRequested?.Invoke(this, Math.Min(InRead, OutRead));
                }
            } catch (Exception ex) {
                Placeholder = PlaceholderViewModel.GetForException(ex, () => { LoadMessages(startMessageId); });
            } finally {
                IsLoading = false;
                // SetPlaceholder();
            }
        }

        public async void LoadPreviousMessages() {
            if (DisplayedMessages?.Count == 0 || IsLoading) return;
            int count = Constants.MessagesCount;

            try {
                Log.Information("LoadPreviousMessages peer: {0}, count: {1}, displayed messages count: {2}", PeerId, count, DisplayedMessages.Count);
                IsLoading = true;
                MessagesHistoryEx mhr = await session.API.GetHistoryWithMembersAsync(session.GroupId, PeerId, 1, count, DisplayedMessages.First.Id, false, VKAPIHelper.Fields, true);
                CacheManager.Add(mhr.MentionedProfiles);
                CacheManager.Add(mhr.MentionedGroups);
                mhr.Messages.Reverse();
                MessagesChunkLoaded?.Invoke(this, false);
                DisplayedMessages.InsertRange(mhr.Messages.Select(m => {
                    var msg = new MessageViewModel(m, session);
                    FixState(msg);
                    return msg;
                }).ToList());
                await Task.Delay(100); // Нужно, чтобы не триггерилось подгрузка пред/след сообщений из-за scrollviewer-а.
            } catch (Exception ex) {
                //if (await ExceptionHelper.ShowErrorDialogAsync(ex)) {
                //    LoadPreviousMessages();
                //}
            } finally {
                IsLoading = false;
            }
        }

        public async void LoadNextMessages() {
            if (DisplayedMessages?.Count == 0 || IsLoading) return;
            int count = Constants.MessagesCount;

            try {
                Log.Information("LoadNextMessages peer: {0}, count: {1}, displayed messages count: {2}", PeerId, count, DisplayedMessages.Count);
                IsLoading = true;
                MessagesHistoryEx mhr = await session.API.GetHistoryWithMembersAsync(session.GroupId, PeerId, -count, count, DisplayedMessages.Last.Id, false, VKAPIHelper.Fields, false);
                CacheManager.Add(mhr.MentionedProfiles);
                CacheManager.Add(mhr.MentionedGroups);
                mhr.Messages.Reverse();
                MessagesChunkLoaded?.Invoke(this, true);
                DisplayedMessages.InsertRange(mhr.Messages.Select(m => {
                    var msg = new MessageViewModel(m, session);
                    FixState(msg);
                    return msg;
                }).ToList());
                await Task.Delay(100); // Нужно, чтобы не триггерилось подгрузка пред/след сообщений из-за scrollviewer-а.
            } catch (Exception ex) {
                //if (await ExceptionHelper.ShowErrorDialogAsync(ex)) {
                //    LoadNextMessages();
                //}
            } finally {
                IsLoading = false;
            }
        }

        private void FixState(MessageViewModel msg) {
            int senderId = session.Id;
            bool isOutgoing = msg.SenderId == senderId;
            if (isOutgoing) {
                msg.State = msg.Id > OutRead ? MessageVMState.Unread : MessageVMState.Read;
            } else {
                msg.State = msg.Id > InRead ? MessageVMState.Unread : MessageVMState.Read;
            }
        }

        #endregion

        #region LongPoll events

        private async void LongPoll_MessageReceived(LongPoll longPoll, Message message, int flags) {
            if (message.PeerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                MessageViewModel msg = new MessageViewModel(message, session);
                
                if (!message.IsPartial) {
                    bool isUnread = flags.HasFlag(1) && !flags.HasFlag(8388608);
                    msg.State = isUnread ? MessageVMState.Unread : MessageVMState.Read;
                } else {
                    msg.PropertyChanged += MessagePropertyChanged;
                }

                bool canAddToDisplayedMessages = DisplayedMessages?.LastOrDefault()?.Id == LastMessage.Id;
                ReceivedMessages.Add(msg);
                if (!flags.HasFlag(65536)) UpdateSortId(SortId.MajorId, msg.Id);
                if (msg.SenderId != session.Id) UnreadMessagesCount++;
                if (canAddToDisplayedMessages) DisplayedMessages.Insert(msg);
            });
        }

        private void MessagePropertyChanged(object sender, PropertyChangedEventArgs e) {
            MessageViewModel msg = sender as MessageViewModel;
            if (e.PropertyName == nameof(MessageViewModel.State)) {
                msg.PropertyChanged -= MessagePropertyChanged;
                OnPropertyChanged(nameof(LastMessage));
            }
        }

        private void LongPoll_MessagesRead(LongPoll longPoll, int peerId, int messageId, int count) {
            if (PeerId != peerId) return;
            UnreadMessagesCount = count;

            // Чтобы индикаторы прочитанности в списке чатов обновились.
            // if (LastMessage?.Id == messageId) OnPropertyChanged(nameof(LastMessage));
        }

        private async void LongPoll_ConversationFlagReset(LongPoll longPoll, int peerId, int flags) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                
            });
        }

        private async void LongPoll_ConversationFlagSet(LongPoll longPoll, int peerId, int flags) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                
            });
        }

        // Если что, flags и есть major/minor_id.
        private async void LongPoll_MajorIdChanged(LongPoll longPoll, int peerId, int flags) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                UpdateSortId(flags, SortId.MinorId);
            });
        }

        private async void LongPoll_MinorIdChanged(LongPoll longPoll, int peerId, int flags) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                UpdateSortId(SortId.MajorId, flags);
            });
        }

        // Надо задать новый объект SortId, чтобы сработало событие OnPropertyChanged для SortIndex.
        private void UpdateSortId(int major, int minor) {
            SortId = new SortId { 
                MajorId = major,
                MinorId = minor
            };
        }

        #endregion

        private ulong GetSortIndex() {
            if (SortId.MajorId == 0) return (ulong)SortId.MinorId;
            ulong index = ((ulong)SortId.MajorId * 100000000) + (ulong)SortId.MinorId;
            return index;
        }
    }
}