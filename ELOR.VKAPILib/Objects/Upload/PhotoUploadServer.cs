using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Upload {
    public class PhotoUploadServer : VkUploadServer {
        public PhotoUploadServer() {}
        
        [JsonPropertyName("album_id")]
        public int AlbumId { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }
    }
}