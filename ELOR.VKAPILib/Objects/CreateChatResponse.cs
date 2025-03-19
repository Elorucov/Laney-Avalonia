using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class CreateChatResponse {
        public CreateChatResponse() { }

        [JsonPropertyName("chat_id")]
        public long ChatId { get; set; }

        [JsonPropertyName("peer_ids")]
        public List<long> PeerIds { get; set; }
    }
}