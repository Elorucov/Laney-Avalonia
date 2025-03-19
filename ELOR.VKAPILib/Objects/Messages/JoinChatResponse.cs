using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {

    public class JoinChatResponse {
        public JoinChatResponse() { }

        [JsonPropertyName("chat_id")]
        public long ChatId { get; set; }
    }
}