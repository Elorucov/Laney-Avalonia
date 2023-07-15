using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class DocumentSaveResult {
        public DocumentSaveResult() {}

        [JsonPropertyName("type")]
        public AttachmentType Type { get; set; }

        [JsonPropertyName("graffiti")]
        public Graffiti Graffiti { get; set; }

        [JsonPropertyName("audio_message")]
        public AudioMessage AudioMessage { get; set; }

        [JsonPropertyName("doc")]
        public Document Document { get; set; }
    }
}