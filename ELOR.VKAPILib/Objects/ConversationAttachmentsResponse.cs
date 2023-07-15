using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class ConversationAttachment {
        public ConversationAttachment() {}

        [JsonPropertyName("attachment")]
        public Attachment Attachment { get; set; }

        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("from_id")]
        public long FromId { get; set; }
    }

    public class ConversationAttachmentsResponse : VKList<ConversationAttachment> {
        public ConversationAttachmentsResponse() {}

        [JsonPropertyName("next_from")]
        public string NextFrom { get; set; }
    }
}