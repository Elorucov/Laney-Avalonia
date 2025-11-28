using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public class ChatOnlineResponse {
        public ChatOnlineResponse() { }

        [JsonPropertyName("online_count")]
        public int OnlineCount { get; set; }
    }
}
