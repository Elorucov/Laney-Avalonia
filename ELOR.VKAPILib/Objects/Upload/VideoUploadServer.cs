using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Upload {
    public class VideoUploadServer : VkUploadServer {
        public VideoUploadServer() { }

        [JsonPropertyName("video_id")]
        public int VideoId { get; set; }

        [JsonPropertyName("owner_id")]
        public int OwnerId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("access_key")]
        public string AccessKey { get; set; }
    }
}