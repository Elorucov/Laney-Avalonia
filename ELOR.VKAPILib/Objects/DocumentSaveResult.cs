using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class DocumentSaveResult {
        public DocumentSaveResult() { }

        [JsonPropertyName("type")]
        public string TypeString { get; set; }

        [JsonIgnore]
        public AttachmentType Type { get { return Attachment.GetAttachmentEnum(TypeString); } }

        [JsonPropertyName("graffiti")]
        public Graffiti Graffiti { get; set; }

        [JsonPropertyName("audio_message")]
        public AudioMessage AudioMessage { get; set; }

        [JsonPropertyName("doc")]
        public Document Document { get; set; }
    }
}