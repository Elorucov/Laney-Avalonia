
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class ChatPreview {
        public ChatPreview() { }

        [JsonPropertyName("admin_id")]
        public long AdminId { get; set; }

        [JsonPropertyName("members_count")]
        public int MembersCount { get; set; }

        [JsonPropertyName("members")]
        public List<long> Members { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("photo")]
        public Photo Photo { get; set; }

        [JsonPropertyName("local_id")]
        public long LocalId { get; set; }

        [JsonPropertyName("joined")]
        public bool Joined { get; set; }

        [JsonPropertyName("is_group_channel")]
        public bool IsGroupChannel { get; set; }
    }

    public class ChatPreviewResponse {
        public ChatPreviewResponse() { }

        [JsonPropertyName("preview")]
        public ChatPreview Preview { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }
}
