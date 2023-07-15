using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class Gift {
        public Gift() {}

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("thumb_256")]
        public string Thumb { get; set; }

        [JsonIgnore]
        public Uri ThumbUri { get { return new Uri(Thumb); } }
    }
}