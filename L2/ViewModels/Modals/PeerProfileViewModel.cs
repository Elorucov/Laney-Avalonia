using Avalonia.Controls;
using DynamicData;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using ELOR.Laney.Execute;
using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Methods;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VKUI.Controls;
using VKUI.Popups;

namespace ELOR.Laney.ViewModels.Modals {
    public sealed class ConversationAttachmentsTabViewModel : ItemsViewModel<ConversationAttachment> {
        public bool End { get; set; } = false;
    }

    public sealed class ChatMembersTabViewModel : ItemsViewModel<Entity> {
        private List<Entity> _allMembers;

        private string _searchQuery;
        public string SearchQuery { get { return _searchQuery; } set { _searchQuery = value; OnPropertyChanged(); } }
        public bool SearchAvailable { get { return Items.Count > 0; } }

        public ChatMembersTabViewModel(ObservableCollection<Entity> displayedItems, Func<List<Entity>> getAllMembersCallback) : base(displayedItems) {
            _allMembers = getAllMembersCallback();
            PropertyChanged += OnPropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(SearchAvailable));
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SearchQuery):
                    SearchMember();
                    break;
            }
        }

        private void SearchMember() {
            if (IsLoading) return;
            Items.CollectionChanged -= Items_CollectionChanged; // Required, because searchbox is temporary disappear and focus losing from that searchbox.
            Items.Clear();
            if (!String.IsNullOrWhiteSpace(SearchQuery)) {
                var foundMembers = _allMembers.Where(m => m.Name.ToLower().Contains(SearchQuery.ToLower()));
                Items.AddRange(foundMembers);
            } else {
                Items.AddRange(_allMembers);
            }
            Items.CollectionChanged += Items_CollectionChanged;
        }

        ~ChatMembersTabViewModel() {
            PropertyChanged -= OnPropertyChanged;
            Items.CollectionChanged -= Items_CollectionChanged;
        }
    }

    public sealed class PeerProfileViewModel : CommonViewModel {
        private long _id;
        private string _header;
        private string _subhead;
        private Uri _avatar;
        private ObservableCollection<TwoStringTuple> _information = new ObservableCollection<TwoStringTuple>();
        private ChatMembersTabViewModel _chatMembers;

        private ConversationAttachmentsTabViewModel _photos = new ConversationAttachmentsTabViewModel();
        private ConversationAttachmentsTabViewModel _videos = new ConversationAttachmentsTabViewModel();
        private ConversationAttachmentsTabViewModel _audios = new ConversationAttachmentsTabViewModel();
        private ConversationAttachmentsTabViewModel _documents = new ConversationAttachmentsTabViewModel();
        private ConversationAttachmentsTabViewModel _share = new ConversationAttachmentsTabViewModel();
        //private ConversationAttachmentsTabViewModel _graffities = new ConversationAttachmentsTabViewModel();
        //private ConversationAttachmentsTabViewModel _audioMessages = new ConversationAttachmentsTabViewModel();

        private Command _firstCommand;
        private Command _secondCommand;
        private Command _thirdCommand;
        private Command _moreCommand;

        public long Id { get { return _id; } private set { _id = value; OnPropertyChanged(); } }
        public string Header { get { return _header; } private set { _header = value; OnPropertyChanged(); } }
        public string Subhead { get { return _subhead; } private set { _subhead = value; OnPropertyChanged(); } }
        public Uri Avatar { get { return _avatar; } private set { _avatar = value; OnPropertyChanged(); } }
        public ObservableCollection<TwoStringTuple> Information { get { return _information; } private set { _information = value; OnPropertyChanged(); } }
        public ChatMembersTabViewModel ChatMembers { get { return _chatMembers; } private set { _chatMembers = value; OnPropertyChanged(); } }

        public ConversationAttachmentsTabViewModel Photos { get { return _photos; } set { _photos = value; OnPropertyChanged(); } }
        public ConversationAttachmentsTabViewModel Videos { get { return _videos; } set { _videos = value; OnPropertyChanged(); } }
        public ConversationAttachmentsTabViewModel Audios { get { return _audios; } set { _audios = value; OnPropertyChanged(); } }
        public ConversationAttachmentsTabViewModel Documents { get { return _documents; } set { _documents = value; OnPropertyChanged(); } }
        public ConversationAttachmentsTabViewModel Share { get { return _share; } set { _share = value; OnPropertyChanged(); } }
        //public ConversationAttachmentsTabViewModel Graffities { get { return _graffities; } set { _graffities = value; OnPropertyChanged(); } }
        //public ConversationAttachmentsTabViewModel AudioMessages { get { return _audioMessages; } set { _audioMessages = value; OnPropertyChanged(); } }

        public Command FirstCommand { get { return _firstCommand; } private set { _firstCommand = value; OnPropertyChanged(); } }
        public Command SecondCommand { get { return _secondCommand; } private set { _secondCommand = value; OnPropertyChanged(); } }
        public Command ThirdCommand { get { return _thirdCommand; } private set { _thirdCommand = value; OnPropertyChanged(); } }
        public Command MoreCommand { get { return _moreCommand; } private set { _moreCommand = value; OnPropertyChanged(); } }

        private int _lastCmid = 0;
        private VKSession session;
        private List<Entity> _allMembers = new List<Entity>();
        private ObservableCollection<Entity> _displayedMembers = new ObservableCollection<Entity>();
        public event EventHandler CloseWindowRequested;

        public PeerProfileViewModel(VKSession session, long peerId) {
            this.session = session;
            Id = peerId;
            new System.Action(async () => await SetupAsync())();
        }

        private async Task SetupAsync() {
            Header = null;
            if (Id.IsChat()) {
                ChatMembers = null;
                await GetChatAsync(Id);
            } else if (Id.IsUser()) {
                await GetUserAsync(Id);
            } else if (Id.IsGroup()) {
                await GetGroupAsync(Id * -1);
            }
        }

        #region User-specific

        private async Task GetUserAsync(long userId) {
            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            try {
                UserEx user = await session.API.GetUserCardAsync(userId, VKAPIHelper.UserFields);
                _lastCmid = user.LastCMID;
                Header = String.Join(" ", new string[2] { user.FirstName, user.LastName });
                if (user.Photo != null) Avatar = user.Photo;
                Subhead = VKAPIHelper.GetOnlineInfo(user.OnlineInfo, user.Sex).ToLowerInvariant();

                switch (user.Deactivated) {
                    case DeactivationState.Banned: Subhead = Assets.i18n.Resources.user_blocked; break;
                    case DeactivationState.Deleted: Subhead = Assets.i18n.Resources.user_deleted; break;
                    default: Subhead = VKAPIHelper.GetOnlineInfo(user.OnlineInfo, user.Sex).ToLowerInvariant(); break;
                }

                SetupInfo(user);
                SetupCommands(user);
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.GetUser!");
                Header = null;
                Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await GetUserAsync(userId));
            }
            IsLoading = false;
        }

        private void SetupInfo(UserEx user) {
            Information.Clear();

            Information.Add(new TwoStringTuple(VKIconNames.Icon20BugOutline, user.Id.ToString()));

            // Banned/deleted/blocked...
            if (user.Blacklisted == 1) {
                Information.Add(new TwoStringTuple(VKIconNames.Icon20BlockOutline, Localizer.Get("user_blacklisted", user.Sex)));
            }
            if (user.BlacklistedByMe == 1) {
                Information.Add(new TwoStringTuple(VKIconNames.Icon20BlockOutline, Localizer.Get("user_blacklisted_by_me", user.Sex)));
            }

            // Domain
            Information.Add(new TwoStringTuple(VKIconNames.Icon20MentionOutline, user.Domain));

            // Private profile
            if (user.IsClosed && !user.CanAccessClosed)
                Information.Add(new TwoStringTuple(VKIconNames.Icon20LockOutline, Assets.i18n.Resources.user_private));

            // Status
            if (!String.IsNullOrEmpty(user.Status))
                Information.Add(new TwoStringTuple(VKIconNames.Icon20ArticleOutline, user.Status.Trim()));

            // Birthday
            if (!String.IsNullOrEmpty(user.BirthDate))
                Information.Add(new TwoStringTuple(VKIconNames.Icon20GiftOutline, VKAPIHelper.GetNormalizedBirthDate(user.BirthDate)));

            // Live in
            if (!String.IsNullOrWhiteSpace(user.LiveIn))
                Information.Add(new TwoStringTuple(VKIconNames.Icon20HomeOutline, user.LiveIn.Trim()));

            // Work
            if (user.CurrentCareer != null) {
                var c = user.CurrentCareer;
                string h = c.Company.Trim();
                Information.Add(new TwoStringTuple(VKIconNames.Icon20WorkOutline, String.IsNullOrWhiteSpace(c.Position) ? h : $"{h} — {c.Position.Trim()}"));
            }

            // Education
            if (!String.IsNullOrWhiteSpace(user.CurrentEducation))
                Information.Add(new TwoStringTuple(VKIconNames.Icon20EducationOutline, user.CurrentEducation.Trim()));

            // Site
            if (!String.IsNullOrWhiteSpace(user.Site))
                Information.Add(new TwoStringTuple(VKIconNames.Icon20LinkCircleOutline, user.Site.Trim()));

            // Followers
            if (user.Followers > 0)
                Information.Add(new TwoStringTuple(VKIconNames.Icon20FollowersOutline, Localizer.GetDeclensionFormatted(user.Followers, "follower")));
        }

        private void SetupCommands(UserEx user) {
            FirstCommand = null;
            SecondCommand = null;
            ThirdCommand = null;
            MoreCommand = null;
            List<Command> commands = new List<Command>();
            List<Command> moreCommands = new List<Command>();

            // Если нет истории сообщений с этим юзером,
            // и ему нельзя писать сообщение,
            // или если открыт чат с этим юзером,
            // то не будем добавлять эту кнопку
            if ((user.CanWritePrivateMessage == 1 || user.MessagesCount > 0) && session.CurrentOpenedChat?.PeerId != user.Id) {
                Command messageCmd = new Command(VKIconNames.Icon20MessageOutline, Assets.i18n.Resources.message, false, (a) => {
                    CloseWindowRequested?.Invoke(this, null);
                    session.GoToChat(user.Id);
                });
                commands.Add(messageCmd);
            }

            // Friend
            if (session.UserId != user.Id && user.Blacklisted == 0 && user.BlacklistedByMe == 0
                && user.Deactivated == DeactivationState.No && user.CanSendFriendRequest == 1) {
                string ficon = VKIconNames.Icon20ServicesOutline;
                string flabel = "";

                switch (user.FriendStatus) {
                    case FriendStatus.None:
                        flabel = Assets.i18n.Resources.pp_friend_add;
                        ficon = VKIconNames.Icon20UserAddOutline;
                        break;
                    case FriendStatus.IsFriend:
                        flabel = Assets.i18n.Resources.pp_friend_your;
                        ficon = VKIconNames.Icon20UserCheckOutline;
                        break;
                    case FriendStatus.InboundRequest:
                        flabel = Assets.i18n.Resources.pp_friend_accept;
                        ficon = VKIconNames.Icon20UserAddOutline;
                        break;
                    case FriendStatus.RequestSent:
                        flabel = Assets.i18n.Resources.pp_friend_request;
                        ficon = VKIconNames.Icon20UserOutline;
                        break;
                }

                Command friendCmd = new Command(ficon, flabel, false, (a) => ExceptionHelper.ShowNotImplementedDialog(session.ModalWindow));
                commands.Add(friendCmd);
            }

            // Notifications
            if (session.UserId != user.Id) {
                string notifIcon = user.NotificationsDisabled ? VKIconNames.Icon20NotificationSlashOutline : VKIconNames.Icon20NotificationOutline;
                Command notifsCmd = new Command(notifIcon, user.NotificationsDisabled ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled, false, async (a) => await ToggleNotificationsAsync(!user.NotificationsDisabled, user.Id));
                commands.Add(notifsCmd);
            }

            // Open in browser
            Command openExternalCmd = new Command(VKIconNames.Icon20LinkCircleOutline, Assets.i18n.Resources.pp_profile, false, async (a) => await Launcher.LaunchUrl($"https://vk.com/id{user.Id}"));
            commands.Add(openExternalCmd);

            // Ban/unban
            if (session.UserId != user.Id && user.Blacklisted == 0) {
                string banIcon = user.BlacklistedByMe == 1 ? VKIconNames.Icon20UnlockOutline : VKIconNames.Icon20BlockOutline;
                string banLabel = user.BlacklistedByMe == 1 ? Assets.i18n.Resources.unblock : Assets.i18n.Resources.block;
                Command banCmd = new Command(banIcon, banLabel, true, async (a) => await ToggleBanAsync(user.Id, user.BlacklistedByMe == 1));
                moreCommands.Add(banCmd);
            }

            // Clear history
            if (user.MessagesCount > 0) {
                Command clearCmd = new Command(VKIconNames.Icon20DeleteOutline, Assets.i18n.Resources.chat_clear_history, true, (a) => ContextMenuHelper.TryClearChat(session, Id, async () => await SetupAsync()));
                moreCommands.Add(clearCmd);
            }

            Command moreCommand = new Command(VKIconNames.Icon20More, Assets.i18n.Resources.more, false, (a) => OpenContextMenu(a, commands, moreCommands));

            FirstCommand = commands[0];

            if (commands.Count < 2) {
                SecondCommand = moreCommand;
            } else if (commands.Count < 3) {
                SecondCommand = commands[1];
                ThirdCommand = moreCommand;
            } else {
                SecondCommand = commands[1];
                ThirdCommand = commands[2];
                MoreCommand = moreCommand;
            }
        }

        private async Task ToggleBanAsync(long userId, bool unban) {
            IsLoading = true;
            try {
                bool result = unban ?
                await session.API.Account.UnbanAsync(userId) :
                await session.API.Account.BanAsync(userId);
                IsLoading = false;
                await SetupAsync(); // TODO: обновить кнопку, а не всё окно.
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.ToggleBan!");
                IsLoading = false;
                await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
            }
        }

        #endregion

        #region Group-specific

        private async Task GetGroupAsync(long groupId) {
            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            try {
                GroupEx group = await session.API.GetGroupCardAsync(groupId, VKAPIHelper.GroupFields);
                Header = group.Name;
                if (group.Photo != null) Avatar = group.Photo;
                Subhead = group.Activity;
                _lastCmid = group.LastCMID;
                SetupInfo(group);
                SetupCommands(group);
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.GetGroup!");
                Header = null; // чтобы содержимое окна было скрыто
                Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await GetGroupAsync(groupId));
            }
            IsLoading = false;
        }

        private void SetupInfo(GroupEx group) {
            Information.Clear();

            Information.Add(new TwoStringTuple(VKIconNames.Icon20BugOutline, group.Id.ToString()));

            // Domain
            Information.Add(new TwoStringTuple(VKIconNames.Icon20MentionOutline, !String.IsNullOrEmpty(group.ScreenName) ? group.ScreenName : $"club{group.Id}"));

            // Status
            if (!String.IsNullOrEmpty(group.Status))
                Information.Add(new TwoStringTuple(VKIconNames.Icon20ArticleOutline, group.Status.Trim()));

            // City
            string cc = null;
            if (group.City != null) cc = group.City.Title.Trim();
            if (group.Country != null) cc += !String.IsNullOrEmpty(cc) ? $", {group.Country.Title.Trim()}" : group.Country.Title.Trim();
            if (!String.IsNullOrEmpty(cc))
                Information.Add(new TwoStringTuple(VKIconNames.Icon20HomeOutline, cc));

            // Site
            if (!String.IsNullOrWhiteSpace(group.Site))
                Information.Add(new TwoStringTuple(VKIconNames.Icon20LinkCircleOutline, group.Site.Trim()));

            // Members
            if (group.Members > 0)
                Information.Add(new TwoStringTuple(VKIconNames.Icon20FollowersOutline, Localizer.GetDeclensionFormatted(group.Members, "members_sub")));
        }

        private void SetupCommands(GroupEx group) {
            FirstCommand = null;
            SecondCommand = null;
            ThirdCommand = null;
            MoreCommand = null;
            List<Command> commands = new List<Command>();
            List<Command> moreCommands = new List<Command>();

            if ((group.CanMessage == 1 || group.MessagesCount > 0) && session.CurrentOpenedChat?.PeerId != -group.Id) {
                Command messageCmd = new Command(VKIconNames.Icon20MessageOutline, Assets.i18n.Resources.message, false, (a) => {
                    CloseWindowRequested?.Invoke(this, null);
                    session.GoToChat(-group.Id);
                });
                commands.Add(messageCmd);
            }

            // Notifications
            string notifIcon = group.NotificationsDisabled ? VKIconNames.Icon20NotificationSlashOutline : VKIconNames.Icon20NotificationOutline;
            Command notifsCmd = new Command(notifIcon, group.NotificationsDisabled ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled, false, async (a) => await ToggleNotificationsAsync(!group.NotificationsDisabled, -group.Id));
            commands.Add(notifsCmd);

            // Open in browser
            Command openExternalCmd = new Command(VKIconNames.Icon20LinkCircleOutline, Assets.i18n.Resources.pp_group, false, async (a) => await Launcher.LaunchUrl($"https://vk.com/club{group.Id}"));
            commands.Add(openExternalCmd);

            // Allow/deny messages from group
            string banIcon = group.MessagesAllowed == 1 ? VKIconNames.Icon20BlockOutline : VKIconNames.Icon20Check;
            string banLabel = group.MessagesAllowed == 1 ? Assets.i18n.Resources.pp_deny : Assets.i18n.Resources.pp_allow;
            Command banCmd = new Command(banIcon, banLabel, group.MessagesAllowed == 1, async (a) => await ToggleMessagesFromGroupAsync(group.Id, group.MessagesAllowed == 1));
            moreCommands.Add(banCmd);

            // Clear history
            if (group.MessagesCount > 0) {
                Command clearCmd = new Command(VKIconNames.Icon20DeleteOutline, Assets.i18n.Resources.chat_clear_history, true, (a) => ContextMenuHelper.TryClearChat(session, Id, async () => await SetupAsync()));
                moreCommands.Add(clearCmd);
            }

            Command moreCommand = new Command(VKIconNames.Icon20More, Assets.i18n.Resources.more, false, (a) => OpenContextMenu(a, commands, moreCommands));

            FirstCommand = commands[0];

            if (commands.Count < 2) {
                SecondCommand = moreCommand;
            } else if (commands.Count < 3) {
                SecondCommand = commands[1];
                ThirdCommand = moreCommand;
            } else {
                SecondCommand = commands[1];
                ThirdCommand = commands[2];
                MoreCommand = moreCommand;
            }
        }

        private async Task ToggleMessagesFromGroupAsync(long groupId, bool allowed) {
            IsLoading = true;
            try {
                bool result = allowed ?
                    await session.API.Messages.DenyMessagesFromGroupAsync(groupId) :
                    await session.API.Messages.AllowMessagesFromGroupAsync(groupId);
                IsLoading = false;
                await SetupAsync(); // TODO: обновить кнопку, а не всё окно.
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.ToggleMessageFromGroup!");
                IsLoading = false;
                await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
            }
        }

        #endregion

        #region Chat-specific

        private async Task GetChatAsync(long peerId) {
            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            try {
                ChatInfoEx chat = await session.API.GetChatAsync(peerId - 2000000000, VKAPIHelper.Fields);
                _lastCmid = chat.LastCMID;
                Header = chat.Name;
                if (chat.PhotoUri != null) Avatar = chat.PhotoUri;

                if (chat.State == UserStateInChat.In) {
                    Subhead = String.Empty;
                    if (chat.IsCasperChat) Subhead = $"{Assets.i18n.Resources.casper_chat.ToLowerInvariant()}, ";
                    Subhead += Localizer.GetDeclensionFormatted(chat.MembersCount, "members_sub");
                } else {
                    Subhead = chat.State == UserStateInChat.Left ? Assets.i18n.Resources.chat_left : Assets.i18n.Resources.chat_kicked.ToLowerInvariant();
                }

                SetupCommands(chat);
                if (!chat.IsChannel && chat.State == UserStateInChat.In) {
                    if (ChatMembers == null) ChatMembers = new ChatMembersTabViewModel(_displayedMembers, () => _allMembers);
                    IsLoading = false; // required, see PeerProfile.axaml.cs > ViewModel_PropertyChanged
                    await LoadChatMembersAsync(chat);
                }
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.GetChatAsync!");
                Header = null; // чтобы содержимое окна было скрыто
                Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await GetChatAsync(peerId));
            } finally {
                IsLoading = false;
            }
        }

        private async Task LoadChatMembersAsync(ChatInfoEx chat) {
            if (ChatMembers.IsLoading) return;
            try {
                ChatMembers.IsLoading = true;
                _allMembers.Clear();
                _displayedMembers.Clear();

                var response = await session.API.Messages.GetConversationMembersAsync(session.GroupId, Id, extended: true, fields: VKAPIHelper.Fields);
                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);
                SetupMembers(chat, response.Items);
                _displayedMembers.AddRange(_allMembers);
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.LoadChatMembersAsync!");
                _displayedMembers.Clear(); // вдруг краш произойдёт при парсинге участников, а часть из них уже были добавлены в список/UI, их надо удалить.
                ChatMembers.Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadChatMembersAsync(chat));
            } finally {
                ChatMembers.IsLoading = false;
            }
        }

        private void SetupMembers(ChatInfoEx chat, List<ChatMember> members) {
            if (members == null || members.Count == 0) return;
            foreach (var member in CollectionsMarshal.AsSpan(members)) {
                string name = member.MemberId.ToString();
                string desc = String.Empty;
                long mid = member.MemberId;
                long iid = member.InvitedBy;
                Uri avatar = null;

                string joinDate = member.JoinDate.ToHumanizedTimeOrDateString();

                if (mid != iid) {
                    string invitedBy = String.Empty;
                    if (iid.IsUser()) {
                        var user = CacheManager.GetUser(iid);
                        if (user != null) {
                            invitedBy = Localizer.GetFormatted(user.Sex, "invited_by", user.NameWithFirstLetterSurname());
                        }
                    } else if (iid.IsGroup()) {
                        var group = CacheManager.GetGroup(iid);
                        if (group != null) {
                            invitedBy = Localizer.GetFormatted(Sex.Male, "invited_by", group.Name);
                        }
                    }
                    if (member.IsAdmin) desc = $"{Assets.i18n.Resources.admin}, ";
                    desc += $"{invitedBy} {joinDate}";
                } else if (mid == chat.OwnerId) {
                    desc = Localizer.Get("created_on", Sex.Male);
                }

                if (mid.IsUser()) {
                    var user = CacheManager.GetUser(member.MemberId);
                    if (user != null) {
                        name = user.FullName;
                        avatar = user.Photo;
                    }
                } else if (mid.IsGroup()) {
                    var group = CacheManager.GetGroup(member.MemberId);
                    if (group != null) {
                        name = group.Name;
                        avatar = group.Photo;
                    }
                }

                Command command = SetUpMemberCommand(chat, member);
                _allMembers.Add(new Entity(mid, avatar, name, desc, command));
            }
        }

        private Command SetUpMemberCommand(ChatInfoEx chat, ChatMember member) {
            ActionSheet ash = new ActionSheet {
                Placement = PlacementMode.BottomEdgeAlignedRight
            };

            var profile = new ActionSheetItem {
                Header = Assets.i18n.Resources.open_profile
            };
            profile.Click += (a, b) => {
                // TODO: открывать профиль в текущем окне PeerProfile с возможностью вернуться назад.
                CloseWindowRequested?.Invoke(this, null);
                new System.Action(async () => await Router.OpenPeerProfileAsync(session, member.MemberId))();
            };
            ash.Items.Add(profile);

            // TODO: админы (не создатель), которые тоже имеют права менять админов.
            bool canChangeAdmin = chat.OwnerId == session.Id;

            if (member.MemberId != session.Id && canChangeAdmin && !member.IsAdmin) ash.Items.Add(new ActionSheetItem {
                Header = Assets.i18n.Resources.pp_member_admin_add
            });

            if (member.MemberId != session.Id && canChangeAdmin && member.IsAdmin) ash.Items.Add(new ActionSheetItem {
                Header = Assets.i18n.Resources.pp_member_admin_remove
            });

            if (member.MemberId != session.Id && member.CanKick) ash.Items.Add(new ActionSheetItem {
                Header = Assets.i18n.Resources.pp_member_kick
            });

            if (ash.Items.Count > 0) {
                return new Command(VKIconNames.Icon24MoreHorizontal, Assets.i18n.Resources.more, false, (c) => {
                    ash.ShowAt(c as Control);
                });
            }

            return null;
        }

        private void SetupCommands(ChatInfoEx chat) {
            FirstCommand = null;
            SecondCommand = null;
            ThirdCommand = null;
            MoreCommand = null;
            List<Command> commands = new List<Command>();
            List<Command> moreCommands = new List<Command>();

            // Edit
            if (chat.ACL.CanChangeInfo) {
                Command editCmd = new Command(VKIconNames.Icon20WriteOutline, Assets.i18n.Resources.edit, false, (a) => ExceptionHelper.ShowNotImplementedDialog(session.ModalWindow));
                commands.Add(editCmd);
            }

            // Add member
            if (chat.ACL.CanInvite) {
                Command addCmd = new Command(VKIconNames.Icon20UserAddOutline, Assets.i18n.Resources.add, false, (a) => ExceptionHelper.ShowNotImplementedDialog(session.ModalWindow));
                commands.Add(addCmd);
            }

            // Notifications
            bool notifsDisabled = chat.PushSettings != null && chat.PushSettings.DisabledForever;
            string notifIcon = notifsDisabled ? VKIconNames.Icon20NotificationSlashOutline : VKIconNames.Icon20NotificationOutline;
            Command notifsCmd = new Command(notifIcon, notifsDisabled ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled, false, async (a) => await ToggleNotificationsAsync(!notifsDisabled, chat.PeerId));
            commands.Add(notifsCmd);

            // Link
            if (chat.ACL.CanSeeInviteLink) {
                var act = new System.Action<object>((o) => {
                    ChatLinkViewer modal = new ChatLinkViewer(session, Id);
                    modal.ShowDialog(session.ModalWindow);
                });

                Command chatLinkCmd = new Command(VKIconNames.Icon20LinkCircleOutline, Assets.i18n.Resources.link, false, act);
                commands.Add(chatLinkCmd);
            }

            // Unpin message
            if (chat.ACL.CanChangePin && chat.PinnedMessage != null) {
                Command unpinCmd = new Command(VKIconNames.Icon20PinSlashOutline, Assets.i18n.Resources.pp_unpin_message, false, async (a) => {
                    if (await ContextMenuHelper.UnpinMessageAsync(session, Id)) chat.PinnedMessage = null;
                });
                commands.Add(unpinCmd);
            }

            // Clear history
            Command clearCmd = new Command(VKIconNames.Icon20DeleteOutline, Assets.i18n.Resources.chat_clear_history, true, (a) => ContextMenuHelper.TryClearChat(session, Id, async () => await SetupAsync()));
            moreCommands.Add(clearCmd);

            // Exit or return to chat/channel
            if (chat.State != UserStateInChat.Kicked) {
                string exitLabel = chat.IsChannel ? Assets.i18n.Resources.pp_exit_channel : Assets.i18n.Resources.pp_exit_chat;
                string returnLabel = chat.IsChannel ? Assets.i18n.Resources.pp_return_channel : Assets.i18n.Resources.pp_return_chat;
                string icon = chat.State == UserStateInChat.In ? VKIconNames.Icon20DoorArrowRightOutline : VKIconNames.Icon20DoorEnterArrowRightOutline;
                Command exitRetCmd = new Command(icon, chat.State == UserStateInChat.In ? exitLabel : returnLabel, true, (a) => {
                    if (chat.State == UserStateInChat.In) {
                        ContextMenuHelper.TryLeaveChat(session, Id, async () => await SetupAsync());
                    } else {
                        ContextMenuHelper.ReturnToChat(session, Id, async () => await SetupAsync());
                    }
                });
                moreCommands.Add(exitRetCmd);
            }

            Command moreCommand = new Command(VKIconNames.Icon20More, Assets.i18n.Resources.more, false, (a) => OpenContextMenu(a, commands, moreCommands));

            FirstCommand = commands[0];

            if (commands.Count < 2) {
                SecondCommand = moreCommand;
            } else if (commands.Count < 3) {
                SecondCommand = commands[1];
                ThirdCommand = moreCommand;
            } else {
                SecondCommand = commands[1];
                ThirdCommand = commands[2];
                MoreCommand = moreCommand;
            }
        }

        #endregion

        #region General commands

        private void OpenContextMenu(object target, List<Command> commands, List<Command> moreCommands) {
            ActionSheet ash = new ActionSheet();

            if (commands.Count > 3) {
                commands = commands.GetRange(3, commands.Count - 3);
                foreach (var item in CollectionsMarshal.AsSpan(commands)) {
                    ActionSheetItem asi = new ActionSheetItem {
                        Before = new VKIcon {
                            Id = item.IconId
                        },
                        Header = item.Label
                    };
                    asi.Click += (a, b) => item.Action.Execute(asi);
                    if (item.IsDestructive) asi.Classes.Add("Destructive");
                    ash.Items.Add(asi);
                }
            }

            if (ash.Items.Count > 0) ash.Items.Add(new ActionSheetItem());

            foreach (var item in CollectionsMarshal.AsSpan(moreCommands)) {
                ActionSheetItem asi = new ActionSheetItem {
                    Before = new VKIcon {
                        Id = item.IconId
                    },
                    Header = item.Label
                };
                asi.Click += (a, b) => item.Action.Execute(asi);
                if (item.IsDestructive) asi.Classes.Add("Destructive");
                ash.Items.Add(asi);
            }

            ash.ShowAt(target as Control, true);

        }

        private async Task ToggleNotificationsAsync(bool enabled, long id) {
            IsLoading = true;
            try {
                var result = await session.API.Account.SetSilenceModeAsync(!enabled ? 0 : -1, id, true);
                IsLoading = false;
                await SetupAsync(); // TODO: обновить кнопку, а не всё окно.
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.ToggleNotifications!");
                IsLoading = false;
                await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
            }
        }

        #endregion

        #region Conversation attachments

        public async Task LoadPhotosAsync() {
            await LoadConvAttachmentsAsync(Photos, HistoryAttachmentMediaType.Photo);
        }

        public async Task LoadVideosAsync() {
            await LoadConvAttachmentsAsync(Videos, HistoryAttachmentMediaType.Video);
        }

        public async Task LoadAudiosAsync() {
            await LoadConvAttachmentsAsync(Audios, HistoryAttachmentMediaType.Audio);
        }

        public async Task LoadDocsAsync() {
            await LoadConvAttachmentsAsync(Documents, HistoryAttachmentMediaType.Doc);
        }

        public async Task LoadLinksAsync() {
            await LoadConvAttachmentsAsync(Share, HistoryAttachmentMediaType.Share);
        }

        //public void LoadGraffities() {
        //    LoadVM(Graffities, HistoryAttachmentMediaType.Graffiti);
        //}

        //public void LoadAudioMessages() {
        //    LoadVM(AudioMessages, HistoryAttachmentMediaType.AudioMessage);
        //}

        private async Task LoadConvAttachmentsAsync(ConversationAttachmentsTabViewModel ivm, HistoryAttachmentMediaType type) {
            if (ivm.IsLoading || ivm.End) return;
            ivm.Placeholder = null;
            ivm.IsLoading = true;
            try {
                ConversationAttachmentsResponse resp = await session.API.Messages.GetHistoryAttachmentsAsync(session.GroupId, Id, type, _lastCmid, ivm.Items.Count, Constants.AttachmentsCountPerRequest, true, fields: VKAPIHelper.Fields);
                CacheManager.Add(resp.Profiles);
                CacheManager.Add(resp.Groups);
                foreach (var item in CollectionsMarshal.AsSpan(resp.Items)) {
                    ivm.Items.Add(item);
                }
                if (resp.Items.Count < Constants.AttachmentsCountPerRequest) ivm.End = true;

                if (ivm.Items.Count == 0) {
                    ivm.Placeholder = new PlaceholderViewModel {
                        Text = Localizer.Get($"pp_attachments_{type}".ToLower()),
                        ActionButton = null
                    };
                    ivm.End = true;
                }
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.LoadConvAttachmentsAsync!");
                if (ivm.Items.Count == 0) {
                    ivm.Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadConvAttachmentsAsync(ivm, type));
                } else {
                    if (await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex)) await LoadConvAttachmentsAsync(ivm, type);
                }
            }
            ivm.IsLoading = false;
        }

        #endregion
    }
}