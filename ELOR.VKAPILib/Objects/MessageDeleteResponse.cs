using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects {
    public class MessageDeleteResponse {
        [JsonProperty("peer_id")]
        public long PeerId { get; set; }

        [JsonProperty("conversation_message_id")]
        public int ConversationMessageId { get; set; }

        [JsonProperty("response")]
        public int Response { get; set; }
    }
}