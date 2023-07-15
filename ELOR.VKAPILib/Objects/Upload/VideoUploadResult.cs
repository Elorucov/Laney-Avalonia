using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Upload {
    public class VideoUploadResult {
        public VideoUploadResult() {}
        
        [JsonPropertyName("video_id")]
        public int VideoId { get; set; }

        [JsonPropertyName("owner_id")]
        public int OwnerId { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("video_hash")]
        public string VideoHash { get; set; }
    }
}