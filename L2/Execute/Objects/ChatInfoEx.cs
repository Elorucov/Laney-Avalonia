using ELOR.VKAPILib.Objects;
using Newtonsoft.Json;
using System;

namespace ELOR.Laney.Execute.Objects {
    public class ChatInfoEx {
        [JsonProperty("chat_id")]
        public int ChatId { get; set; }

        [JsonProperty("peer_id")]
        public int PeerId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonIgnore]
        public Uri PhotoUri { get { return Uri.IsWellFormedUriString(Photo, UriKind.Absolute) ? new Uri(Photo) : null; } }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("is_casper_chat")]
        public bool IsCasperChat { get; set; }

        [JsonProperty("is_channel")]
        public bool IsChannel { get; set; }

        [JsonProperty("members_count")]
        public int MembersCount { get; set; }

        [JsonProperty("online_count")]
        public int OnlineCount { get; set; }

        [JsonProperty("push_settings")]
        public PushSettings PushSettings { get; set; }

        [JsonProperty("acl")]
        public ChatACL ACL { get; set; }

        [JsonProperty("permissions")]
        public ChatPermissions Permissions { get; set; }

        [JsonProperty("state")]
        public UserStateInChat State { get; set; }

        [JsonProperty("members")]
        public VKList<ChatMember> Members { get; set; }

        [JsonProperty("pinned_message")]
        public Message PinnedMessage { get; set; }
    }
}