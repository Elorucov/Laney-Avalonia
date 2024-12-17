﻿using Avalonia.Controls;
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
using VKUI.Controls;
using VKUI.Popups;

namespace ELOR.Laney.ViewModels.Modals {
    public class ConversationAttachmentsTabViewModel : ItemsViewModel<ConversationAttachment> {
        public int StartFrom { get; set; } = 0;
        public bool End { get; set; } = false;
    }

    public class PeerProfileViewModel : CommonViewModel {
        private long _id;
        private string _header;
        private string _subhead;
        private Uri _avatar;
        private ObservableCollection<TwoStringTuple> _information = new ObservableCollection<TwoStringTuple>();
        private ObservableCollection<Entity> _displayedMembers;
        private string _memberSearchQuery;

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
        public ObservableCollection<Entity> DisplayedMembers { get { return _displayedMembers; } private set { _displayedMembers = value; OnPropertyChanged(); } }
        public string MemberSearchQuery { get { return _memberSearchQuery; } set { _memberSearchQuery = value; OnPropertyChanged(); } }

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

        private VKSession session;
        private ObservableCollection<Entity> allMembers = new ObservableCollection<Entity>();
        public event EventHandler CloseWindowRequested;

        public PeerProfileViewModel(VKSession session, long peerId) {
            this.session = session;
            Id = peerId;
            PropertyChanged += OnPropertyChanged;
            Setup();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(MemberSearchQuery):
                    SearchMember();
                    break;
            }
        }

        private void Setup() {
            Header = null;
            if (Id.IsChat()) {
                MemberSearchQuery = null;
                GetChat(Id);
            } else if (Id.IsUser()) {
                GetUser(Id);
            } else if (Id.IsGroup()) {
                GetGroup(Id * -1);
            }
        }

        #region User-specific

        private async void GetUser(long userId) {
            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            try {
                UserEx user = await session.API.GetUserCardAsync(userId);
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
                Placeholder = PlaceholderViewModel.GetForException(ex, (o) => GetUser(userId));
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

                Command friendCmd = new Command(ficon, flabel, false, (a) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow));
                commands.Add(friendCmd);
            }

            // Notifications
            if (session.UserId != user.Id) {
                string notifIcon = user.NotificationsDisabled ? VKIconNames.Icon20NotificationSlashOutline : VKIconNames.Icon20NotificationOutline;
                Command notifsCmd = new Command(notifIcon, user.NotificationsDisabled ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled, false, (a) => ToggleNotifications(!user.NotificationsDisabled, user.Id));
                commands.Add(notifsCmd);
            }

            // Open in browser
            Command openExternalCmd = new Command(VKIconNames.Icon20LinkCircleOutline, Assets.i18n.Resources.pp_profile, false, async (a) => await Launcher.LaunchUrl($"https://vk.com/id{user.Id}"));
            commands.Add(openExternalCmd);

            // Ban/unban
            if (session.UserId != user.Id && user.Blacklisted == 0) {
                string banIcon = user.BlacklistedByMe == 1 ? VKIconNames.Icon20UnlockOutline : VKIconNames.Icon20BlockOutline;
                string banLabel = user.BlacklistedByMe == 1 ? Assets.i18n.Resources.unblock : Assets.i18n.Resources.block;
                Command banCmd = new Command(banIcon, banLabel, true, (a) => ToggleBan(user.Id, user.BlacklistedByMe == 1));
                moreCommands.Add(banCmd);
            }

            // Clear history
            if (user.MessagesCount > 0) {
                Command clearCmd = new Command(VKIconNames.Icon20DeleteOutline, Assets.i18n.Resources.chat_clear_history, true, (a) => ContextMenuHelper.TryClearChat(session, Id, Setup));
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

        private async void ToggleBan(long userId, bool unban) {
            IsLoading = true;
            try {
                bool result = unban ?
                await session.API.Account.UnbanAsync(userId) :
                await session.API.Account.BanAsync(userId);
                IsLoading = false;
                Setup(); // TODO: обновить кнопку, а не всё окно.
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.ToggleBan!");
                IsLoading = false;
                await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
            }
        }

        #endregion

        #region Group-specific

        private async void GetGroup(long groupId) {
            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            try {
                GroupEx group = await session.API.GetGroupCardAsync(groupId);
                Header = group.Name;
                if (group.Photo != null) Avatar = group.Photo;
                Subhead = group.Activity;
                SetupInfo(group);
                SetupCommands(group);
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.GetGroup!");
                Header = null; // чтобы содержимое окна было скрыто
                Placeholder = PlaceholderViewModel.GetForException(ex, (o) => GetGroup(groupId));
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
            Command notifsCmd = new Command(notifIcon,group.NotificationsDisabled ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled, false, (a) => ToggleNotifications(!group.NotificationsDisabled, -group.Id));
            commands.Add(notifsCmd);

            // Open in browser
            Command openExternalCmd = new Command(VKIconNames.Icon20LinkCircleOutline, Assets.i18n.Resources.pp_group, false,async (a) => await Launcher.LaunchUrl($"https://vk.com/club{group.Id}"));
            commands.Add(openExternalCmd);

            // Allow/deny messages from group
            string banIcon = group.MessagesAllowed == 1 ? VKIconNames.Icon20BlockOutline : VKIconNames.Icon20Check;
            string banLabel = group.MessagesAllowed == 1 ? Assets.i18n.Resources.pp_deny : Assets.i18n.Resources.pp_allow;
            Command banCmd = new Command(banIcon, banLabel, group.MessagesAllowed == 1, (a) => ToggleMessagesFromGroup(group.Id, group.MessagesAllowed == 1));
            moreCommands.Add(banCmd);

            // Clear history
            if (group.MessagesCount > 0) {
                Command clearCmd = new Command(VKIconNames.Icon20DeleteOutline, Assets.i18n.Resources.chat_clear_history, true, (a) => ContextMenuHelper.TryClearChat(session, Id, Setup));
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

        private async void ToggleMessagesFromGroup(long groupId, bool allowed) {
            IsLoading = true;
            try {
                bool result = allowed ? 
                    await session.API.Messages.DenyMessagesFromGroupAsync(groupId) :
                    await session.API.Messages.AllowMessagesFromGroupAsync(groupId);
                IsLoading = false;
                Setup(); // TODO: обновить кнопку, а не всё окно.
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.ToggleMessageFromGroup!");
                IsLoading = false;
                await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
            }
        }

        #endregion

        #region Chat-specific

        private async void GetChat(long peerId) {
            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            try {
                ChatInfoEx chat = await session.API.GetChatAsync(peerId - 2000000000, VKAPIHelper.Fields);
                Header = chat.Name;
                if (chat.PhotoUri != null) Avatar = chat.PhotoUri;

                if (chat.State == UserStateInChat.In) {
                    Subhead = String.Empty;
                    if (chat.IsCasperChat) Subhead = $"{Assets.i18n.Resources.casper_chat.ToLowerInvariant()}, ";
                    Subhead += Localizer.GetDeclensionFormatted(chat.MembersCount, "members_sub");
                } else {
                    Subhead = chat.State == UserStateInChat.Left ? Assets.i18n.Resources.chat_left : Assets.i18n.Resources.chat_kicked.ToLowerInvariant();
                }

                if (!chat.IsChannel) SetupMembers(chat);
                SetupCommands(chat);
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.GetChat!");
                Header = null; // чтобы содержимое окна было скрыто
                Placeholder = PlaceholderViewModel.GetForException(ex, (o) => GetChat(peerId));
            }
            IsLoading = false;
        }

        private void SetupMembers(ChatInfoEx chat) {
            allMembers.Clear();
            DisplayedMembers = null;
            if (chat.Members == null) return;

            CacheManager.Add(chat.Members?.Profiles);
            CacheManager.Add(chat.Members?.Groups);

            foreach (var member in CollectionsMarshal.AsSpan(chat.Members.Items)) {
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

                allMembers.Add(new Entity(mid, avatar, name, desc, command));
                DisplayedMembers = allMembers;
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
                Router.OpenPeerProfile(session, member.MemberId);
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
                Command editCmd = new Command(VKIconNames.Icon20WriteOutline, Assets.i18n.Resources.edit, false, (a) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow));
                commands.Add(editCmd);
            }

            // Add member
            if (chat.ACL.CanInvite) {
                Command addCmd = new Command(VKIconNames.Icon20UserAddOutline, Assets.i18n.Resources.add, false, (a) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow));
                commands.Add(addCmd);
            }

            // Notifications
            bool notifsDisabled = chat.PushSettings != null && chat.PushSettings.DisabledForever;
            string notifIcon = notifsDisabled ? VKIconNames.Icon20NotificationSlashOutline : VKIconNames.Icon20NotificationOutline;
            Command notifsCmd = new Command(notifIcon, notifsDisabled ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled, false, (a) => ToggleNotifications(!notifsDisabled, chat.PeerId));
            commands.Add(notifsCmd);

            // Link
            if (chat.ACL.CanSeeInviteLink) {
                Command chatLinkCmd = new Command(VKIconNames.Icon20LinkCircleOutline, Assets.i18n.Resources.link, false, (a) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow));
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
            Command clearCmd = new Command(VKIconNames.Icon20DeleteOutline, Assets.i18n.Resources.chat_clear_history, true, (a) => ContextMenuHelper.TryClearChat(session, Id, Setup));
            moreCommands.Add(clearCmd);

            // Exit or return to chat/channel
            if (chat.State != UserStateInChat.Kicked) {
                string exitLabel = chat.IsChannel ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled;
                string returnLabel = chat.IsChannel ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled;
                string icon = chat.State == UserStateInChat.In ? VKIconNames.Icon20DoorArrowRightOutline : VKIconNames.Icon20DoorEnterArrowRightOutline;
                Command exitRetCmd = new Command(icon, chat.State == UserStateInChat.In ? exitLabel : returnLabel, true, (a) => {
                    if (chat.State == UserStateInChat.In) {
                        ContextMenuHelper.TryLeaveChat(session, Id, Setup);
                    } else {
                        ContextMenuHelper.ReturnToChat(session, Id, Setup);
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

        private void SearchMember() {
            if (IsLoading) return;
            if (!String.IsNullOrWhiteSpace(MemberSearchQuery)) {
                var foundMembers = allMembers.Where(m => m.Item3.ToLower().Contains(MemberSearchQuery.ToLower()));
                DisplayedMembers = new ObservableCollection<Entity>(foundMembers);
            } else {
                DisplayedMembers = allMembers;
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

        private async void ToggleNotifications(bool enabled, long id) {
            IsLoading = true;
            try {
                var result = await session.API.Account.SetSilenceModeAsync(!enabled ? 0 : -1, id, true);
                IsLoading = false;
                Setup(); // TODO: обновить кнопку, а не всё окно.
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.ToggleNotifications!");
                IsLoading = false;
                await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
            }
        }

        #endregion

        #region Conversation attachments

        public void LoadPhotos() {
            LoadVM(Photos, HistoryAttachmentMediaType.Photo);
        }

        public void LoadVideos() {
            LoadVM(Videos, HistoryAttachmentMediaType.Video);
        }

        public void LoadAudios() {
            LoadVM(Audios, HistoryAttachmentMediaType.Audio);
        }

        public void LoadDocs() {
            LoadVM(Documents, HistoryAttachmentMediaType.Doc);
        }

        public void LoadLinks() {
            LoadVM(Share, HistoryAttachmentMediaType.Share);
        }

        //public void LoadGraffities() {
        //    LoadVM(Graffities, HistoryAttachmentMediaType.Graffiti);
        //}

        //public void LoadAudioMessages() {
        //    LoadVM(AudioMessages, HistoryAttachmentMediaType.AudioMessage);
        //}

        private async void LoadVM(ConversationAttachmentsTabViewModel ivm, HistoryAttachmentMediaType type) {
            if (ivm.IsLoading || ivm.End) return;
            ivm.Placeholder = null;
            ivm.IsLoading = true;
            try {
                ConversationAttachmentsResponse resp = await session.API.Messages.GetHistoryAttachmentsAsync(session.GroupId, Id, type, ivm.StartFrom, 60, true, fields: VKAPIHelper.Fields);
                CacheManager.Add(resp.Profiles);
                CacheManager.Add(resp.Groups);
                resp.Items.ForEach(ivm.Items.Add);
                if (!String.IsNullOrEmpty(resp.NextFrom)) {
                    ivm.StartFrom = Int32.Parse(resp.NextFrom.Split('/')[0]) - 1;
                } else {
                    if (ivm.Items.Count > 0) ivm.End = true;
                }

                if (ivm.Items.Count == 0) {
                    ivm.Placeholder = new PlaceholderViewModel {
                        Text = Localizer.Get($"pp_attachments_{type}".ToLower()),
                        ActionButton = null
                    };
                    ivm.End = true;
                }
            } catch (Exception ex) {
                Log.Error(ex, $"Error in PeerProfileViewModel.LoadVM!");
                if (ivm.Items.Count == 0) {
                    ivm.Placeholder = PlaceholderViewModel.GetForException(ex, (o) => LoadVM(ivm, type));
                } else {
                    if (await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex)) LoadVM(ivm, type);
                }
            }
            ivm.IsLoading = false;
        }

        #endregion
    }
}