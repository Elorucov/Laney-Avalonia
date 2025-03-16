using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public class LongPollHistoryResponse {
        public LongPollHistoryResponse() { }

        [JsonPropertyName("messages")]
        public MessagesList Messages { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("new_pts")]
        public int NewPTS { get; set; }

        [JsonPropertyName("more")]
        public bool More { get; set; }

        [JsonPropertyName("credentials")]
        public LongPollServerInfo Credentials { get; set; }

        [JsonPropertyName("history")]
        public JsonArray History { get; set; }
    }
}