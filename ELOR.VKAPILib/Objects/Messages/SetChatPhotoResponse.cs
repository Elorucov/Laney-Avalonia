using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public class SetChatPhotoResponse {
        public SetChatPhotoResponse() { }

        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("chat")]
        public Chat Chat { get; set; }
    }
}