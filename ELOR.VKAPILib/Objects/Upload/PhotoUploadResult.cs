using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Upload {
    public class PhotoUploadResult {
        public PhotoUploadResult() { }

        [JsonPropertyName("server")]
        public int Server { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }
    }
}