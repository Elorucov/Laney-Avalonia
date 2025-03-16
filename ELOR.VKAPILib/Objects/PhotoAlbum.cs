using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class PhotoAlbum : Album {
        public PhotoAlbum() { }

        [JsonPropertyName("thumb_id")]
        public int ThumbId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("created")]
        public long CreatedUnix { get; set; }

        [JsonIgnore]
        public DateTime Created { get { return DateTimeOffset.FromUnixTimeSeconds(CreatedUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("updated")]
        public long UpdatedUnix { get; set; }

        [JsonIgnore]
        public DateTime Updated { get { return DateTimeOffset.FromUnixTimeSeconds(UpdatedUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("thumb_src")]
        public string ThumbSrc { get; set; }

        [JsonIgnore]
        public Uri Thumb { get { return new Uri(ThumbSrc); } }

        [JsonPropertyName("sizes")]
        public List<PhotoSizes> Sizes { get; set; }
    }
}