using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class Graffiti : AttachmentBase {
        public Graffiti() {}

        [JsonIgnore]
        public override string ObjectType { get { return "doc"; } }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("src")]
        public string Src { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return !String.IsNullOrEmpty(Src) ? new Uri(Src) : new Uri(Url); } } // VK API devs is suckers.
    }
}