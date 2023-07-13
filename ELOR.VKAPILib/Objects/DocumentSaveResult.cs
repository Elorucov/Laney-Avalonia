using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects {
    public class DocumentSaveResult {
        [JsonProperty("type")]
        public AttachmentType Type { get; set; }

        [JsonProperty("graffiti")]
        public Graffiti Graffiti { get; set; }

        [JsonProperty("audio_message")]
        public AudioMessage AudioMessage { get; set; }

        [JsonProperty("doc")]
        public Document Document { get; set; }
    }
}