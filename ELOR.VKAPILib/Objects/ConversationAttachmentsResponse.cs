using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class ConversationAttachment {
        public ConversationAttachment() { }

        [JsonPropertyName("attachment")]
        public Attachment Attachment { get; set; }

        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("cmid")]
        public int CMID { get; set; }

        [JsonPropertyName("from_id")]
        public long FromId { get; set; }
    }

    public class ConversationAttachmentsResponse : IVKList<ConversationAttachment> {
        public ConversationAttachmentsResponse() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<ConversationAttachment> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("cmid_next_from")]
        public string NextFrom { get; set; }
    }
}