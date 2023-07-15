using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public class DeleteConversationResponse {
        public DeleteConversationResponse() {}
        
        [JsonPropertyName("last_deleted_id")]
        public int LastDeletedId { get; set; }
    }
}