using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects {
    public class CreateChatResponse {
        [JsonProperty("chat_id")]
        public int ChatId { get; set; }

        [JsonProperty("peer_ids")]
        public List<int> PeerIds { get; set; }
    }
}