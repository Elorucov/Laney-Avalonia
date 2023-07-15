using System.Text.Json.Serialization;
using System;

namespace ELOR.Laney.Execute.Objects {
    public class AlbumLite {
        public AlbumLite() {}

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }

        [JsonIgnore]
        public Uri ThumbUri => Uri.IsWellFormedUriString(Thumb, UriKind.Absolute) ? new Uri(Thumb) : null;
    }
}