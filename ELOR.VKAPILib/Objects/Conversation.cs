using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using ELOR.VKAPILib.Attributes;

namespace ELOR.VKAPILib.Objects {
    public class ConversationsResponse {
        public ConversationsResponse() {}

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<ConversationItem> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("unread_count")]
        public int UnreadCount { get; set; }
    }

    public class ConversationItem {
        public ConversationItem() {}

        [JsonPropertyName("conversation")]
        public Conversation Conversation { get; set; }

        [JsonPropertyName("last_message")]
        public Message LastMessage { get; set; }
    }

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

    public enum UserStateInChat {
        [EnumMember(Value = "in")]
        In,

        [EnumMember(Value = "kicked")]
        Kicked,

        [EnumMember(Value = "left")]
        Left,

        [EnumMember(Value = "out")]
        Out
    }

    public enum ChatSettingsChangers {
        [EnumMember(Value = "owner")]
        Owner,

        [EnumMember(Value = "owner_and_admins")]
        OwnerAndAdmins,

        [EnumMember(Value = "all")]
        All,
    }

    public class Peer {
        public Peer() {}

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<PeerType>))]
        public PeerType Type { get; set; }

        [JsonPropertyName("local_id")]
        public long LocalId { get; set; }
    }

    public class PushSettings {
        public PushSettings() {}

        [JsonPropertyName("disabled_until")]
        public long DisabledUntil { get; set; }

        [JsonPropertyName("disabled_forever")]
        public bool DisabledForever { get; set; }

        [JsonPropertyName("no_sound")]
        public bool NoSound { get; set; }
    }

    public class CanWrite {
        public CanWrite() {}

        [JsonPropertyName("allowed")]
        public bool Allowed { get; set; }

        [JsonPropertyName("reason")]
        public int Reason { get; set; }
    }

    public class ChatPhoto {
        public ChatPhoto() {}

        [JsonPropertyName("photo_50")]
        public string SmallUrl { get; set; }

        [JsonPropertyName("photo_100")]
        public string MediumUrl { get; set; }

        [JsonPropertyName("photo_200")]
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
        public ChatACL() {}

        [JsonPropertyName("can_change_info")]
        public bool CanChangeInfo { get; set; }

        [JsonPropertyName("can_change_invite_link")]
        public bool CanChangeInviteLink { get; set; }

        [JsonPropertyName("can_change_pin")]
        public bool CanChangePin { get; set; }

        [JsonPropertyName("can_invite")]
        public bool CanInvite { get; set; }

        [JsonPropertyName("can_promote_users")]
        public bool CanPromoteUsers { get; set; }

        [JsonPropertyName("can_see_invite_link")]
        public bool CanSeeInviteLink { get; set; }

        [JsonPropertyName("can_send_reactions")]
        public bool CanSendReactions { get; set; }
    }

    public class ChatPermissions {
        public ChatPermissions() {}

        [JsonPropertyName("invite")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<ChatSettingsChangers>))]
        public ChatSettingsChangers Invite { get; set; }

        [JsonPropertyName("change_info")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<ChatSettingsChangers>))]
        public ChatSettingsChangers ChangeInfo { get; set; }

        [JsonPropertyName("change_pin")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<ChatSettingsChangers>))]
        public ChatSettingsChangers ChangePin { get; set; }

        [JsonPropertyName("use_mass_mentions")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<ChatSettingsChangers>))]
        public ChatSettingsChangers UseMassMentions { get; set; }

        [JsonPropertyName("see_invite_link")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<ChatSettingsChangers>))]
        public ChatSettingsChangers SeeInviteLink { get; set; }

        [JsonPropertyName("call")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<ChatSettingsChangers>))]
        public ChatSettingsChangers Call { get; set; }

        [JsonPropertyName("change_admins")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<ChatSettingsChangers>))]
        public ChatSettingsChangers ChangeAdmins { get; set; }
    }

    public class ChatSettings {
        public ChatSettings() {}

        [JsonPropertyName("members_count")]
        public int MembersCount { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("acl")]
        public ChatACL ACL { get; set; }

        [JsonPropertyName("is_group_channel")]
        public bool IsGroupChannel { get; set; }

        [JsonPropertyName("pinned_message")]
        public Message PinnedMessage { get; set; }

        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<UserStateInChat>))]
        public UserStateInChat State { get; set; }

        [JsonPropertyName("photo")]
        public ChatPhoto Photo { get; set; }

        [JsonPropertyName("active_ids")]
        public List<long> ActiveIDs { get; set; }

        [JsonPropertyName("admin_ids")]
        public List<long> AdminIDs { get; set; }

        [JsonPropertyName("is_disappearing")]
        public bool IsDisappearing { get; set; }

        [JsonPropertyName("permissions")]
        public ChatPermissions Permissions { get; set; }

        [JsonPropertyName("theme")]
        public string Theme { get; set; }
    }

    public class SortId {
        public SortId() {}

        [JsonPropertyName("major_id")]
        public int MajorId { get; set; }

        [JsonPropertyName("minor_id")]
        public int MinorId { get; set; }
    }

    public class Conversation {
        public Conversation() {}

        [JsonPropertyName("peer")]
        public Peer Peer { get; set; }

        [JsonPropertyName("in_read")]
        public int InRead { get; set; }

        [JsonPropertyName("out_read")]
        public int OutRead { get; set; }

        [JsonPropertyName("in_read_cmid")]
        public int InReadCMID { get; set; }

        [JsonPropertyName("out_read_cmid")]
        public int OutReadCMID { get; set; }

        [JsonPropertyName("unread_count")]
        public int UnreadCount { get; set; }

        [JsonPropertyName("is_marked_unread")]
        public bool IsMarkedUnread { get; set; }

        [JsonPropertyName("important")]
        public bool Important { get; set; }

        [JsonPropertyName("unanswered")]
        public bool Unanswered { get; set; }

        [JsonPropertyName("sort_id")]
        public SortId SortId { get; set; }

        [JsonPropertyName("push_settings")]
        public PushSettings PushSettings { get; set; }

        [JsonPropertyName("can_write")]
        public CanWrite CanWrite { get; set; }

        [JsonPropertyName("current_keyboard")]
        public BotKeyboard CurrentKeyboard { get; set; }

        [JsonPropertyName("chat_settings")]
        public ChatSettings ChatSettings { get; set; }

        [JsonPropertyName("mention_cmids")]
        public List<int> Mentions { get; set; }

        [JsonPropertyName("expire_cmids")]
        public List<int> ExpireConvMessageIds { get; set; }

        [JsonPropertyName("unread_reactions")]
        public List<int> UnreadReactions { get; set; }
    }

    // Chat

    public class ChatMember {
        public ChatMember() {}

        [JsonPropertyName("member_id")]
        public long MemberId { get; set; }

        [JsonPropertyName("join_date")]
        public int JoinDateUnix { get; set; }

        [JsonIgnore]
        public DateTime JoinDate { get { return DateTimeOffset.FromUnixTimeSeconds(JoinDateUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("invited_by")]
        public long InvitedBy { get; set; }

        [JsonPropertyName("can_kick")]
        public bool CanKick { get; set; }

        [JsonPropertyName("is_admin")]
        public bool IsAdmin { get; set; }
    }

    public class Chat {
        public Chat() {}

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("admin_id")]
        public long AdminId { get; set; }

        [JsonPropertyName("users")]
        public List<long> Users { get; set; }

        [JsonPropertyName("members_count")]
        public int MembersCount { get; set; }

        [JsonPropertyName("push_settings")]
        public PushSettings PushSettings { get; set; }

        [JsonPropertyName("photo_200")]
        public string Photo { get; set; }

        [JsonPropertyName("left")]
        public bool Left { get; set; }

        [JsonPropertyName("kicked")]
        public bool Kicked { get; set; }
    }

    public class ChatLink {
        public ChatLink() {}

        [JsonPropertyName("link")]
        public string Link { get; set; }
    }
}