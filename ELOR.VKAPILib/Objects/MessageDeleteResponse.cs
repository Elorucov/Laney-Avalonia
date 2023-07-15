using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class MessageDeleteResponse {
        public MessageDeleteResponse() {}

        [JsonPropertyName("peer_id")]
        public long PeerId { get; set; }

        [JsonPropertyName("conversation_message_id")]
        public int ConversationMessageId { get; set; }

        [JsonPropertyName("response")]
        public int Response { get; set; }
    }
}