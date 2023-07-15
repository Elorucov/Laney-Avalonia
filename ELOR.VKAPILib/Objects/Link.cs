using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class LinkButtonAction {
        public LinkButtonAction() { }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { if (!String.IsNullOrEmpty(Url)) { return new Uri(Url); } else { return null; } } }
    }

    public class LinkButton {
        public LinkButton() { }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("action")]
        public LinkButtonAction Action { get; set; }
    }

    public class Link {
        public Link() { }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { if (!String.IsNullOrEmpty(Url)) { return new Uri(Url); } else { return null; } } }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("photo")]
        public Photo Photo { get; set; }

        [JsonPropertyName("button")]
        public LinkButton Button { get; set; }

        [JsonPropertyName("preview_page")]
        public string PreviewPage { get; set; }

        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; }

        [JsonPropertyName("image_src")]
        public string ImageSrc { get; set; }

        [JsonIgnore]
        public Uri PreviewUri {
            get {
                if (!String.IsNullOrEmpty(PreviewUrl)) { return new Uri(PreviewUrl); } else {
                    if (!String.IsNullOrEmpty(ImageSrc)) { return new Uri(ImageSrc); } else { return null; }
                }
            }
        }
    }
}