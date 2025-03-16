using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class PodcastCover {
        public PodcastCover() { }

        [JsonPropertyName("sizes")]
        public List<PhotoSizes> Sizes { get; set; }
    }

    public class PodcastInfo {
        public PodcastInfo() { }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("plays")]
        public int Plays { get; set; }

        [JsonPropertyName("cover")]
        public PodcastCover Cover { get; set; } // Какой криворукий сотрудник ВК додумался впихнуть обложки как отдельный вложенный объект, бл*ть?
    }

    public class Podcast : Audio {
        public Podcast() { }

        [JsonIgnore]
        public override string ObjectType { get { return "podcast"; } }

        [JsonPropertyName("podcast_info")]
        public PodcastInfo Info { get; set; }
    }
}