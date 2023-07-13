using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ELOR.VKAPILib.Objects
{
    [DataContract]
    public enum DocumentType
    {
        Text = 1,
        Archive = 2,
        GIF = 3,
        Image = 4,
        Audio = 5,
        Video = 6,
        EBook = 7,
        Unknown = 8
    }

    public class DocumentPreview
    {
        [JsonProperty("graffiti")]
        public Graffiti Graffiti { get; set; }

        [JsonProperty("photo")]
        public Photo Photo { get; set; }
    }

    public class Document : AttachmentBase, IPreview {
        [JsonIgnore]
        public override string ObjectType { get { return "doc"; } }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("size")]
        public ulong Size { get; set; }

        [JsonProperty("ext")]
        public string Extension { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }

        [JsonProperty("date")]
        public long DateUnix { get; set; }

        [JsonIgnore]
        public DateTime DateTime { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("type")]
        public DocumentType Type { get; set; }

        [JsonProperty("preview")]
        public DocumentPreview Preview { get; set; }
    }
}