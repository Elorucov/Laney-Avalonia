using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class Curator {
        public Curator() { }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { if (!String.IsNullOrEmpty(Url)) { return new Uri(Url); } else { return null; } } }

        [JsonPropertyName("photo")]
        public List<PhotoSizes> Photo { get; set; }

    }
}