using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Upload {
    public class VkUploadServer {
        public VkUploadServer() { }

        [JsonPropertyName("upload_url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return !String.IsNullOrEmpty(Url) ? new Uri(Url) : null; } }
    }
}