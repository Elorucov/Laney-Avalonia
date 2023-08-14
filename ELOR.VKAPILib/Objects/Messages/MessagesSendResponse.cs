using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public class MessageSendResponse {
        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("cmid")]
        public int Cmid { get; set; }

        [JsonPropertyName("peer_id")] // required for markAsImportant
        public int PeerId { get; set; }
    }

    public class MarkAsImportantResponse {
        [JsonPropertyName("marked")]
        public List<MessageSendResponse> Marked { get; set; }
    }
}