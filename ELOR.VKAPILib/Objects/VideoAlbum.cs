using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class VideoAlbum : Album {
        public VideoAlbum() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("photo_320")]
        public string Photo320 { get; set; }

        [JsonPropertyName("photo_160")]
        public string Photo160 { get; set; }

        [JsonIgnore]
        public Uri Photo {
            get {
                Uri p32 = !String.IsNullOrEmpty(Photo320) && Uri.IsWellFormedUriString(Photo320, UriKind.Absolute) ? new Uri(Photo320) : null;
                Uri p16 = !String.IsNullOrEmpty(Photo160) && Uri.IsWellFormedUriString(Photo160, UriKind.Absolute) ? new Uri(Photo160) : null;
                return String.IsNullOrEmpty(Photo320) ? p16 : p32;
            }
        }
    }
}