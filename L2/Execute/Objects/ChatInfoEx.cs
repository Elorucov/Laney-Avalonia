using ELOR.VKAPILib.Objects;
using System.Text.Json.Serialization;
using System;
using ELOR.VKAPILib.Attributes;

namespace ELOR.Laney.Execute.Objects {
    public class ChatInfoEx {
        public ChatInfoEx() {}

        [JsonPropertyName("chat_id")]
        public long ChatId { get; set; }

        [JsonPropertyName("peer_id")]
        public long PeerId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }

        [JsonIgnore]
        public Uri PhotoUri { get { return Uri.IsWellFormedUriString(Photo, UriKind.Absolute) ? new Uri(Photo) : null; } }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; }

        [JsonPropertyName("is_casper_chat")]
        public bool IsCasperChat { get; set; }

        [JsonPropertyName("is_channel")]
        public bool IsChannel { get; set; }

        [JsonPropertyName("members_count")]
        public int MembersCount { get; set; }

        [JsonPropertyName("online_count")]
        public int OnlineCount { get; set; }

        [JsonPropertyName("push_settings")]
        public PushSettings PushSettings { get; set; }

        [JsonPropertyName("acl")]
        public ChatACL ACL { get; set; }

        [JsonPropertyName("permissions")]
        public ChatPermissions Permissions { get; set; }

        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<UserStateInChat>))]
        public UserStateInChat State { get; set; }

        [JsonPropertyName("members")]
        public VKList<ChatMember> Members { get; set; }

        [JsonPropertyName("pinned_message")]
        public Message PinnedMessage { get; set; }
    }
}