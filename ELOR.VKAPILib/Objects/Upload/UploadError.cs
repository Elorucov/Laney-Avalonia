using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Upload {
    public class UploadError {
        public UploadError() { }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }

        [JsonPropertyName("error_descr")]
        public string ErrorDescription { get; set; }
    }
}