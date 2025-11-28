using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ELOR.Laney.Execute.Objects {
    public class ChatMembersList : IVKList<ChatMember> {
        public ChatMembersList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<ChatMember> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("conversations")]
        public List<Conversation> Conversations { get; set; }
    }

    public class ChatInfoEx {
        public ChatInfoEx() { }

        [JsonPropertyName("chat_id")]
        public int ChatId { get; set; }

        [JsonPropertyName("peer_id")]
        public long PeerId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

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

        [JsonPropertyName("last_cmid")]
        public int LastCMID { get; set; }

        [JsonPropertyName("push_settings")]
        public PushSettings PushSettings { get; set; }

        [JsonPropertyName("acl")]
        public ChatACL ACL { get; set; }

        [JsonPropertyName("permissions")]
        public Dictionary<string, string> Permissions { get; set; }

        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<UserStateInChat>))]
        public UserStateInChat State { get; set; }

        [JsonPropertyName("pinned_message")]
        public Message PinnedMessage { get; set; }

        [JsonPropertyName("disable_service_messages")]
        public bool DisableServiceMessages { get; set; }
    }
}