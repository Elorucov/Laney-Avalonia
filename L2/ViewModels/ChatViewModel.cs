using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Threading;
using ELOR.Laney.Collections;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VKUI.Controls;

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
        private PushSettings _pushSettings;
        private int _inread;
        private int _outread;
        private ChatSettings _csettings;
        private CanWrite _canwrite;
        private ComposerViewModel _composer;
        private bool _isMarkedAsUnread;
        private bool _isPinned;
        private ObservableCollection<int> _mentions;
        private bool _hasMention;
        private bool _hasSelfDestructMessage;
        private string _restrictionReason;
        private bool _isCurrentOpenedChat;
        private int _selectedMessagesCount;
        private ObservableCollection<Command> _messagesCommands = new ObservableCollection<Command>();
        private RelayCommand _openProfileCommand;
        private RelayCommand _goToLastMessageCommand;

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
        public PushSettings PushSettings { get { return _pushSettings; } private set { _pushSettings = value; OnPropertyChanged(); } }
        public int InRead { get { return _inread; } private set { _inread = value; OnPropertyChanged(); } }
        public int OutRead { get { return _outread; } private set { _outread = value; OnPropertyChanged(); } }
        public ChatSettings ChatSettings { get { return _csettings; } private set { _csettings = value; OnPropertyChanged(); } }
        public CanWrite CanWrite { get { return _canwrite; } private set { _canwrite = value; OnPropertyChanged(); } }
        public ComposerViewModel Composer { get { return _composer; } private set { _composer = value; OnPropertyChanged(); } }
        public bool IsMarkedAsUnread { get { return _isMarkedAsUnread; } private set { _isMarkedAsUnread = value; OnPropertyChanged(); } }
        public bool IsPinned { get { return _isPinned; } private set { _isPinned = value; OnPropertyChanged(); } }
        public ObservableCollection<int> Mentions { get { return _mentions; } private set { _mentions = value; OnPropertyChanged(); } }
        public bool HasMention { get { return _hasMention; } private set { _hasMention = value; OnPropertyChanged(); } }
        public bool HasSelfDestructMessage { get { return _hasSelfDestructMessage; } private set { _hasSelfDestructMessage = value; OnPropertyChanged(); } }
        public string MentionIconId { get { return GetMentionIcon(); } }
        public string RestrictionReason { get { return _restrictionReason; } private set { _restrictionReason = value; OnPropertyChanged(); } }
        public bool IsCurrentOpenedChat { get { return _isCurrentOpenedChat; } private set { _isCurrentOpenedChat = value; OnPropertyChanged(); } }
        public int SelectedMessagesCount { get { return _selectedMessagesCount; } private set { _selectedMessagesCount = value; OnPropertyChanged(); } }
        public ObservableCollection<Command> MessagesCommands { get { return _messagesCommands; } private set { _messagesCommands = value; OnPropertyChanged(); } }
        public RelayCommand OpenProfileCommand { get { return _openProfileCommand; } private set { _openProfileCommand = value; OnPropertyChanged(); } }
        public RelayCommand GoToLastMessageCommand { get { return _goToLastMessageCommand; } private set { _goToLastMessageCommand = value; OnPropertyChanged(); } }


        public SelectionModel<MessageViewModel> SelectedMessages { get; } = new SelectionModel<MessageViewModel> { 
            SingleSelect = false
        };

        public List<User> MembersUsers { get; private set; } = new List<User>();
        public List<Group> MembersGroups { get; private set; } = new List<Group>();

        private User PeerUser;
        private Group PeerGroup;

        public event EventHandler<int> ScrollToMessageRequested;
        public event EventHandler<bool> MessagesChunkLoaded; // получение сообщений (false - предыдущих, true - следующих)
        public EventHandler<MessageViewModel> MessageAddedToLast;

        Elapser<LongPollActivityInfo> ActivityStatusUsers = new Elapser<LongPollActivityInfo>();

        public ChatViewModel(VKSession session, int peerId, Message lastMessage = null, bool needSetup = false) {
            this.session = session;
            Composer = new ComposerViewModel(session, this);
            SetUpEvents();
            PeerId = peerId;
            Title = peerId.ToString();
            if (lastMessage != null) {
                MessageViewModel msg = new MessageViewModel(lastMessage, session);
                FixState(msg);
                if (SortId == null) SortId = new SortId { MajorId = 0, MinorId = lastMessage.Id };
                ReceivedMessages.Add(msg);
            }
            // needSetup нужен в случае, когда мы не переходим в беседу и не загружаем сообщения,
            // но надо загрузить инфу о чате, которую можно получить при загрузке сообщений.
            if (needSetup) GetInfoFromAPIAndSetup();
        }

        public ChatViewModel(VKSession session, Conversation c, Message lastMessage = null) {
            this.session = session;
            Composer = new ComposerViewModel(session, this);
            SetUpEvents();
            Setup(c);
            if (lastMessage != null) {
                MessageViewModel msg = new MessageViewModel(lastMessage, session);
                FixState(msg);
                ReceivedMessages.Add(msg);
            }
        }

        // Вызывается при отображении беседы на окне
        public void OnDisplayed(int messageId = -1) {
            bool isDisplayedMessagesEmpty = DisplayedMessages == null || DisplayedMessages.Count == 0;
            Log.Information("Chat {0} is opened. isDisplayedMessagesEmpty: {1}", PeerId, isDisplayedMessagesEmpty);
            if (isDisplayedMessagesEmpty || messageId >= 0) {
                GoToMessage(messageId);
            } else {
                ScrollToMessageRequested?.Invoke(this, InRead);
            }
        }

        private async void GetInfoFromAPIAndSetup() {
            try {
                var response = await session.API.Messages.GetConversationsByIdAsync(session.GroupId, new List<int> { PeerId }, true, VKAPIHelper.Fields);
                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);
                Setup(response.Items.FirstOrDefault());
            } catch (Exception ex) {
                Log.Error(ex, $"Cannot get conv from API! peer={PeerId}");
            }
        }

        private void Setup(Conversation c) {
            PeerId = c.Peer.Id;
            if (SortId?.MajorId != c.SortId.MajorId || SortId?.MinorId != c.SortId.MinorId) SortId = c.SortId; // чтобы не дёргался listbox.
            UnreadMessagesCount = c.UnreadCount;
            CanWrite = c.CanWrite;
            InRead = c.InRead;
            OutRead = c.OutRead;
            PushSettings = c.PushSettings;
            IsMarkedAsUnread = c.IsMarkedUnread;
            IsPinned = SortId.MajorId > 0 && SortId.MajorId % 16 == 0;

            if (PushSettings == null) PushSettings = new PushSettings { 
                NoSound = false,
                DisabledForever = false,
                DisabledUntil = 0
            };

            if (c.Mentions != null && c.Mentions.Count > 0) {
                Mentions = new ObservableCollection<int>(c.Mentions);
                HasMention = true;
            }
            if (c.ExpireConvMessageIds != null && c.ExpireConvMessageIds.Count > 0) {
                HasSelfDestructMessage = true;
            }
            if (c.CurrentKeyboard != null && c.CurrentKeyboard.Buttons.Count > 0) Composer.BotKeyboard = c.CurrentKeyboard;

            if (PeerId > 0 && PeerId < 1000000000) { // User
                PeerType = PeerType.User;
                PeerUser = CacheManager.GetUser(PeerId);
                if (PeerId == session.Id) {
                    Title = Localizer.Instance["favorites"];
                    Avatar = new Uri("https://vk.com/images/icons/im_favorites_200.png");
                } else {
                    Title = PeerUser.FullName;
                    Avatar = new Uri(PeerUser.Photo200);
                }
                IsVerified = PeerUser.Verified;
                Online = PeerUser.OnlineInfo;
            } else if (PeerId < 0 && PeerId > -1000000000) { // Group
                PeerType = PeerType.Group;
                PeerGroup = CacheManager.GetGroup(PeerId);
                Title = PeerGroup.Name;
                Avatar = new Uri(PeerGroup.Photo200);
                IsVerified = PeerGroup.Verified;
                Subtitle = PeerGroup.Activity?.ToLowerInvariant();
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

            // Checking and displaying activity status
            if (DemoMode.IsEnabled) {
                var ds = DemoMode.GetDemoSessionById(session.Id);
                if (ds.ActivityStatuses.ContainsKey(PeerId)) {
                    foreach (var status in ds.ActivityStatuses[PeerId]) {
                        ActivityStatusUsers.Add(status, 1000 * 3600);
                    }
                    UpdateActivityStatus();
                }
            } else {
                OpenProfileCommand = new RelayCommand(OpenPeerProfile);
                GoToLastMessageCommand = new RelayCommand(GoToLastMessage);
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

        private string GetMentionIcon() {
            if (HasSelfDestructMessage) return VKIconNames.Icon12Bomb;
            if (HasMention) return VKIconNames.Icon12Mention;
            return null;
        }

        private void SetUpEvents() {
            // При приёме сообщения обновляем последнее сообщение.
            ReceivedMessages.CollectionChanged += (a, b) => OnPropertyChanged(nameof(LastMessage));
            SelectedMessages.SelectionChanged += SelectedMessages_SelectionChanged;

            PropertyChanged += (a, b) => { 
                if (b.PropertyName == nameof(Online)) 
                    Subtitle = VKAPIHelper.GetOnlineInfo(Online, PeerUser.Sex).ToLowerInvariant();

                if (b.PropertyName == nameof(HasMention) || b.PropertyName == nameof(HasSelfDestructMessage))
                    OnPropertyChanged(nameof(MentionIconId));
            };

            if (!DemoMode.IsEnabled) {
                session.CurrentOpenedChatChanged += (a, b) => IsCurrentOpenedChat = b == PeerId;

                session.LongPoll.MessageFlagSet += LongPoll_MessageFlagSet;
                session.LongPoll.MessageReceived += LongPoll_MessageReceived;
                session.LongPoll.MessageEdited += LongPoll_MessageEdited;
                session.LongPoll.MentionReceived += LongPoll_MentionReceived;
                session.LongPoll.IncomingMessagesRead += LongPoll_IncomingMessagesRead;
                session.LongPoll.OutgoingMessagesRead += LongPoll_OutgoingMessagesRead;
                session.LongPoll.ConversationFlagReset += LongPoll_ConversationFlagReset;
                session.LongPoll.ConversationFlagSet += LongPoll_ConversationFlagSet;
                session.LongPoll.ConversationRemoved += LongPoll_ConversationRemoved;
                session.LongPoll.MajorIdChanged += LongPoll_MajorIdChanged;
                session.LongPoll.MinorIdChanged += LongPoll_MinorIdChanged;
                session.LongPoll.ConversationDataChanged += LongPoll_ConversationDataChanged;
                session.LongPoll.ActivityStatusChanged += LongPoll_ActivityStatusChanged;
                session.LongPoll.NotificationsSettingsChanged += LongPoll_NotificationsSettingsChanged;
            }

            ActivityStatusUsers.Elapsed += (a, b) => UpdateActivityStatus();
        }

        private void SelectedMessages_SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs<MessageViewModel> e) {
            SelectedMessagesCount = SelectedMessages.Count;
            MessagesCommands.Clear();
            if (SelectedMessagesCount > 0) {
                Command reply = new Command(VKIconNames.Icon24ReplyOutline, Localizer.Instance["reply"], false, ReplyToMessageCommand);
                Command fwdhere = new Command(VKIconNames.Icon24ReplyOutline, Localizer.Instance["forward_here"], false, ForwardHereCommand);
                Command forward = new Command(VKIconNames.Icon24ShareOutline, Localizer.Instance["forward"], false, ForwardCommand);

                MessagesCommands.Add(SelectedMessagesCount == 1 ? reply : fwdhere);
                MessagesCommands.Add(forward);
            }
        }

        #region Commands

        private void OpenPeerProfile(object o) {
            Router.OpenPeerProfile(session, PeerId);
        }

        private void GoToLastMessage(object obj) {
            if (IsLoading) return;

            if (DisplayedMessages.Last.Id == LastMessage.Id) {
                GoToMessage(LastMessage);
            } else {
                Log.Information($"GoToLastMessage: last message in chat is not displayed. Showing ReceivedMessages...");
                DisplayedMessages = new MessagesCollection(ReceivedMessages.ToList());
                ScrollToMessageRequested?.Invoke(this, LastMessage.Id);
                if (ReceivedMessages.Count < 20) {
                    Log.Information($"GoToLastMessage: need get more messages from API to display.");
                    MessagesChunkLoaded += PrevMessagesLoaded;
                    LoadPreviousMessages();
                }
            }
        }

        private void PrevMessagesLoaded(object sender, bool next) {
            if (next) return;
            MessagesChunkLoaded -= PrevMessagesLoaded;
            ScrollToMessageRequested?.Invoke(this, LastMessage.Id);
        }

        public void ClearSelectedMessages() {
            SelectedMessages.Clear();
        }

        private void ReplyToMessageCommand(object o) {
            if (SelectedMessages.Count > 0) Composer.AddReply(SelectedMessages.SelectedItem);
            SelectedMessages.Clear();
        }

        private void ForwardHereCommand(object o) {
            Composer.Clear();
            Composer.AddForwardedMessages(SelectedMessages.SelectedItems.ToList(), session.GroupId);
            SelectedMessages.Clear();
        }

        private void ForwardCommand(object o) {
            // TODO
        }

        public void ShowContextMenuForSelectedMessages(object p) {
            ContextMenuHelper.ShowForMultipleMessages(SelectedMessages.SelectedItems.ToList(), (Control)p);
        }

        #endregion

        #region Loading messages

        public void GoToMessage(MessageViewModel message) {
            if (message == null) return;
            if (message.Id > 0) {
                GoToMessage(message.Id);
            } else {
                ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);
            }
        }

        public void GoToMessage(int id) {
            if (id == 0) return;
            if (DisplayedMessages == null) {
                LoadMessages(id);
                return;
            }
            MessageViewModel msg = (from m in DisplayedMessages where m.Id == id select m).FirstOrDefault();
            // TODO: искать ещё и в received messages.
            if (msg != null) {
                ScrollToMessageRequested?.Invoke(this, id);
            } else {
                LoadMessages(id);
            }
        }

        private async void LoadMessages(int startMessageId = -1) {
            if (DemoMode.IsEnabled) {
                DemoModeSession ds = DemoMode.GetDemoSessionById(session.Id);
                var messages = ds.Messages.Where(m => m.PeerId == PeerId).ToList();
                DisplayedMessages = new MessagesCollection(MessageViewModel.BuildFromAPI(messages, session, FixState));

                return;
            }

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

                await Task.Delay(32);
                if (startMessageId > 0) ScrollToMessageRequested?.Invoke(this, startMessageId);
                if (startMessageId == -1) {
                    ScrollToMessageRequested?.Invoke(this, Math.Min(InRead, OutRead));
                }
            } catch (Exception ex) {
                Placeholder = PlaceholderViewModel.GetForException(ex, (o) => { LoadMessages(startMessageId); });
            } finally {
                IsLoading = false;
            }
        }

        public async void LoadPreviousMessages() {
            if (DemoMode.IsEnabled || DisplayedMessages?.Count == 0 || IsLoading) return;
            int count = Constants.MessagesCount;

            try {
                Log.Information("LoadPreviousMessages peer: {0}, count: {1}, displayed messages count: {2}", PeerId, count, DisplayedMessages?.Count);
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
                if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) {
                    LoadPreviousMessages();
                }
            } finally {
                IsLoading = false;
            }
        }

        public async void LoadNextMessages() {
            if (DemoMode.IsEnabled || DisplayedMessages?.Count == 0 || IsLoading) return;
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
                if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) {
                    LoadNextMessages();
                }
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

        private async void LongPoll_MessageFlagSet(LongPoll longPoll, int messageId, int flags, int peerId) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                if (flags.HasFlag(128)) { // Удаление сообщения
                    if (messageId > InRead && UnreadMessagesCount > 0) UnreadMessagesCount--;

                    if (ReceivedMessages.LastOrDefault()?.Id == SortId.MinorId && (ChatSettings != null && !ChatSettings.IsDisappearing)) {
                        if (ReceivedMessages.Count > 1) {
                            MessageViewModel prev = ReceivedMessages[ReceivedMessages.Count - 2];
                            UpdateSortId(SortId.MajorId, prev.Id);
                        } else {
                            Log.Warning("Cannot update minor_id after last message is deleted!");
                        }
                    }

                    MessageViewModel msg = ReceivedMessages.Where(m => m.Id == messageId).FirstOrDefault();
                    if (msg != null) ReceivedMessages.Remove(msg);

                    MessageViewModel dmsg = DisplayedMessages?.Where(m => m.Id == messageId).FirstOrDefault();
                    if (dmsg != null) DisplayedMessages.Remove(dmsg);
                }
            });
        }

        private async void LongPoll_MessageReceived(LongPoll longPoll, Message message, int flags) {
            if (message.PeerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                MessageViewModel msg = new MessageViewModel(message, session);
                
                if (!message.IsPartial) {
                    bool isUnread = flags.HasFlag(1) && !flags.HasFlag(8388608);
                    msg.State = isUnread ? MessageVMState.Unread : MessageVMState.Read;
                }

                bool canAddToDisplayedMessages = DisplayedMessages?.LastOrDefault()?.Id == ReceivedMessages.LastOrDefault()?.Id;
                ReceivedMessages.Add(msg);
                if (message.Action != null) ParseActionMessage(message.FromId, message.Action, message.Attachments);
                if (!flags.HasFlag(65536)) UpdateSortId(SortId.MajorId, msg.Id);
                if (msg.SenderId != session.Id) UnreadMessagesCount++;
                if (canAddToDisplayedMessages) {
                    if (DisplayedMessages == null) {
                        DisplayedMessages = new MessagesCollection(new List<MessageViewModel>() { msg });
                    } else {
                        DisplayedMessages.Insert(msg);
                    }
                }

                // Remove user from activity status
                var status = ActivityStatusUsers.RegisteredObjects.Where(m => m.MemberId == message.FromId).FirstOrDefault();
                if (status != null) ActivityStatusUsers.Remove(status);
            });
        }

        private async void LongPoll_MessageEdited(LongPoll longPoll, Message message, int flags) {
            if (PeerId != message.PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(async () => {
                if (LastMessage?.Id == message.Id) {
                    // нужно для корректной обработки смены фото чата.
                    if (message.Action != null) ParseActionMessage(message.FromId, message.Action, message.Attachments);

                    await Task.Delay(16); // ибо первым выполняется событие в объекте сообщения, и только потом тут.
                    OnPropertyChanged(nameof(LastMessage));
                }
            });
        }

        private void ParseActionMessage(int fromId, VKAPILib.Objects.Action action, List<Attachment> attachments) {
            switch (action.Type) {
                case "chat_title_update":
                    Title = action.Text;
                    break;
                case "chat_photo_update":
                    if (attachments != null) Avatar = attachments[0].Photo.GetSizeAndUriForThumbnail().Uri;
                    break;
                case "chat_photo_remove":
                    Avatar = new Uri("https://vk.com/images/icons/im_multichat_200.png");
                    break;
                case "chat_pin_message":
                    UpdatePinnedMessage(action.ConversationMessageId);
                    break;
                case "chat_unpin_message":
                    PinnedMessage = null;
                    break;
            }
        }

        private async void UpdatePinnedMessage(int cmid) {
            var msg = ReceivedMessages.Where(m => m.ConversationMessageId == cmid).FirstOrDefault();
            if (msg == null) msg = DisplayedMessages.Where(m => m.ConversationMessageId == cmid).FirstOrDefault();
            if (msg != null) {
                PinnedMessage = msg;
            } else {
                try {
                    var resp = await session.API.Messages.GetByConversationMessageIdAsync(session.GroupId, PeerId, new List<int> { cmid });
                    PinnedMessage = new MessageViewModel(resp.Items[0], session);
                } catch (Exception ex) {
                    Log.Error(ex, $"Cannot get pinned message from event! peer={PeerId} cmid={cmid}");
                }
            }
        }

        private async void LongPoll_MentionReceived(LongPoll longPoll, int peerId, int messageId, bool isSelfDestruct) {
            if (PeerId != peerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                if (!isSelfDestruct) {
                    if (Mentions == null) {
                        Mentions = new ObservableCollection<int>() { messageId };
                    } else {
                        Mentions.Add(messageId);
                    }
                }
            });
        }

        private async void LongPoll_IncomingMessagesRead(LongPoll longPoll, int peerId, int messageId, int count) {
            if (PeerId != peerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                InRead = messageId;
                LongPoll_MessagesRead(longPoll, peerId, messageId, count);
            });
        }

        private async void LongPoll_OutgoingMessagesRead(LongPoll longPoll, int peerId, int messageId, int count) {
            if (PeerId != peerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                OutRead = messageId;
                LongPoll_MessagesRead(longPoll, peerId, messageId, count);
            });
        }

        private void LongPoll_MessagesRead(LongPoll longPoll, int peerId, int messageId, int count) {
            UnreadMessagesCount = count;

            if (Mentions != null && Mentions.Count > 0) {
                var mentions = Mentions.ToList();
                foreach (int id in mentions) {
                    if (id <= messageId) Mentions.Remove(id);
                }
                if (Mentions.Count == 0) Mentions = null;
            }
        }

        private async void LongPoll_ConversationFlagReset(LongPoll longPoll, int peerId, int flags) {
            if (PeerId != peerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                bool mention = flags.HasFlag(1024); // Упоминаний больше нет
                bool mark = flags.HasFlag(16384); // Маркированного сообщения больше нет
                if (mark) {
                    HasMention = false;
                    HasSelfDestructMessage = false;
                }
            });
        }

        private async void LongPoll_ConversationFlagSet(LongPoll longPoll, int peerId, int flags) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                bool mention = flags.HasFlag(1024); // Наличие упоминания
                bool mark = flags.HasFlag(16384); // Наличие маркированного сообщения
                if (mark) {
                    if (mention) {
                        HasMention = true;
                    } else {
                        HasSelfDestructMessage = true;
                    }
                }
            });
        }

        private async void LongPoll_ConversationRemoved(object sender, int peerId) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                DisplayedMessages?.Clear();
                ReceivedMessages.Clear();
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

        private async void LongPoll_ConversationDataChanged(LongPoll longPoll, int type, int peerId, int extra) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                // TODO: 4 и 6
                switch (type) {
                    case 3: // Назначен новый администратор
                        if (ChatSettings.AdminIDs == null) ChatSettings.AdminIDs = new List<int>();
                        ChatSettings.AdminIDs?.Add(extra);
                        break;
                    case 7: // Выход из беседы
                    case 8: // Исключение из беседы
                        if (extra > 0) {
                            User user = MembersUsers.Where(u => u.Id == extra).FirstOrDefault();
                            if (user != null) MembersUsers.Remove(user);
                        } else if (extra < 0) {
                            Group group = MembersGroups.Where(g => g.Id == -extra).FirstOrDefault();
                            if (group != null) MembersGroups.Remove(group);
                        }
                        if (extra == session.Id) {
                            ChatSettings.State = type == 8 ? UserStateInChat.Kicked : UserStateInChat.Left;
                        }
                        ChatSettings.MembersCount--;
                        UpdateSubtitleForChat();
                        UpdateRestrictionInfo();
                        break;
                    case 9: // Разжалован администратор
                        if (ChatSettings.AdminIDs != null && ChatSettings.AdminIDs.Contains(extra)) ChatSettings.AdminIDs?.Remove(extra);
                        break;
                }
            });
        }

        private async void LongPoll_ActivityStatusChanged(LongPoll longPoll, int peerId, List<LongPollActivityInfo> infos) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                double timeout = 7000;
                try {
                    foreach (LongPollActivityInfo info in infos) {
                        if (info.MemberId == session.Id) continue;
                        var exist = ActivityStatusUsers.RegisteredObjects.Where(u => u.MemberId == info.MemberId).FirstOrDefault();
                        if (exist != null) ActivityStatusUsers.Remove(exist);
                        ActivityStatusUsers.Add(info, timeout);
                    }
                    UpdateActivityStatus();
                } catch (Exception ex) {
                    ActivityStatusUsers.Clear();
                    ActivityStatus = String.Empty;
                    Log.Error(ex, $"Error while parsing user activity status!");
                }
            });
        }

        private void UpdateActivityStatus() {
            try {
                var acts = ActivityStatusUsers.RegisteredObjects;
                int count = acts.Count();

                Debug.WriteLine($"UpdateActivityStatus: {String.Join(";", acts)}");

                if (count == 0) {
                    ActivityStatus = String.Empty;
                    return;
                }

                if (PeerType != PeerType.Chat) {
                    if (count == 1) {
                        ActivityStatus = GetLocalizedActivityStatus(acts.FirstOrDefault().Status, 1) + "...";
                    }
                } else {
                    var typing = acts.Where(a => a?.Status == LongPollActivityType.Typing).ToList();
                    var voice = acts.Where(a => a?.Status == LongPollActivityType.RecordingAudioMessage).ToList();
                    var photo = acts.Where(a => a?.Status == LongPollActivityType.UploadingPhoto).ToList();
                    var video = acts.Where(a => a?.Status == LongPollActivityType.UploadingVideo).ToList();
                    var file = acts.Where(a => a?.Status == LongPollActivityType.UploadingFile).ToList();
                    List<List<LongPollActivityInfo>> groupedActivities = new List<List<LongPollActivityInfo>> {
                        typing, voice, photo, video, file
                    };

                    bool has3AndMoreDifferentTypes = groupedActivities.Where(a => a.Count > 0).Count() >= 3;

                    string status = String.Empty;
                    foreach (var act in groupedActivities) {
                        if (act.Count == 0) continue;
                        var type = act[0].Status;
                        string actstr = GetLocalizedActivityStatus(type, act.Count);

                        if (has3AndMoreDifferentTypes) {
                            if (status.Length != 0) status += ", ";
                            status += $"{act.Count} {actstr}";
                        } else {
                            if (status.Length != 0) status += ", ";
                            List<int> ids = act.Select(s => s.MemberId).ToList();
                            status += $"{GetNamesForActivityStatus(ids, act.Count, act.Count == 1)} {actstr}";
                        }
                    }

                    ActivityStatus = status.Trim() + "...";
                }
            } catch (Exception ex) {
                ActivityStatus = String.Empty;
                string count = ActivityStatusUsers.RegisteredObjects == null ? "null" : ActivityStatusUsers.RegisteredObjects.Count.ToString();
                Log.Error(ex, $"Exception in UpdateActivityStatus (0x{ex.HResult.ToString("x8")}), current au count: {count}");
            }
        }

        private string GetLocalizedActivityStatus(LongPollActivityType status, int count) {
            string suffix = count == 1 ? "_single" : "_multi";
            switch (status) {
                case LongPollActivityType.Typing: return Localizer.Instance[$"lp_act_typing{suffix}"];
                case LongPollActivityType.RecordingAudioMessage: return Localizer.Instance[$"lp_act_voice{suffix}"];
                case LongPollActivityType.UploadingPhoto: return Localizer.Instance[$"lp_act_photo{suffix}"];
                case LongPollActivityType.UploadingVideo: return Localizer.Instance[$"lp_act_video{suffix}"];
                case LongPollActivityType.UploadingFile: return Localizer.Instance[$"lp_act_file{suffix}"];
            }
            return String.Empty;
        }

        private string GetNamesForActivityStatus(IReadOnlyList<int> ids, int count, bool showFullLastName) {
            string r = String.Empty;
            foreach (int id in ids) {
                if (id > 0) {
                    User u = CacheManager.GetUser(id);
                    if (u != null) {
                        string lastName = showFullLastName ? u.LastName : u.LastName[0] + ".";
                        r = $"{u.FirstName} {lastName}";
                    }
                } else if (id < 0) {
                    var g = CacheManager.GetGroup(id);
                    if (g != null) {
                        r = $"\"{g.Name}\"";
                    }
                }
            }
            if (!String.IsNullOrEmpty(r)) {
                if (count > 1) {
                    r += $" {Localizer.Instance.GetFormatted("im_status_more", count - 1)}";
                }
            }
            return r;
        }

        private async void LongPoll_NotificationsSettingsChanged(object sender, LongPollPushNotificationData e) {
            if (e.PeerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                PushSettings ps = new PushSettings {
                    DisabledForever = e.DisabledUntil == -1,
                    DisabledUntil = e.DisabledUntil,
                    NoSound = e.Sound == 0
                };
                PushSettings = ps;
            });
        }

        #endregion

        private ulong GetSortIndex() {
            if (SortId.MajorId == 0) return (ulong)SortId.MinorId;
            ulong index = ((ulong)SortId.MajorId * 100000000) + (ulong)SortId.MinorId;
            return index;
        }
    }
}