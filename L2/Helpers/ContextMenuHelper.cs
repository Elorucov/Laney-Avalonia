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

namespace ELOR.Laney.Helpers {
    public class ContextMenuHelper {
        public static void ShowForMessage(MessageViewModel message, ChatViewModel chat, Control target) {
            if (chat.PeerId != message.PeerId) return;
            ActionSheet ash = new ActionSheet();

            ActionSheetItem debug = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20BugOutline },
                Header = $"{message.Id} - {message.ConversationMessageId}"
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

            bool canReplyPrivately = chat.PeerType == PeerType.Chat && message.SenderId > 0 && message.SenderId != session.Id;
            if (message.SenderId > 0) {
                User sender = CacheManager.GetUser(message.SenderId);
                if (sender != null) canReplyPrivately = sender.CanWritePrivateMessage;
            }

            bool isAdminInChat = false;
            if (chat.ChatSettings?.AdminIDs != null) isAdminInChat = chat.ChatSettings.AdminIDs.Contains(session.Id);

            bool canPin = chat.ChatSettings != null ? chat.ChatSettings.ACL.CanChangePin : false;
            bool isMessagePinned = chat.ChatSettings?.PinnedMessage != null 
                ? chat.ChatSettings.PinnedMessage.Id == message.Id : false;

            bool canEdit = message.CanEdit(session.Id);

            // Actions

            reply.Click += (a, b) => chat.Composer.AddReply(message);
            forwardHere.Click += (a, b) => {
                chat.Composer.Clear();
                chat.Composer.AddForwardedMessages(new List<MessageViewModel> { message });
            };
            edit.Click += (a, b) => chat.Composer.StartEditing(message);

            // ¯\_(ツ)_/¯

            if (Settings.ShowDevItemsInContextMenus) ash.Items.Add(debug);
            if (message.Action == null) {
                if (ash.Items.Count > 0) ash.Items.Add(new ActionSheetItem());
                if (chat.CanWrite.Allowed) ash.Items.Add(reply);
                if (canReplyPrivately && chat.PeerType == PeerType.Chat) ash.Items.Add(repriv);
                ash.Items.Add(forward);
                // if (chat.CanWrite.Allowed) ash.Items.Add(forwardHere);
                if (!message.IsImportant) ash.Items.Add(mark);
                if (message.IsImportant) ash.Items.Add(unmark);
                if (canPin && !isMessagePinned) ash.Items.Add(pin);
                if (canPin && isMessagePinned) ash.Items.Add(unpin);
                if (canEdit) ash.Items.Add(edit);
                ash.Items.Add(spam);
                ash.Items.Add(delete);
            }
            if (ash.Items.Count > 0) ash.ShowAt(target, true);
        }

        public static void ShowForMultipleMessages(List<MessageViewModel> messages, Control target) {
            ActionSheet ash = new ActionSheet { 
                Placement = FlyoutPlacementMode.LeftEdgeAlignedTop
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
            var isAllMessagesMarkedAsImportant = messages.Where(m => m.IsImportant).Count() == messages.Count;

            // Actions


            
            // ¯\_(ツ)_/¯

            ash.Items.Add(spam);
            ash.Items.Add(delete);
            ash.Items.Add(new ActionSheetItem());
            if (!isAllMessagesMarkedAsImportant) ash.Items.Add(mark);
            if (isAllMessagesMarkedAsImportant) ash.Items.Add(unmark);

            if (ash.Items.Count > 0) ash.ShowAt(target, true);
        }
    }
}