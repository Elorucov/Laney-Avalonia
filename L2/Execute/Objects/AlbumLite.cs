using Newtonsoft.Json;
using System;

namespace ELOR.Laney.Execute.Objects {
    public class AlbumLite {
        [JsonProperty("id")]
        public int Id { get; private set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("size")]
        public int Size { get; private set; }

        [JsonProperty("thumb")]
        public string Thumb { get; private set; }

        [JsonIgnore]
        public Uri ThumbUri => Uri.IsWellFormedUriString(Thumb, UriKind.Absolute) ? new Uri(Thumb) : null;
    }
}