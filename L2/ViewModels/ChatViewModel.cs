using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Threading;
using ELOR.Laney.Collections;
using ELOR.Laney.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using ELOR.Laney.Execute;
using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToastNotifications.Avalonia;
using VKUI.Controls;

namespace ELOR.Laney.ViewModels {
    public sealed class ChatViewModel : CommonViewModel, IMessagesListHolder {
        private static uint _instances = 0;
        private VKSession session;

        private PeerType _peerType;
        private long _peerId;
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
        private bool _isFavoritesChat;
        private ObservableCollection<int> _mentions;
        private bool _hasMention;
        private bool _hasSelfDestructMessage;
        private ObservableCollection<int> _unreadReactions;
        private string _restrictionReason;
        private bool _isCurrentOpenedChat;
        private int _selectedMessagesCount;
        private ObservableCollection<Command> _messagesCommands = new ObservableCollection<Command>();
        private RelayCommand _openProfileCommand;
        private RelayCommand _goToLastMessageCommand;
        private RelayCommand _goToLastReactedMessageCommand;

        public PeerType PeerType { get { return _peerType; } private set { _peerType = value; OnPropertyChanged(); } }
        public long PeerId { get { return _peerId; } private set { _peerId = value; OnPropertyChanged(); } }
        public string Title { get { return _title; } private set { _title = value; OnPropertyChanged(); OnPropertyChanged(nameof(Initials)); } }
        public string Subtitle { get { return _subtitle; } private set { _subtitle = value; OnPropertyChanged(); } }
        public string ActivityStatus { get { return _activityStatus; } set { _activityStatus = value; OnPropertyChanged(); } }
        public Uri Avatar { get { return _avatar; } private set { _avatar = value; OnPropertyChanged(); } }
        public bool IsVerified { get { return _isVerified; } private set { _isVerified = value; OnPropertyChanged(); } }
        public UserOnlineInfo Online { get { return _online; } set { _online = value; OnPropertyChanged(); } }
        public string Initials { get { return _title.GetInitials(PeerId.IsChat() || PeerId.IsGroup()); } }
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
        public bool IsFavoritesChat { get { return _isFavoritesChat; } private set { _isFavoritesChat = value; OnPropertyChanged(); } }
        public ObservableCollection<int> Mentions { get { return _mentions; } private set { _mentions = value; OnPropertyChanged(); } }
        public bool HasMention { get { return _hasMention; } private set { _hasMention = value; OnPropertyChanged(); } }
        public bool HasSelfDestructMessage { get { return _hasSelfDestructMessage; } private set { _hasSelfDestructMessage = value; OnPropertyChanged(); } }
        public string MentionIconId { get { return GetMentionIcon(); } }
        public ObservableCollection<int> UnreadReactions { get { return _unreadReactions; } set { _unreadReactions = value; OnPropertyChanged(); } }
        public string RestrictionReason { get { return _restrictionReason; } private set { _restrictionReason = value; OnPropertyChanged(); } }
        public bool IsCurrentOpenedChat { get { return _isCurrentOpenedChat; } private set { _isCurrentOpenedChat = value; OnPropertyChanged(); } }
        public int SelectedMessagesCount { get { return _selectedMessagesCount; } private set { _selectedMessagesCount = value; OnPropertyChanged(); } }
        public ObservableCollection<Command> MessagesCommands { get { return _messagesCommands; } private set { _messagesCommands = value; OnPropertyChanged(); } }
        public RelayCommand OpenProfileCommand { get { return _openProfileCommand; } private set { _openProfileCommand = value; OnPropertyChanged(); } }
        public RelayCommand GoToLastMessageCommand { get { return _goToLastMessageCommand; } private set { _goToLastMessageCommand = value; OnPropertyChanged(); } }
        public RelayCommand GoToLastReactedMessageCommand { get { return _goToLastReactedMessageCommand; } private set { _goToLastReactedMessageCommand = value; OnPropertyChanged(); } }


        public SelectionModel<MessageViewModel> SelectedMessages { get; } = new SelectionModel<MessageViewModel> {
            SingleSelect = false
        };

        public List<ChatMember> Members { get; private set; } = new List<ChatMember>();
        public List<User> MembersUsers { get; private set; } = new List<User>();
        public List<Group> MembersGroups { get; private set; } = new List<Group>();

        public long Id => PeerId;
        public long OwnedSessionId => session.Id;
        public static uint Instances => _instances;

        private User PeerUser;
        private Group PeerGroup;

        public event EventHandler<IMessageListItem> ScrollToMessageRequested;
        public event EventHandler<bool> MessagesChunkLoaded; // получение сообщений (false - предыдущих, true - следующих)
        public EventHandler<MessageViewModel> MessageAddedToLast;

        Elapser<LongPollActivityInfo> ActivityStatusUsers = new Elapser<LongPollActivityInfo>();

        public ChatViewModel(VKSession session, long peerId, Message lastMessage = null, bool needSetup = false) {
            _instances++;
            int cmid = lastMessage != null ? lastMessage.ConversationMessageId : 0;
            Log.Verbose($"New ChatViewModel for peer {peerId}. Last message: {cmid}, need setup: {needSetup}");

            this.session = session;
            Composer = new ComposerViewModel(session, this);
            SetUpEvents();
            PeerId = peerId;
            Title = peerId.ToString();
            MessageViewModel msg = null;
            if (lastMessage != null) {
                msg = MessageViewModel.Create(lastMessage, session);
                if (!lastMessage.IsPartial) FixState(msg);
                if (SortId == null) SortId = new SortId { MajorId = 0, MinorId = lastMessage.Id };
                ReceivedMessages.Add(msg);
            }
            // needSetup нужен в случае, когда мы не переходим в беседу и не загружаем сообщения,
            // но надо загрузить инфу о чате, которую можно получить при загрузке сообщений.
            if (needSetup) new System.Action(async () => await GetInfoFromAPIAndSetupAsync(lastMessage, msg))();
        }

        public ChatViewModel(VKSession session, Conversation c, Message lastMessage = null) {
            _instances++;
            Log.Verbose($"New ChatViewModel for conversation with peer {c.Peer.Id}. Last message: {lastMessage?.ConversationMessageId}");

            this.session = session;
            Composer = new ComposerViewModel(session, this);
            SetUpEvents();
            Setup(c);
            if (lastMessage != null) {
                MessageViewModel msg = MessageViewModel.Create(lastMessage, session);
                FixState(msg);
                ReceivedMessages.Add(msg);
            }
        }

        ~ChatViewModel() {
            _instances--;
        }

        // Вызывается при отображении беседы на окне
        public void OnDisplayed(int messageId = -1) {
            bool isDisplayedMessagesEmpty = DisplayedMessages == null || DisplayedMessages.Count == 0;
            Log.Information("Chat {0} is opened. isDisplayedMessagesEmpty: {1}, CMID: {2}", PeerId, isDisplayedMessagesEmpty, messageId);
            if (isDisplayedMessagesEmpty || messageId >= 0) {
                new System.Action(async () => await GoToMessageAsync(messageId))();
            }
        }

        private async Task GetInfoFromAPIAndSetupAsync(Message message, MessageViewModel msg) {
            try {
                var response = await session.API.Messages.GetConversationsByIdAsync(session.GroupId, new List<long> { PeerId }, true, VKAPIHelper.Fields);
                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);
                Setup(response.Items.FirstOrDefault());

                // Если чат новый, то нам надо отобразить уведомление о новом сообщении,
                // т. к. обычно уведомления отправляет метод LongPoll_MessageReceived.
                if (message == null || msg == null) return;

                bool isMention = false;
                if (!message.IsSilent && message.MentionedUsers != null) {
                    if (message.MentionedUsers.Count == 0) { // признак того, что пушнули всех (@all)
                        isMention = true;
                    } else {
                        isMention = message.MentionedUsers.Contains(session.Id);
                    }
                }

                // Если сообщение неполное даже после получения инфы о чате, то добавляем сообщение в pending.
                if (msg.State == MessageVMState.Loading) {
                    Log.Information($"Adding message {message.PeerId}_{message.ConversationMessageId} to pending for notification... (by new chat)");
                    if (!message.IsSilent) pendingMessages.Add(message.ConversationMessageId, isMention);
                } else {
                    if (!message.IsSilent) await ShowSystemNotificationAsync(msg, isMention);
                }
            } catch (Exception ex) {
                Log.Error(ex, $"Cannot get conversation from API! Peer={PeerId}");
            }
        }

        bool _isConvoRefreshing = false;
        private async Task RefreshConvoInfoAsync(Conversation convo, bool needRefreshChatMembers = false) {
            if (_isConvoRefreshing) return;
            _isConvoRefreshing = true;
            try {
                Log.Information($"RefreshConvoInfo peer: {PeerId}, needRefreshChatMembers: {needRefreshChatMembers}");
                if (convo != null) {
                    Setup(convo);
                } else {
                    var response = await session.API.Messages.GetConversationsByIdAsync(session.GroupId, new List<long> { PeerId }, true, VKAPIHelper.Fields);
                    if (response.Items.Count > 0) Setup(response.Items[0]);
                }

                if (needRefreshChatMembers) await LoadMembersAsync();
            } catch (Exception ex) {
                await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
            } finally {
                _isConvoRefreshing = false;
            }
        }

        bool _isMembersLoading = false;
        private async Task LoadMembersAsync() {
            if (ChatSettings.State != UserStateInChat.In || _isMembersLoading) return;

            _isMembersLoading = true;
            try {
                Log.Information($"LoadMembers peer: {PeerId}");
                var response = await session.API.Messages.GetConversationMembersAsync(session.GroupId, PeerId, extended: true, fields: VKAPIHelper.Fields);
                Members = response.Items;

                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);

                MembersUsers = response.Profiles;
                MembersGroups = response.Groups;
            } catch (Exception ex) {
                await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
            } finally {
                _isMembersLoading = false;
            }
        }

        private void Setup(Conversation c) {
            PeerId = c.Peer.Id;
            if (SortId?.MajorId != c.SortId.MajorId || SortId?.MinorId != c.SortId.MinorId) SortId = c.SortId; // чтобы не дёргался listbox.
            UnreadMessagesCount = c.UnreadCount;
            CanWrite = c.CanWrite;
            InRead = c.InReadCMID;
            OutRead = c.OutReadCMID;
            PushSettings = c.PushSettings;
            IsMarkedAsUnread = c.IsMarkedUnread;
            IsPinned = SortId.MajorId > 0 && SortId.MajorId % 16 == 0;

            if (PushSettings == null) PushSettings = new PushSettings {
                NoSound = false,
                DisabledForever = false,
                DisabledUntil = 0
            };

            if (c.UnreadReactions != null && c.UnreadReactions.Count > 0) UnreadReactions = new ObservableCollection<int>(c.UnreadReactions);
            if (c.Mentions != null && c.Mentions.Count > 0) {
                Mentions = new ObservableCollection<int>(c.Mentions);
                HasMention = true;
            }
            if (c.ExpireConvMessageIds != null && c.ExpireConvMessageIds.Count > 0) {
                HasSelfDestructMessage = true;
            }
            if (c.CurrentKeyboard != null && c.CurrentKeyboard.Buttons.Count > 0) Composer.BotKeyboard = c.CurrentKeyboard;

            if (PeerId.IsUser()) { // User
                PeerType = PeerType.User;
                PeerUser = CacheManager.GetUser(PeerId);
                IsFavoritesChat = PeerId == session.Id;
                if (IsFavoritesChat) {
                    Title = Assets.i18n.Resources.favorites;
                    Avatar = new Uri("https://vk.com/images/icons/im_favorites_200.png");
                    // make favorites offline
                    var tmp = PeerUser.OnlineInfo;
                    tmp.IsMobile = false;
                    tmp.IsOnline = false;
                    Online = tmp;
                } else {
                    Title = string.Intern(PeerUser.FullName);
                    Avatar = new Uri(PeerUser.Photo200);
                    Online = PeerUser.OnlineInfo;
                }
                IsVerified = PeerUser.Verified == 1;
            } else if (PeerId.IsGroup()) { // Group
                PeerType = PeerType.Group;
                PeerGroup = CacheManager.GetGroup(PeerId);
                Title = string.Intern(PeerGroup.Name);
                Avatar = new Uri(PeerGroup.Photo200);
                IsVerified = PeerGroup.Verified == 1;
                Subtitle = PeerGroup.Activity?.ToLowerInvariant();
            } else if (PeerId.IsChat()) { // Chat
                PeerType = PeerType.Chat;
                ChatSettings = c.ChatSettings;
                Title = string.Intern(ChatSettings.Title);
                Avatar = ChatSettings?.Photo?.Uri;
                if (ChatSettings.PinnedMessage != null)
                    PinnedMessage = MessageViewModel.Create(ChatSettings.PinnedMessage, session);
                UpdateSubtitleForChat();
            } else if (PeerId.IsContact()) { // Contact?
                PeerType = PeerType.Contact;
                Title = $"Contact {PeerId}";
            } else if (PeerId.IsEmail()) { // E-mail
                PeerType = PeerType.Email;
                Title = $"E-Mail {PeerId}";
            }

            // Checking and displaying activity status
            if (DemoMode.IsEnabled) {
                var ds = DemoMode.GetDemoSessionById(session.Id);
                if (ds.ActivityStatuses.ContainsKey(PeerId.ToString())) {
                    foreach (var status in ds.ActivityStatuses[PeerId.ToString()]) {
                        ActivityStatusUsers.Add(status, 1000 * 3600);
                    }
                    UpdateActivityStatus();
                }
                OpenProfileCommand = new RelayCommand((o) => { });
            } else {
                OpenProfileCommand = new RelayCommand(OpenPeerProfile);
                GoToLastMessageCommand = new RelayCommand(async (o) => await GoToLastMessageAsync());
                GoToLastReactedMessageCommand = new RelayCommand(async (o) => await GoToLastReactedMessageAsync());
            }

            UpdateRestrictionInfo();
        }

        private void UpdateSubtitleForChat() {
            if (ChatSettings.State == UserStateInChat.In) {
                Subtitle = String.Empty;
                if (ChatSettings.IsDisappearing) Subtitle = $"{Assets.i18n.Resources.casper_chat.ToLowerInvariant()}, ";
                Subtitle += Localizer.GetDeclensionFormatted(ChatSettings.MembersCount, "members_sub");
            } else {
                Subtitle = ChatSettings.State == UserStateInChat.Left ? Assets.i18n.Resources.chat_left.ToLowerInvariant() : Assets.i18n.Resources.chat_kicked.ToLowerInvariant();
            }
        }

        private void UpdateRestrictionInfo() {
            if (CanWrite.Allowed) {
                RestrictionReason = String.Empty; return;
            }

            if (PeerType == PeerType.Chat) {
                if (ChatSettings.State != UserStateInChat.In) {
                    RestrictionReason = Localizer.Get($"chat_{ChatSettings.State.ToString().ToLower()}");
                } else {
                    RestrictionReason = VKAPIHelper.GetUnderstandableErrorMessage(CanWrite.Reason, Assets.i18n.Resources.cannot_write);
                }
            } else if (PeerType == PeerType.User) {
                switch (CanWrite.Reason) {
                    case 18:
                        if (PeerUser.Deactivated == DeactivationState.Deleted) RestrictionReason = Assets.i18n.Resources.user_deleted;
                        if (PeerUser.Deactivated == DeactivationState.Banned) RestrictionReason = Assets.i18n.Resources.user_blocked;
                        break;
                    case 900:
                        if (PeerUser.Blacklisted == 1) RestrictionReason = Localizer.Get("user_blacklisted", PeerUser.Sex);
                        if (PeerUser.BlacklistedByMe == 1) RestrictionReason = Localizer.Get("user_blacklisted_by_me", PeerUser.Sex);
                        break;
                    default:
                        RestrictionReason = VKAPIHelper.GetUnderstandableErrorMessage(CanWrite.Reason, Assets.i18n.Resources.cannot_write);
                        break;
                }
            } else if (PeerType == PeerType.Group) {
                RestrictionReason = VKAPIHelper.GetUnderstandableErrorMessage(CanWrite.Reason, Assets.i18n.Resources.cannot_write);
            }
        }

        private string GetMentionIcon() {
            if (HasSelfDestructMessage) return VKIconNames.Icon12Bomb;
            if (HasMention) return VKIconNames.Icon12Mention;
            return null;
        }

        bool eventsAlreadySetup = false;
        private void SetUpEvents() {
            if (eventsAlreadySetup) return;
            eventsAlreadySetup = true;
            // При приёме сообщения обновляем последнее сообщение.
            ReceivedMessages.CollectionChanged += (a, b) => OnPropertyChanged(nameof(LastMessage));
            SelectedMessages.SelectionChanged += SelectedMessages_SelectionChanged;

            PropertyChanged += (a, b) => {
                if (b.PropertyName == nameof(Online)) {
                    // make an empty subtitle if it is favorites
                    if (PeerId == session.Id) Subtitle = Assets.i18n.Resources.saved_messages;
                    else Subtitle = VKAPIHelper.GetOnlineInfo(Online, PeerUser.Sex).ToLowerInvariant();
                }

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
                session.LongPoll.UnreadReactionsChanged += LongPoll_UnreadReactionsChanged;

                if (!session.IsGroup) VKQueue.Online += VKQueue_Online;
            }

            ActivityStatusUsers.Elapsed += ActivityStatusUsers_Elapsed;
        }

        private void ActivityStatusUsers_Elapsed(object sender, LongPollActivityInfo e) {
            UpdateActivityStatus();
        }

        private void SelectedMessages_SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs<MessageViewModel> e) {
            SelectedMessagesCount = SelectedMessages.Count;
            MessagesCommands.Clear();
            if (SelectedMessagesCount > 0) {
                Command reply = new Command(VKIconNames.Icon24ReplyOutline, Assets.i18n.Resources.reply, false, ReplyToMessageCommand);
                Command fwdhere = new Command(VKIconNames.Icon24ReplyOutline, Assets.i18n.Resources.forward_here, false, ForwardHereCommand);
                Command forward = new Command(VKIconNames.Icon24ShareOutline, Assets.i18n.Resources.forward, false, ForwardCommand);

                bool isChannel = ChatSettings != null && ChatSettings.IsGroupChannel;
                if (!isChannel) MessagesCommands.Add(SelectedMessagesCount == 1 ? reply : fwdhere);
                MessagesCommands.Add(forward);
            }
        }

        #region Commands

        private void OpenPeerProfile(object o) {
            new System.Action(async () => await Router.OpenPeerProfileAsync(session, PeerId))();
        }

        private async Task GoToLastMessageAsync() {
            if (IsLoading) return;

            if (DisplayedMessages?.Last?.ConversationMessageId == LastMessage?.ConversationMessageId) {
                await GoToMessageAsync(LastMessage);
            } else {
                Log.Information($"GoToLastMessage: last message in chat is not displayed. Showing ReceivedMessages...");
                if (ReceivedMessages.Count > 10) {
                    DisplayedMessages = new MessagesCollection(ReceivedMessages.ToList());
                    ScrollToMessageRequested?.Invoke(this, LastMessage);
                    if (ReceivedMessages.Count < 20) {
                        Log.Information($"GoToLastMessage: need get more messages from API to display.");
                        MessagesChunkLoaded += PrevMessagesLoaded;
                        await LoadPreviousMessagesAsync(null);
                    }
                } else {
                    await GoToMessageAsync(LastMessage);
                }
            }
        }

        private async Task GoToLastReactedMessageAsync() {
            if (IsLoading) return;
            if (UnreadReactions != null && UnreadReactions.Count > 0) await GoToMessageAsync(UnreadReactions.LastOrDefault());
        }

        private void PrevMessagesLoaded(object sender, bool next) {
            if (next) return;
            MessagesChunkLoaded -= PrevMessagesLoaded;
            ScrollToMessageRequested?.Invoke(this, LastMessage);
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
            Composer.AddForwardedMessages(PeerId, SelectedMessages.SelectedItems.ToList(), session.GroupId);
            SelectedMessages.Clear();
        }

        private void ForwardCommand(object o) {
            session.Share(PeerId, SelectedMessages.SelectedItems.ToList());
        }

        public void ShowContextMenuForSelectedMessages(object p) {
            ContextMenuHelper.ShowForMultipleMessages(SelectedMessages.SelectedItems.ToList(), this, (Control)p);
        }

        #endregion

        #region Loading messages

        public async Task GoToMessageAsync(MessageViewModel message) {
            if (message == null) return;
            if (!message.IsUnavailable) {
                await GoToMessageAsync(message.ConversationMessageId);
            } else {
                StandaloneMessageViewer smv = new StandaloneMessageViewer(session, message);
                await smv.ShowDialog(session.Window);
            }
        }

        public async Task GoToMessageAsync(int id) {
            if (id == 0) return;
            if (DisplayedMessages == null) {
                await LoadMessagesAsync(id);
                return;
            }
            MessageViewModel msg = DisplayedMessages.GetById(id);
            // TODO: искать ещё и в received messages.
            if (msg != null) {
                ScrollToMessageRequested?.Invoke(this, msg);
            } else {
                await LoadMessagesAsync(id);
            }
        }

        private async Task LoadMessagesAsync(int startMessageId = -1) {
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
                Log.Information("LoadMessages peer: {0}, count: {1}, startMessageId: {2}", PeerId, count, startMessageId);
                IsLoading = true;
                int offset = -count / 2;

                // TODO: use messages.getHistory, т. к. участников получаем сразу после первой загрузки сообщений.
                MessagesHistoryEx mhr = await session.API.GetHistoryWithMembersAsync(session.GroupId, PeerId, offset, count, startMessageId, false, VKAPIHelper.Fields, true);
                CacheManager.Add(mhr.Profiles);
                CacheManager.Add(mhr.Groups);
                CacheManager.Add(mhr.MentionedProfiles);
                CacheManager.Add(mhr.MentionedGroups);

                Setup(mhr.Conversation);
                mhr.Messages?.Reverse();
                DisplayedMessages = new MessagesCollection(MessageViewModel.BuildFromAPI(mhr.Messages, session, FixState));

                int scrollTo = 0;
                if (startMessageId > 0) scrollTo = startMessageId;
                if (startMessageId == -1) {
                    scrollTo = InRead;
                }
                if (scrollTo > 0) {
                    MessageViewModel scrollToMsg = DisplayedMessages.SingleOrDefault(m => m.ConversationMessageId == scrollTo);
                    if (scrollToMsg != null) {
                        ScrollToMessageRequested?.Invoke(this, scrollToMsg);
                    } else {
                        Log.Warning($"LoadMessages: cannot find message with cmid {scrollTo}, so cannot scroll to this message!");
                    }
                }

                if (Members.Count == 0) await LoadMembersAsync();
            } catch (Exception ex) {
                Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadMessagesAsync(startMessageId));
            } finally {
                IsLoading = false;

                await Task.Delay(2000);
                GC.Collect(2, GCCollectionMode.Aggressive);
            }
        }

        public async Task LoadPreviousMessagesAsync(CancellationToken? ct) {
            if (DemoMode.IsEnabled || DisplayedMessages?.Count == 0 || IsLoading) return;
            int count = Constants.MessagesCount;

            try {
                Log.Information("LoadPreviousMessages peer: {0}, count: {1}, displayed messages count: {2}", PeerId, count, DisplayedMessages?.Count);
                IsLoading = true;
                MessagesHistoryEx mhr = await session.API.GetHistoryWithMembersAsync(session.GroupId, PeerId, 1, count, DisplayedMessages.First.ConversationMessageId, false, VKAPIHelper.Fields, true);
                CacheManager.Add(mhr.MentionedProfiles);
                CacheManager.Add(mhr.MentionedGroups);
                mhr.Messages.Reverse();
                MessagesChunkLoaded?.Invoke(this, false);
                DisplayedMessages.InsertRange(mhr.Messages.Select(m => {
                    var msg = MessageViewModel.Create(m, session);
                    FixState(msg);
                    return msg;
                }).ToList());
            } catch (Exception ex) {
                if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) {
                    await LoadPreviousMessagesAsync(ct);
                }
            } finally {
                IsLoading = false;
            }
        }

        public async Task LoadNextMessagesAsync(CancellationToken? ct) {
            if (DemoMode.IsEnabled || DisplayedMessages?.Count == 0 || IsLoading) return;
            if (LastMessage?.ConversationMessageId == DisplayedMessages.LastOrDefault()?.ConversationMessageId) return;
            int count = Constants.MessagesCount;

            try {
                Log.Information("LoadNextMessages peer: {0}, count: {1}, displayed messages count: {2}", PeerId, count, DisplayedMessages.Count);
                IsLoading = true;
                MessagesHistoryEx mhr = await session.API.GetHistoryWithMembersAsync(session.GroupId, PeerId, -count, count, DisplayedMessages.Last.ConversationMessageId, false, VKAPIHelper.Fields, false);
                CacheManager.Add(mhr.MentionedProfiles);
                CacheManager.Add(mhr.MentionedGroups);
                mhr.Messages.Reverse();
                MessagesChunkLoaded?.Invoke(this, true);
                DisplayedMessages.InsertRange(mhr.Messages.Select(m => {
                    var msg = MessageViewModel.Create(m, session);
                    FixState(msg);
                    return msg;
                }).ToList());
            } catch (Exception ex) {
                if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) {
                    await LoadNextMessagesAsync(ct);
                }
            } finally {
                IsLoading = false;
            }
        }

        private void FixState(MessageViewModel msg) {
            long senderId = session.Id;
            bool isOutgoing = msg.SenderId == senderId;
            if (isOutgoing) {
                msg.State = msg.ConversationMessageId > OutRead ? MessageVMState.Unread : MessageVMState.Read;
            } else {
                msg.State = msg.ConversationMessageId > InRead ? MessageVMState.Unread : MessageVMState.Read;
            }
        }

        #endregion

        #region LongPoll events

        private void LongPoll_MessageFlagSet(LongPoll longPoll, int messageId, int flags, long peerId) {
            if (peerId != PeerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    if (flags.HasFlag(128)) { // Удаление сообщения
                        if (messageId > InRead && UnreadMessagesCount > 0) UnreadMessagesCount--;
                        if (ReceivedMessages.Count > 1) {
                            MessageViewModel prev = ReceivedMessages[ReceivedMessages.Count - 2];
                            UpdateSortId(SortId.MajorId, prev.GlobalId);
                        } else {
                            Log.Warning("Cannot update minor_id after last message is deleted!");
                        }

                        MessageViewModel msg = ReceivedMessages.Where(m => m.ConversationMessageId == messageId).FirstOrDefault();
                        if (msg != null) ReceivedMessages.Remove(msg);

                        MessageViewModel dmsg = DisplayedMessages?.GetById(messageId);
                        if (dmsg != null) DisplayedMessages.Remove(dmsg);
                    }
                });
            })();
        }

        Dictionary<int, bool> pendingMessages = new Dictionary<int, bool>();

        private void LongPoll_MessageReceived(LongPoll longPoll, Message message, int flags) {
            if (message.PeerId != PeerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    MessageViewModel msg = MessageViewModel.Create(message, session);

                    bool isMention = false;
                    if (!message.IsSilent && message.MentionedUsers != null) {
                        if (message.MentionedUsers.Count == 0) { // признак того, что пушнули всех (@all)
                            isMention = true;
                        } else {
                            isMention = message.MentionedUsers.Contains(session.Id);
                        }
                    }

                    if (!message.IsPartial) {
                        bool isUnread = flags.HasFlag(1) && !flags.HasFlag(8388608);
                        msg.State = isUnread ? MessageVMState.Unread : MessageVMState.Read;
                        if (!message.IsSilent) await ShowSystemNotificationAsync(msg, isMention);
                    } else {
                        if (!message.IsSilent) {
                            Log.Information($"Adding message {message.PeerId}_{message.ConversationMessageId} to pending for notification... (by longpoll)");
                            pendingMessages.Add(message.ConversationMessageId, isMention);
                        }
                    }

                    bool canAddToDisplayedMessages = DisplayedMessages?.Last?.ConversationMessageId == ReceivedMessages.LastOrDefault()?.ConversationMessageId;

                    if (ReceivedMessages.Count >= Constants.MessagesCount) {
                        var oldReceived = _receivedMessages;
                        _receivedMessages = new ObservableCollection<MessageViewModel> { msg };
                        oldReceived.Clear();
                        oldReceived = null;
                        Log.Information($"All received messages except the last one is removed from cache in chat {Id}");
                    } else {
                        ReceivedMessages.Add(msg);
                    }

                    // if (message.Action != null) ParseActionMessage(message.FromId, message.Action, message.Attachments);
                    // if (!flags.HasFlag(65536)) UpdateSortId(SortId.MajorId, msg.Id);
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
            })();
        }

        private void LongPoll_MessageEdited(LongPoll longPoll, Message message, int flags) {
            if (PeerId != message.PeerId) return;
            bool isFullReceived = pendingMessages.ContainsKey(message.ConversationMessageId);

            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    if (isFullReceived) {
                        bool isMention = pendingMessages[message.ConversationMessageId];
                        pendingMessages.Remove(message.ConversationMessageId);
                        MessageViewModel msg = ReceivedMessages.Where(m => m.ConversationMessageId == message.ConversationMessageId).FirstOrDefault();
                        if (msg != null) await ShowSystemNotificationAsync(msg, isMention);
                    }

                    if (LastMessage?.ConversationMessageId == message.ConversationMessageId) {
                        // нужно для корректной обработки смены фото чата.
                        //if (message.Action != null) ParseActionMessage(message.FromId, message.Action, message.Attachments);

                        await Task.Delay(16); // ибо первым выполняется событие в объекте сообщения, и только потом тут.
                        OnPropertyChanged(nameof(LastMessage));
                    }
                });
            })();
        }

        //private void ParseActionMessage(long fromId, VKAPILib.Objects.Action action, List<Attachment> attachments) {
        //    switch (action.Type) {
        //        case "chat_title_update":
        //            Title = action.Text;
        //            break;
        //        case "chat_photo_update":
        //            if (attachments != null) Avatar = attachments[0].Photo.GetSizeAndUriForThumbnail(Constants.ChatHeaderAvatarSize, Constants.ChatHeaderAvatarSize).Uri;
        //            break;
        //        case "chat_photo_remove":
        //            Avatar = new Uri("https://vk.com/images/icons/im_multichat_200.png");
        //            break;
        //        case "chat_pin_message":
        //            UpdatePinnedMessage(action.ConversationMessageId);
        //            break;
        //        case "chat_unpin_message":
        //            PinnedMessage = null;
        //            break;
        //    }
        //}

        private void UpdatePinnedMessage(int cmid) {
            var msg = ReceivedMessages.Where(m => m.ConversationMessageId == cmid).FirstOrDefault();
            if (msg == null) msg = DisplayedMessages.GetById(cmid);
            if (msg != null) {
                PinnedMessage = msg;
            } else {
                new System.Action(async () => {
                    try {
                        var resp = await session.API.Messages.GetByConversationMessageIdAsync(session.GroupId, PeerId, new List<int> { cmid });
                        PinnedMessage = MessageViewModel.Create(resp.Items[0], session);
                    } catch (Exception ex) {
                        Log.Error(ex, $"Cannot get pinned message from event! peer={PeerId} cmid={cmid}");
                    }
                })();
            }
        }

        private void LongPoll_MentionReceived(LongPoll longPoll, long peerId, int messageId, bool isSelfDestruct) {
            if (PeerId != peerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    if (!isSelfDestruct) {
                        if (Mentions == null) {
                            Mentions = new ObservableCollection<int>() { messageId };
                        } else {
                            Mentions.Add(messageId);
                        }
                    }
                });
            })();
        }

        private void LongPoll_IncomingMessagesRead(LongPoll longPoll, long peerId, int messageId, int count) {
            if (PeerId != peerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    InRead = messageId;
                    LongPoll_MessagesRead(longPoll, peerId, messageId, count);
                });
            })();
        }

        private void LongPoll_OutgoingMessagesRead(LongPoll longPoll, long peerId, int messageId, int count) {
            if (PeerId != peerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    OutRead = messageId;
                    LongPoll_MessagesRead(longPoll, peerId, messageId, count);
                });
            })();
        }

        private void LongPoll_MessagesRead(LongPoll longPoll, long peerId, int messageId, int count) {
            if (PeerId != peerId) return;
            UnreadMessagesCount = count;

            if (Mentions != null && Mentions.Count > 0) {
                var mentions = Mentions.ToList();
                foreach (int id in mentions) {
                    if (id <= messageId) Mentions.Remove(id);
                }
                if (Mentions.Count == 0) Mentions = null;
            }
        }

        private void LongPoll_ConversationFlagReset(LongPoll longPoll, long peerId, int flags) {
            if (PeerId != peerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    if (flags.HasFlag(1048576)) IsMarkedAsUnread = false;
                    bool mention = flags.HasFlag(1024); // Упоминаний больше нет
                    bool mark = flags.HasFlag(16384); // Маркированного сообщения больше нет
                    if (mark) {
                        HasMention = false;
                        HasSelfDestructMessage = false;
                    }
                });
            })();
        }

        private void LongPoll_ConversationFlagSet(LongPoll longPoll, long peerId, int flags) {
            if (peerId != PeerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    if (flags.HasFlag(1048576)) IsMarkedAsUnread = true;
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
            })();
        }

        private void LongPoll_ConversationRemoved(object sender, long peerId) {
            if (peerId != PeerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    DisplayedMessages?.Clear();
                    ReceivedMessages.Clear();
                });
            })();
        }

        // Если что, flags и есть major/minor_id.
        private void LongPoll_MajorIdChanged(LongPoll longPoll, long peerId, int flags) {
            if (peerId != PeerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    UpdateSortId(flags, SortId.MinorId);
                });
            })();
        }

        private void LongPoll_MinorIdChanged(LongPoll longPoll, long peerId, int flags) {
            if (peerId != PeerId || flags == 0) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    UpdateSortId(SortId.MajorId, flags);
                });
            })();
        }

        // Надо задать новый объект SortId, чтобы сработало событие OnPropertyChanged для SortIndex.
        private void UpdateSortId(int major, int minor) {
            SortId = new SortId {
                MajorId = major,
                MinorId = minor
            };
        }

        private void LongPoll_ConversationDataChanged(LongPoll longPoll, int type, long peerId, long extra, Conversation convo) {
            if (peerId != PeerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    // TODO: type 6
                    switch (type) {
                        case 1: // Измененилось название чата
                        case 2: // Обновилась аватарка чата
                        case 4: // Изменились права доступа в чате
                        case 10: // Изменился баннер (будет поддерживаться в будущем)
                        // case 19: // Начало или окончание звонка
                            await RefreshConvoInfoAsync(convo, type == 4);
                            break;
                        case 3: // Назначен новый администратор
                            if (ChatSettings.AdminIDs == null) ChatSettings.AdminIDs = new List<long>();
                            ChatSettings.AdminIDs?.Add(extra);
                            break;
                        case 5: // Закрепление или открепление сообщения
                            if (extra == 0) {
                                PinnedMessage = null;
                            } else {
                                UpdatePinnedMessage(Convert.ToInt32(extra));
                            }
                            break;
                        case 7: // Выход из беседы
                        case 8: // Исключение из беседы
                            if (extra.IsUser()) {
                                var um = Members.SingleOrDefault(m => m.MemberId == extra);
                                if (um != null) Members.Remove(um);

                                User user = MembersUsers?.Where(u => u.Id == extra).FirstOrDefault();
                                if (user != null) MembersUsers?.Remove(user);
                            } else if (extra.IsGroup()) {
                                var gm = Members.SingleOrDefault(m => m.MemberId == -extra);
                                if (gm != null) Members.Remove(gm);

                                Group group = MembersGroups?.Where(g => g.Id == -extra).FirstOrDefault();
                                if (group != null) MembersGroups?.Remove(group);
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
            })();
        }

        private void LongPoll_ActivityStatusChanged(LongPoll longPoll, long peerId, List<LongPollActivityInfo> infos) {
            if (peerId != PeerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    double timeout = 5000;
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
            })();
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

                    StringBuilder status = new StringBuilder();
                    foreach (var act in groupedActivities) {
                        if (act.Count == 0) continue;
                        var type = act[0].Status;
                        string actstr = GetLocalizedActivityStatus(type, act.Count);

                        if (has3AndMoreDifferentTypes) {
                            if (status.Length != 0) status.Append(", ");
                            status.Append($"{act.Count} {actstr}");
                        } else {
                            if (status.Length != 0) status.Append(", ");
                            List<long> ids = act.Select(s => s.MemberId).ToList();
                            status.Append($"{GetNamesForActivityStatus(ids, act.Count, act.Count == 1)} {actstr}");
                        }
                    }

                    ActivityStatus = $"{status.ToString().Trim()}…";
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
                case LongPollActivityType.Typing: return Localizer.Get($"lp_act_typing{suffix}");
                case LongPollActivityType.RecordingAudioMessage: return Localizer.Get($"lp_act_voice{suffix}");
                case LongPollActivityType.UploadingPhoto: return Localizer.Get($"lp_act_photo{suffix}");
                case LongPollActivityType.UploadingVideo: return Localizer.Get($"lp_act_video{suffix}");
                case LongPollActivityType.UploadingFile: return Localizer.Get($"lp_act_file{suffix}");
            }
            return string.Empty;
        }

        private string GetNamesForActivityStatus(IReadOnlyList<long> ids, int count, bool showFullLastName) {
            StringBuilder sb = new StringBuilder();
            foreach (long id in ids) {
                if (id.IsUser()) {
                    User u = CacheManager.GetUser(id);
                    if (u != null) {
                        string lastName = showFullLastName ? u.LastName : $"{u.LastName[0]}.";
                        sb.Append($"{u.FirstName} {lastName}");
                    }
                } else if (id.IsGroup()) {
                    var g = CacheManager.GetGroup(id);
                    if (g != null) sb.Append($"\"{g.Name}\"");
                }
            }
            if (sb.Length > 0 && count > 1) sb.Append($" {Localizer.GetFormatted("im_status_more", count - 1)}");
            return sb.ToString();
        }

        private void LongPoll_NotificationsSettingsChanged(object sender, LongPollPushNotificationData e) {
            if (e.PeerId != PeerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    PushSettings ps = new PushSettings {
                        DisabledForever = e.DisabledUntil == -1,
                        DisabledUntil = e.DisabledUntil,
                        NoSound = e.Sound == 0
                    };
                    PushSettings = ps;
                });
            })();
        }

        private void LongPoll_UnreadReactionsChanged(LongPoll longPoll, long peerId, List<int> cmIds) {
            if (peerId != PeerId) return;
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    if (cmIds == null || cmIds.Count == 0) {
                        UnreadReactions = null;
                        return;
                    }
                    UnreadReactions = new ObservableCollection<int>(cmIds);
                });
            })();
        }

        private void VKQueue_Online(object sender, DataModels.VKQueue.OnlineEvent e) {
            if (PeerId != e.UserId) return;
            Log.Verbose($"ChatViewModel > Online event. User: {e.UserId}; IsOnline: {e.Online}; Platform: {e.Platform}; App: {e.AppId}; LastSeen: {e.LastSeen}");
            new System.Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    Online.IsOnline = e.Online;
                    Online.IsMobile = e.Platform != 6 && e.Platform != 7;
                    Online.AppId = e.AppId;
                    Online.LastSeenUnix = e.LastSeenUnix;
                    OnPropertyChanged(nameof(Online));
                });
            })();
        }

        #endregion

        #region Notification

        private async Task ShowSystemNotificationAsync(MessageViewModel message, bool isMention) {
            bool notifsEnabled = PeerType == PeerType.Chat ? Settings.NotificationsGroupChat : Settings.NotificationsPrivate;
            if (!notifsEnabled) return;

            bool soundSettings = PeerType == PeerType.Chat ? Settings.NotificationsGroupChatSound : Settings.NotificationsPrivateSound;
            bool sound = !PushSettings.NoSound && soundSettings;
            bool canNotify = isMention ? true : CanNotify();
            if (message.IsOutgoing || !canNotify) return;
            Log.Information($"ChatViewModel: about to show new message notification ({message.PeerId}_{message.ConversationMessageId}). Is mention: {isMention}.");
            await Task.Delay(20); // имя отправителя может не оказаться в кеше вовремя.

            string text = message.ToString();
            string chatName = PeerType == PeerType.Chat ? Localizer.GetFormatted("in_chat", Title) : null;

            var ava = await BitmapManager.GetBitmapAsync(message.SenderAvatar, 56, 56);
            var t = new ToastNotification(message, session.Name, message.SenderName, text, chatName, ava);
            t.OnClick += () => {
                Log.Information($"ChatViewModel: clicked on message {message.PeerId}_{message.ConversationMessageId}");
                session.TryOpenWindow();
                session.GoToChat(message.PeerId, message.ConversationMessageId);
            };
            //if (CanWrite.Allowed) t.OnSendClick += (text) => {
            //    // TODO: send message from toast
            //};
            session.ShowSystemNotification(t);
            if (sound) {
                var bb2 = AssetsManager.OpenAsset(new Uri("avares://laney/Assets/Audio/bb2.mp3"));
                LMediaPlayer.SFX?.PlayStream(bb2);
            }
        }

        private bool CanNotify() {
            if (PushSettings.DisabledForever) return false;
            return PushSettings.DisabledUntil == 0 || PushSettings.DisabledUntil < DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        #endregion


        private ulong GetSortIndex() {
            if (SortId.MajorId == 0) return (ulong)SortId.MinorId;
            ulong index = ((ulong)SortId.MajorId * 100000000) + (ulong)SortId.MinorId;
            return index;
        }

        public void Unload() {
            Log.Information($"Unloading chat {Id}...");
            if (!DemoMode.IsEnabled) {
                session.LongPoll.MessageFlagSet -= LongPoll_MessageFlagSet;
                session.LongPoll.MessageReceived -= LongPoll_MessageReceived;
                session.LongPoll.MessageEdited -= LongPoll_MessageEdited;
                session.LongPoll.MentionReceived -= LongPoll_MentionReceived;
                session.LongPoll.IncomingMessagesRead -= LongPoll_IncomingMessagesRead;
                session.LongPoll.OutgoingMessagesRead -= LongPoll_OutgoingMessagesRead;
                session.LongPoll.ConversationFlagReset -= LongPoll_ConversationFlagReset;
                session.LongPoll.ConversationFlagSet -= LongPoll_ConversationFlagSet;
                session.LongPoll.ConversationRemoved -= LongPoll_ConversationRemoved;
                session.LongPoll.MajorIdChanged -= LongPoll_MajorIdChanged;
                session.LongPoll.MinorIdChanged -= LongPoll_MinorIdChanged;
                session.LongPoll.ConversationDataChanged -= LongPoll_ConversationDataChanged;
                session.LongPoll.ActivityStatusChanged -= LongPoll_ActivityStatusChanged;
                session.LongPoll.NotificationsSettingsChanged -= LongPoll_NotificationsSettingsChanged;
                session.LongPoll.UnreadReactionsChanged -= LongPoll_UnreadReactionsChanged;

                if (!session.IsGroup) VKQueue.Online -= VKQueue_Online;
            }

            ActivityStatusUsers.Elapsed -= ActivityStatusUsers_Elapsed;
            DisplayedMessages?.Clear();
            ReceivedMessages?.Clear();
        }
    }
}