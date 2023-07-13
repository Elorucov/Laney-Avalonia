
using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects {
    public class ChatPreview {
        [JsonProperty("admin_id")]
        public long AdminId { get; set; }

        [JsonProperty("members_count")]
        public int MembersCount { get; set; }

        [JsonProperty("members")]
        public List<long> Members { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("photo")]
        public Photo Photo { get; set; }

        [JsonProperty("local_id")]
        public long LocalId { get; set; }

        [JsonProperty("joined")]
        public bool Joined { get; set; }

        [JsonProperty("is_group_channel")]
        public bool IsGroupChannel { get; set; }
    }

    public class ChatPreviewResponse {
        [JsonProperty("preview")]
        public ChatPreview Preview { get; set; }

        [JsonProperty("profiles")]
        public List<User> Profiles { get; set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }
    }
}
