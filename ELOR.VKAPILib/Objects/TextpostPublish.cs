using System.Text.Json.Serialization;
using System;

namespace ELOR.VKAPILib.Objects {
    public class TextpostPublish {
        public TextpostPublish() {}

        [JsonPropertyName("attach_url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { if (!String.IsNullOrEmpty(Url)) { return new Uri(Url); } else { return null; } } }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}