using ELOR.Laney.Collections;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
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
using System.Linq;
using System.Text;
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
            if (lastMessage != null) ReceivedMessages.Add(new MessageViewModel(lastMessage));
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
            SortId = c.SortId;
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
                if (ChatSettings.PinnedMessage != null) PinnedMessage = new MessageViewModel(ChatSettings.PinnedMessage);
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
        }

        #region Loading messages

        public async void LoadMessages(int startMessageId = -1) {
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
                DisplayedMessages = new MessagesCollection(mhr.Messages);

                //foreach (MessageViewModel msg in DisplayedMessages) {
                //    FixState(msg);
                //}

                //if (startMessageId > 0) ScrollToMessageCallback?.Invoke(startMessageId, true, true);
                //if (startMessageId == -1) {
                //    ScrollToMessageCallback?.Invoke(Math.Min(InRead, OutRead), false, false);
                //}
            } catch (Exception ex) {
                Placeholder = PlaceholderViewModel.GetForException(ex, () => { LoadMessages(startMessageId); });
            } finally {
                IsLoading = false;
                // SetPlaceholder();
            }
        }

        #endregion

        private ulong GetSortIndex() {
            if (SortId.MajorId == 0) return (ulong)SortId.MinorId;
            ulong index = ((ulong)SortId.MajorId * 100000000) + (ulong)SortId.MinorId;
            return index;
        }

        public void TestSetSortId(int num) {
            SortId.MinorId = num;
            OnPropertyChanged(nameof(SortIndex));
        }
    }
}