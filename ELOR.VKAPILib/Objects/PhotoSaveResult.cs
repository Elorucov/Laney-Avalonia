using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class PhotoSaveResult {
        public PhotoSaveResult() {}

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("pid")]
        public int PhotoId { get; set; }

        [JsonPropertyName("album_id")]
        public int AlbumId { get; set; }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; }

        [JsonPropertyName("access_key")]
        public string AccessKey { get; set; }

        [JsonPropertyName("sizes")]
        public List<PhotoSizes> Sizes { get; set; }
    }
}