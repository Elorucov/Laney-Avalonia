using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class VKList<T> {
        public VKList() {}

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<T> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("conversations")]
        public List<Conversation> Conversations { get; set; }
    }
}