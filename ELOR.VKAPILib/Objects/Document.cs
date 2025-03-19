using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public enum DocumentType {
        Text = 1,
        Archive = 2,
        GIF = 3,
        Image = 4,
        Audio = 5,
        Video = 6,
        EBook = 7,
        Unknown = 8
    }

    public class DocumentPreview {
        public DocumentPreview() { }

        [JsonPropertyName("graffiti")]
        public Graffiti Graffiti { get; set; }

        [JsonPropertyName("photo")]
        public Photo Photo { get; set; }
    }

    public class Document : AttachmentBase, IPreview {
        public Document() { }

        [JsonIgnore]
        public override string ObjectType { get { return "doc"; } }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("size")]
        public ulong Size { get; set; }

        [JsonPropertyName("ext")]
        public string Extension { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }

        [JsonPropertyName("date")]
        public long DateUnix { get; set; }

        [JsonIgnore]
        public DateTime DateTime { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("type")]
        public DocumentType Type { get; set; }

        [JsonPropertyName("preview")]
        public DocumentPreview Preview { get; set; }
    }
}