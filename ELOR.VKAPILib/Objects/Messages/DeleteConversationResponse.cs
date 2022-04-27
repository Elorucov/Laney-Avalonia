using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects.Messages {
    public class DeleteConversationResponse {
        [JsonProperty("last_deleted_id")]
        public int LastDeletedId { get; set; }
    }
}