using Avalonia.Controls;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using ELOR.Laney.ViewModels.Controls;
using VKUI.Controls;
using VKUI.Popups;
using ELOR.VKAPILib.Objects;
using System.Collections.Generic;
using System.Linq;
using System;
using ELOR.Laney.Views.Modals;
using Avalonia.Controls.Notifications;

namespace ELOR.Laney.Helpers {
    public class ContextMenuHelper {
        #region For chat

        public static void ShowForChat(ChatViewModel chat, Control target) {
            ActionSheet ash = new ActionSheet();

            ActionSheetItem debug = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20BugOutline },
                Header = $"ID: {chat.PeerId} ({chat.PeerType})"
            };
            ActionSheetItem read = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20MessageOutline },
                Header = Localizer.Instance["mark_read"],
            };
            ActionSheetItem unread = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20MessageUnreadTopOutline },
                Header = Localizer.Instance["mark_unread"],
            };
            ActionSheetItem notifon = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20NotificationOutline },
                Header = Localizer.Instance["notifications_enable"],
            };
            ActionSheetItem notifoff = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20NotificationSlashOutline },
                Header = Localizer.Instance["notifications_disable"],
            };
            ActionSheetItem clear = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20DeleteOutline },
                Header = Localizer.Instance["chat_clear_history"],
            };
            ActionSheetItem leave = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20DoorArrowRightOutline },
                Header = Localizer.Instance[chat.ChatSettings?.IsGroupChannel == true ? "pp_exit_channel" : "pp_exit_chat"],
            };
            ActionSheetItem creturn = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20DoorEnterArrowRightOutline },
                Header = Localizer.Instance[chat.ChatSettings?.IsGroupChannel == true ? "pp_return_channel" : "pp_return_chat"],
            };
            ActionSheetItem gdeny = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20BlockOutline },
                Header = Localizer.Instance["pp_deny"],
            };

            clear.Classes.Add("Destructive");
            leave.Classes.Add("Destructive");
            gdeny.Classes.Add("Destructive");

            // Conditions

            var session = VKSession.GetByDataContext(target);

            bool notificationsDisabled = chat.PushSettings.DisabledForever || chat.PushSettings.DisabledUntil > DateTimeOffset.Now.ToUnixTimeSeconds();

            // Actions

            notifon.Click += async (a, b) => {
                try {
                    var result = await session.API.Account.SetSilenceModeAsync(0, chat.PeerId, true);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
                }
            };

            notifoff.Click += async (a, b) => {
                try {
                    var result = await session.API.Account.SetSilenceModeAsync(-1, chat.PeerId, true);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
                }
            };

            read.Click += async (a, b) => {
                try {
                    var result = await session.API.Messages.MarkAsReadAsync(session.GroupId, chat.PeerId, chat.LastMessage.ConversationMessageId, true);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
                }
            };

            unread.Click += async (a, b) => {
                try {
                    var result = await session.API.Messages.MarkAsUnreadConversationAsync(session.GroupId, chat.PeerId);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex);
                }
            };

            clear.Click += (a, b) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);
            leave.Click += (a, b) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);
            creturn.Click += (a, b) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);
            gdeny.Click += (a, b) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);

            // ¯\_(ツ)_/¯

            if (Settings.ShowDevItemsInContextMenus) ash.Items.Add(debug);
            if (ash.Items.Count > 0) ash.Items.Add(new ActionSheetItem());

            if (chat.UnreadMessagesCount > 0 || chat.IsMarkedAsUnread) ash.Items.Add(read);
            if (chat.UnreadMessagesCount == 0 && !chat.IsMarkedAsUnread) ash.Items.Add(unread);

            if (chat.PeerId != session.Id) {
                if (!notificationsDisabled) ash.Items.Add(notifoff);
                if (notificationsDisabled) ash.Items.Add(notifon);
            }

            if (!session.IsGroup && chat.PeerType == PeerType.Chat && chat.ChatSettings != null) {
                if (chat.ChatSettings.State == UserStateInChat.In) ash.Items.Add(leave);
                if (chat.ChatSettings.State == UserStateInChat.Left) ash.Items.Add(creturn);
            }

            // TODO: Запретить сообщения для диалога с группой.

            if (chat.PeerId != session.Id) ash.Items.Add(clear);

            if (ash.Items.Count > 0) ash.ShowAt(target, true);
        }

        #endregion


        #region For message

        public static void ShowForMessage(MessageViewModel message, ChatViewModel chat, Control target) {
            if (chat.PeerId != message.PeerId) return;
            ActionSheet ash = new ActionSheet();

            ActionSheetItem debug = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20BugOutline },
                Header = $"ID: {message.Id}, CMID: {message.ConversationMessageId}"
            };
            ActionSheetItem reply = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20ReplyOutline },
                Header = Localizer.Instance["reply"]
            };
            ActionSheetItem repriv = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20ReplyOutline },
                Header = Localizer.Instance["reply_privately"]
            };
            ActionSheetItem forward = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20ShareOutline },
                Header = Localizer.Instance["forward"]
            };
            ActionSheetItem forwardHere = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20ShareOutline },
                Header = Localizer.Instance["forward_here"]
            };
            ActionSheetItem mark = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20FavoriteOutline },
                Header = Localizer.Instance["mark_important"],
            };
            ActionSheetItem unmark = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20UnfavoriteOutline },
                Header = Localizer.Instance["unmark_important"],
            };
            ActionSheetItem pin = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20PinOutline },
                Header = Localizer.Instance["pin"],
            };
            ActionSheetItem unpin = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20PinSlashOutline },
                Header = Localizer.Instance["unpin"],
            };
            ActionSheetItem edit = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20WriteOutline },
                Header = Localizer.Instance["edit"],
            };
            ActionSheetItem spam = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20ReportOutline },
                Header = Localizer.Instance["mark_spam"],
            };
            ActionSheetItem delete = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20DeleteOutline },
                Header = Localizer.Instance["delete"],
            };
            spam.Classes.Add("Destructive");
            delete.Classes.Add("Destructive");

            // Conditions

            var session = VKSession.GetByDataContext(target);

            bool canReplyPrivately = chat.PeerType == PeerType.Chat && message.SenderId.IsUser() && message.SenderId != session.Id;
            if (message.SenderId.IsUser()) {
                User sender = CacheManager.GetUser(message.SenderId);
                if (sender != null) canReplyPrivately = sender.CanWritePrivateMessage == 1;
            }

            bool isAdminInChat = false;
            if (chat.ChatSettings?.AdminIDs != null) isAdminInChat = chat.ChatSettings.AdminIDs.Contains(session.Id);

            bool canPin = chat.ChatSettings != null ? chat.ChatSettings.ACL.CanChangePin : false;
            bool isMessagePinned = chat.PinnedMessage != null 
                ? chat.PinnedMessage.Id == message.Id : false;

            bool canEdit = message.CanEdit(session.Id);

            bool canDeleteWithoutConfirmation = message.SenderId != session.Id || chat.PeerId == session.Id;
            bool canDeleteForAll = message.SenderId == session.Id && message.PeerId != message.SenderId
                && message.SentTime > DateTime.Now.AddDays(-1);

            // Actions

            reply.Click += (a, b) => chat.Composer.AddReply(message);

            repriv.Click += (a, b) => {
                session.GoToChat(message.SenderId);
                session.CurrentOpenedChat.Composer.AddForwardedMessages(chat.PeerId, new List<MessageViewModel> { message });
            };

            forward.Click += (a, b) => session.Share(chat.PeerId, new List<MessageViewModel> { message });

            forwardHere.Click += (a, b) => {
                chat.Composer.Clear();
                chat.Composer.AddForwardedMessages(chat.PeerId, new List<MessageViewModel> { message });
            };

            edit.Click += (a, b) => chat.Composer.StartEditing(message);

            mark.Click += async (a, b) => {
                try {
                    var response = await session.API.Messages.MarkAsImportantAsync(message.PeerId, new List<int> { message.ConversationMessageId }, true);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
                }
            };

            unmark.Click += async (a, b) => {
                try {
                    var response = await session.API.Messages.MarkAsImportantAsync(message.PeerId, new List<int> { message.ConversationMessageId }, false);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
                }
            };

            pin.Click += async (a, b) => {
                try {
                    var response = await session.API.Messages.PinAsync(session.GroupId, chat.PeerId, message.ConversationMessageId);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
                }
            };

            unpin.Click += async (a, b) => {
                try {
                    var response = await session.API.Messages.UnpinAsync(session.GroupId, chat.PeerId);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
                }
            };

            spam.Click += (a, b) => DeleteMessages(session, chat.PeerId, new List<int> { message.ConversationMessageId }, false, true);
            delete.Click += (a, b) => TryDeleteMessages(canDeleteWithoutConfirmation, session, chat.PeerId, new List<int> { message.ConversationMessageId }, canDeleteForAll);

            // ¯\_(ツ)_/¯

            if (Settings.ShowDevItemsInContextMenus) ash.Items.Add(debug);
            if (message.Action == null && !message.IsExpired) {
                if (ash.Items.Count > 0) ash.Items.Add(new ActionSheetItem());
                if (chat.CanWrite.Allowed) ash.Items.Add(reply);
                if (canReplyPrivately && chat.PeerType == PeerType.Chat) ash.Items.Add(repriv);
                ash.Items.Add(forward);
                // if (chat.CanWrite.Allowed) ash.Items.Add(forwardHere);
                if (!session.IsGroup && !message.IsImportant) ash.Items.Add(mark);
                if (!session.IsGroup && message.IsImportant) ash.Items.Add(unmark);
                if (canPin && !isMessagePinned) ash.Items.Add(pin);
                if (canPin && isMessagePinned) ash.Items.Add(unpin);
                if (canEdit) ash.Items.Add(edit);
                if (message.SenderId != session.Id) ash.Items.Add(spam);
                ash.Items.Add(delete);
            }
            if (ash.Items.Count > 0) ash.ShowAt(target, true);
        }

        public static void ShowForMultipleMessages(List<MessageViewModel> messages, ChatViewModel chat, Control target) {
            ActionSheet ash = new ActionSheet { 
                Placement = PlacementMode.LeftEdgeAlignedTop
            };

            ActionSheetItem mark = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20FavoriteOutline },
                Header = Localizer.Instance["mark_important"],
            };
            ActionSheetItem unmark = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20UnfavoriteOutline },
                Header = Localizer.Instance["unmark_important"],
            };
            ActionSheetItem spam = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20ReportOutline },
                Header = Localizer.Instance["mark_spam"],
            };
            ActionSheetItem delete = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20DeleteOutline },
                Header = Localizer.Instance["delete"],
            };
            spam.Classes.Add("Destructive");
            delete.Classes.Add("Destructive");

            // Conditions

            var session = VKSession.GetByDataContext(target);
            bool isAllMessagesMarkedAsImportant = messages.Where(m => m.IsImportant).Count() == messages.Count;
            bool canDeleteForAll = messages.All(m => m.SenderId == session.Id && m.PeerId != m.SenderId
                && m.SentTime > DateTime.Now.AddDays(-1));
            bool canDeleteWithoutConfirmation = messages.All(m => m.SenderId != session.Id || chat.PeerId == session.Id);
            bool spamAvailable = messages.All(m => m.SenderId != session.Id);

            // Actions

            mark.Click += async (a, b) => {
                try {
                    var response = await session.API.Messages.MarkAsImportantAsync(chat.PeerId, messages.Select(m => m.ConversationMessageId).ToList(), true);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
                }
            };
            unmark.Click += async (a, b) => {
                try {
                    var response = await session.API.Messages.MarkAsImportantAsync(chat.PeerId, messages.Select(m => m.ConversationMessageId).ToList(), false);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
                }
            };

            spam.Click += (a, b) => DeleteMessages(session, chat.PeerId, messages.Select(m => m.ConversationMessageId).ToList(), false, true);
            delete.Click += (a, b) => TryDeleteMessages(canDeleteWithoutConfirmation, session, chat.PeerId, messages.Select(m => m.ConversationMessageId).ToList(), canDeleteForAll);

            // ¯\_(ツ)_/¯

            if (spamAvailable) ash.Items.Add(spam);
            ash.Items.Add(delete);
            ash.Items.Add(new ActionSheetItem());
            if (!isAllMessagesMarkedAsImportant) ash.Items.Add(mark);
            if (isAllMessagesMarkedAsImportant) ash.Items.Add(unmark);

            if (ash.Items.Count > 0) ash.ShowAt(target, true);
        }

        private static async void TryDeleteMessages(bool withoutConfirmation, VKSession session, long peerId, List<int> ids, bool canDeleteForAll) {
            if (withoutConfirmation) {
                DeleteMessages(session, peerId, ids, false, false);
            } else {
                string title, subtitle = String.Empty;

                if (ids.Count == 1) {
                    title = Localizer.Instance[canDeleteForAll ? "msg_delete_dialog_single_question" : "msg_delete_dialog_single_title"];
                } else {
                    title = Localizer.Instance[canDeleteForAll ? "msg_delete_dialog_multi_question" : "msg_delete_dialog_multi_title"];
                }
                subtitle = canDeleteForAll ? String.Empty : Localizer.Instance.GetDeclensionFormatted(ids.Count, "msg_delete_dialog_text");

                VKUIDialog dlg = new VKUIDialog(title, subtitle, [Localizer.Instance["yes"], Localizer.Instance["no"]], 2);
                CheckBox forAll = new CheckBox { Content = Localizer.Instance["delete_for_all"] };

                if (canDeleteForAll) dlg.DialogContent = forAll;
                int result = await dlg.ShowDialog<int>(session.ModalWindow);
                if (result == 1) DeleteMessages(session, peerId, ids, forAll.IsChecked.Value, false);
            }
        }

        private static async void DeleteMessages(VKSession session, long peerId, List<int> ids, bool forAll, bool spam) {
            try {
                var response = await session.API.Messages.DeleteAsync(session.GroupId, peerId, ids, spam, forAll);
                int count = response.Where(r => r.Response == 1).Count();

                string type = spam ? "spam" : "deleted";
                string multi = count > 1 ? "multi" : "single";
                session.ShowNotification(new Notification(Localizer.Instance[$"message_{type}_{multi}"], null, NotificationType.Success));
            } catch (Exception ex) {
                await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
            }
        }

        #endregion
    }
}