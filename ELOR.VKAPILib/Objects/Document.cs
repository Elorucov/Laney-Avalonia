using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
        public long Size { get; set; }

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

        //[JsonIgnore]
        //public Uri PreviewImageUri { get { return Preview.Photo.PreviewImageUri; } }

        //[JsonIgnore]
        //public Size PreviewImageSize { get { return Preview.Photo.PreviewImageSize; } }
    }
}