using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    public class ConversationsResponse : VKList<ConversationItem> {
        [JsonProperty("unread_count")]
        public int UnreadCount { get; set; }
    }

    public class ConversationItem {
        [JsonProperty("conversation")]
        public Conversation Conversation { get; set; }

        [JsonProperty("last_message")]
        public Message LastMessage { get; set; }
    }

    [DataContract]
    public enum PeerType {
        [EnumMember(Value = "user")]
        User,

        [EnumMember(Value = "chat")]
        Chat,

        [EnumMember(Value = "group")]
        Group,

        [EnumMember(Value = "email")]
        Email,

        [EnumMember(Value = "contact")]
        Contact
    }

    [DataContract]
    public enum UserStateInChat {
        [EnumMember(Value = "in")]
        In,

        [EnumMember(Value = "kicked")]
        Kicked,

        [EnumMember(Value = "left")]
        Left,
    }

    [DataContract]
    public enum ChatSettingsChangers {
        [EnumMember(Value = "owner")]
        Owner,

        [EnumMember(Value = "owner_and_admins")]
        OwnerAndAdmins,

        [EnumMember(Value = "all")]
        All,
    }

    public class Peer {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public PeerType Type { get; set; }

        [JsonProperty("local_id")]
        public string LocalId { get; set; }
    }

    public class PushSettings {
        [JsonProperty("disabled_until")]
        public int DisabledUntil { get; set; }

        [JsonProperty("disabled_forever")]
        public bool DisabledForever { get; set; }

        [JsonProperty("no_sound")]
        public bool NoSound { get; set; }
    }

    public class CanWrite {
        [JsonProperty("allowed")]
        public bool Allowed { get; set; }

        [JsonProperty("reason")]
        public int Reason { get; set; }
    }

    public class ChatPhoto {
        [JsonProperty("photo_50")]
        public string SmallUrl { get; set; }

        [JsonProperty("photo_100")]
        public string MediumUrl { get; set; }

        [JsonProperty("photo_200")]
        public string BigUrl { get; set; }

        [JsonIgnore]
        public Uri Uri {
            get {
                if (!String.IsNullOrEmpty(BigUrl)) return new Uri(BigUrl);
                if (!String.IsNullOrEmpty(MediumUrl)) return new Uri(MediumUrl);
                if (!String.IsNullOrEmpty(SmallUrl)) return new Uri(SmallUrl);
                return new Uri("https://vk.com/images/icons/im_multichat_200.png");
            }
        }
    }

    public class ChatACL {
        [JsonProperty("can_change_info")]
        public bool CanChangeInfo { get; set; }

        [JsonProperty("can_change_invite_link")]
        public bool CanChangeInviteLink { get; set; }

        [JsonProperty("can_change_pin")]
        public bool CanChangePin { get; set; }

        [JsonProperty("can_invite")]
        public bool CanInvite { get; set; }

        [JsonProperty("can_promote_users")]
        public bool CanPromoteUsers { get; set; }

        [JsonProperty("can_see_invite_link")]
        public bool CanSeeInviteLink { get; set; }
    }

    public class ChatPermissions {
        [JsonProperty("invite")]
        public ChatSettingsChangers Invite { get; set; }

        [JsonProperty("change_info")]
        public ChatSettingsChangers ChangeInfo { get; set; }

        [JsonProperty("change_pin")]
        public ChatSettingsChangers ChangePin { get; set; }

        [JsonProperty("use_mass_mentions")]
        public ChatSettingsChangers UseMassMentions { get; set; }

        [JsonProperty("see_invite_link")]
        public ChatSettingsChangers SeeInviteLink { get; set; }

        [JsonProperty("call")]
        public ChatSettingsChangers Call { get; set; }

        [JsonProperty("change_admins")]
        public ChatSettingsChangers ChangeAdmins { get; set; }
    }

    public class ChatSettings {
        [JsonProperty("members_count")]
        public int MembersCount { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("acl")]
        public ChatACL ACL { get; set; }

        [JsonProperty("is_group_channel")]
        public bool IsGroupChannel { get; set; }

        [JsonProperty("pinned_message")]
        public Message PinnedMessage { get; set; }

        [JsonProperty("state")]
        public UserStateInChat State { get; set; }

        [JsonProperty("photo")]
        public ChatPhoto Photo { get; set; }

        [JsonProperty("active_ids")]
        public List<string> ActiveIDs { get; set; }

        [JsonProperty("is_disappearing")]
        public bool IsDisappearing { get; set; }

        [JsonProperty("permissions")]
        public ChatPermissions Permissions { get; set; }

        [JsonProperty("theme")]
        public string Theme { get; set; }
    }

    public class SortId {
        [JsonProperty("major_id")]
        public int MajorId { get; set; }

        [JsonProperty("minor_id")]
        public int MinorId { get; set; }
    }

    public class Conversation {

        [JsonProperty("peer")]
        public Peer Peer { get; set; }

        [JsonProperty("in_read")]
        public int InRead { get; set; }

        [JsonProperty("out_read")]
        public int OutRead { get; set; }

        [JsonProperty("unread_count")]
        public int UnreadCount { get; set; }

        [JsonProperty("is_marked_unread")]
        public bool IsMarkedUnread { get; set; }

        [JsonProperty("important")]
        public bool Important { get; set; }

        [JsonProperty("unanswered")]
        public bool Unanswered { get; set; }

        [JsonProperty("sort_id")]
        public SortId SortId { get; set; }

        [JsonProperty("push_settings")]
        public PushSettings PushSettings { get; set; }

        [JsonProperty("can_write")]
        public CanWrite CanWrite { get; set; }

        [JsonProperty("current_keyboard")]
        public BotKeyboard CurrentKeyboard { get; set; }

        [JsonProperty("chat_settings")]
        public ChatSettings ChatSettings { get; set; }

        [JsonProperty("mentions")]
        public List<int> Mentions { get; set; }
    }

    // Chat

    public class ChatMember {
        [JsonProperty("member_id")]
        public int MemberId { get; set; }

        [JsonProperty("join_date")]
        public int JoinDateUnix { get; set; }

        [JsonIgnore]
        public DateTime JoinDate { get { return DateTimeOffset.FromUnixTimeSeconds(JoinDateUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("invited_by")]
        public int InvitedBy { get; set; }

        [JsonProperty("can_kick")]
        public bool CanKick { get; set; }

        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }
    }

    public class Chat {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("admin_id")]
        public int AdminId { get; set; }

        [JsonProperty("users")]
        public List<int> Users { get; set; }

        [JsonProperty("members_count")]
        public int MembersCount { get; set; }

        [JsonProperty("push_settings")]
        public PushSettings PushSettings { get; set; }

        [JsonProperty("photo_200")]
        public string Photo { get; set; }

        [JsonProperty("left")]
        public bool Left { get; set; }

        [JsonProperty("kicked")]
        public bool Kicked { get; set; }
    }

    public class ChatLink {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
