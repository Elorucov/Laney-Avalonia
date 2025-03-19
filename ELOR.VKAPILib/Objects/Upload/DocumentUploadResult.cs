using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Upload {
    public class DocumentUploadResult {
        public DocumentUploadResult() { }

        [JsonPropertyName("file")]
        public string File { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("error_descr")]
        public string ErrorDescription { get; set; }
    }
}