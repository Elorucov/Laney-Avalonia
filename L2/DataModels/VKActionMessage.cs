using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;

namespace ELOR.Laney.DataModels {
    public enum VKActionObjectType { Member, ConversationMessage }

    public sealed class VKActionMessage {
        public long InitiatorId { get; private set; }
        public string InitiatorDisplayName { get; private set; }
        public string ActionText { get; private set; }
        public VKActionObjectType ObjectType { get; private set; }
        public long ObjectId { get; private set; }
        public string ObjectDisplayName { get; private set; }
        public string MessageText { get; private set; }
        public string Suffix { get; private set; }

        public VKActionMessage(ELOR.VKAPILib.Objects.Action act, long fromId = 0) {
            string actionerName = "";
            Sex actionerSex = Sex.Male;
            string memberName = "";
            string memberNameGen = "";
            Sex memberSex = Sex.Male;

            if (fromId == 0) fromId = act.FromId;

            if (fromId.IsUser()) {
                User u = CacheManager.GetUser(fromId);
                actionerName = u.FullName;
                actionerSex = u.Sex;
            } else if (fromId.IsGroup()) {
                Group g = CacheManager.GetGroup(fromId);
                actionerName = g.Name;
            }

            if (act.MemberId != 0) ObjectId = act.MemberId;
            if (act.MemberId.IsUser()) {
                User u = CacheManager.GetUser(act.MemberId);
                memberName = u.FullName;
                memberNameGen = $"{u.FirstNameAcc} {u.LastNameAcc}";
                memberSex = u.Sex;
            } else if (act.MemberId.IsGroup()) {
                Group g = CacheManager.GetGroup(act.MemberId);
                memberName = g.Name;
                memberNameGen = g.Name;
            } else if (act.MemberId == 0 && act.ConversationMessageId > 0) {
                ObjectType = VKActionObjectType.ConversationMessage;
                ObjectId = act.ConversationMessageId;
            }

            InitiatorId = fromId;
            InitiatorDisplayName = actionerName;

            string create = Localizer.Get("msg_action_create", actionerSex);
            string invited = Localizer.Get("msg_action_invited", actionerSex);
            string returned = Localizer.Get("msg_action_returned", actionerSex);
            string invitedlink = Localizer.Get("msg_action_invited_by_link", actionerSex);
            string left = Localizer.Get("msg_action_left", memberSex);
            string kicked = Localizer.Get("msg_action_kick", actionerSex);
            string photoupd = Localizer.Get("msg_action_photo_update", actionerSex);
            string photorem = Localizer.Get("msg_action_photo_remove", actionerSex);
            string pin = Localizer.Get("msg_action_pin", actionerSex);
            string unpin = Localizer.Get("msg_action_unpin", actionerSex);
            string rename = Localizer.Get("msg_action_rename", actionerSex);
            string screenshot = Localizer.Get("msg_action_screenshot", actionerSex);
            string acceptedmsgrequest = Localizer.Get("msg_action_accepted_message_request", memberSex);
            string inviteuserbycall = Localizer.Get("msg_action_invite_user_by_call", actionerSex);
            string inviteuserbycalljoinlink = Localizer.Get("msg_action_invite_user_by_call_join_link", actionerSex);
            string inviteuserbycallsuffix = !String.IsNullOrWhiteSpace(Assets.i18n.Resources.msg_action_invite_user_by_call) ? $" {Assets.i18n.Resources.msg_action_invite_user_by_call}" : String.Empty;
            string styleupdate = String.IsNullOrEmpty(act.Style) ? Localizer.Get("msg_action_style_reset", actionerSex) : $"{Localizer.Get("msg_action_style_update", actionerSex)} «{act.Style}»";

            switch (act.Type) {
                case "chat_create":
                    ActionText = $"{create} \"{act.Text}\"";
                    break;
                case "chat_invite_user":
                    ActionText = fromId == act.MemberId ? returned : invited;
                    if (fromId != act.MemberId) {
                        ObjectId = act.MemberId;
                        ObjectDisplayName = memberNameGen;
                    }
                    break;
                case "chat_invite_user_by_link":
                    ActionText = invitedlink;
                    break;
                case "chat_kick_user":
                    ActionText = fromId == act.MemberId ? left : kicked;
                    if (fromId != act.MemberId) {
                        ObjectId = act.MemberId;
                        ObjectDisplayName = memberNameGen;
                    }
                    break;
                case "chat_photo_remove": ActionText = photorem; break;
                case "chat_photo_update": ActionText = photoupd; break;
                case "chat_title_update": ActionText = $"{rename} \"{act.Text}\""; break;
                case "chat_pin_message":
                    InitiatorId = act.MemberId;
                    InitiatorDisplayName = memberName;
                    ActionText = pin;
                    ObjectType = VKActionObjectType.ConversationMessage;
                    ObjectId = act.ConversationMessageId;
                    ObjectDisplayName = Assets.i18n.Resources.message.ToLower();
                    MessageText = act.Message;
                    break;
                case "chat_unpin_message":
                    InitiatorId = act.MemberId;
                    InitiatorDisplayName = memberName;
                    ActionText = unpin;
                    ObjectType = VKActionObjectType.ConversationMessage;
                    ObjectId = act.ConversationMessageId;
                    ObjectDisplayName = Assets.i18n.Resources.message.ToLower();
                    break;
                case "chat_screenshot":
                    InitiatorId = act.MemberId;
                    InitiatorDisplayName = memberName;
                    ActionText = screenshot;
                    break;
                case "accepted_message_request":
                    ActionText = acceptedmsgrequest;
                    break;
                case "chat_invite_user_by_call":
                    ActionText = inviteuserbycall;
                    ObjectId = act.MemberId;
                    ObjectDisplayName = memberNameGen;
                    Suffix = inviteuserbycallsuffix;
                    break;
                case "chat_invite_user_by_call_join_link":
                    ActionText = inviteuserbycalljoinlink;
                    break;
                case "conversation_style_update":
                    ActionText = styleupdate;
                    break;
            }
        }

        public override string ToString() {
            return String.Join(" ", new List<string> { InitiatorDisplayName, ActionText, ObjectDisplayName, Suffix }).Trim();
        }
    }
}