using System.Text.Json.Serialization;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ELOR.VKAPILib.Objects {
    public class PhotoSizes {
        public PhotoSizes() {}

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("src")]
        public string Src { get; set; }

        [JsonIgnore]
        public Uri Uri {
            get {
                if (!String.IsNullOrEmpty(Url)) return new Uri(Url);
                if (!String.IsNullOrEmpty(Src)) return new Uri(Src);
                return null;
            }
        }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("without_padding")]
        public bool WithoutPadding { get; set; }

        [JsonPropertyName("with_padding")]
        public int WithPadding { get; set; }

        public override string ToString() => $"{Type}:{Width}x{Height}";
    }

    public class Photo : AttachmentBase, IPreview {
        public Photo() {}

        [JsonIgnore]
        public override string ObjectType { get { return "photo"; } }

        [JsonPropertyName("album_id")]
        public int AlbumId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("sizes")]
        public List<PhotoSizes> Sizes { get; set; }

        [JsonIgnore]
        public PhotoSizes MaximalSizedPhoto { get { return GetMaximalSizedPhoto(); } }

        [JsonIgnore]
        public PhotoSizes MinimalSizedPhoto { get { return GetMinimalSizedPhoto(); } }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("photo_50")]
        public string Photo50Url { get; set; }

        [JsonIgnore]
        public Uri Photo50 { get { return new Uri(Photo50Url); } }

        [JsonPropertyName("photo_100")]
        public string Photo100Url { get; set; }

        [JsonIgnore]
        public Uri Photo100 { get { return new Uri(Photo100Url); } }

        [JsonPropertyName("photo_200")]
        public string Photo200Url { get; set; }

        [JsonIgnore]
        public Uri Photo200 { get { return new Uri(Photo200Url); } }

        //

        private PhotoSizes GetMaximalSizedPhoto() {
            PhotoSizes p = null;
            long max = 0;
            foreach (PhotoSizes s in Sizes) {
                if (s.Width == 0 && s.Height == 0) {
                    p = Sizes.Last();
                } else {
                    if (s.Width * s.Height > max) {
                        max = s.Width * s.Height;
                        p = s;
                    }
                }
            }
            return p;
        }

        private PhotoSizes GetMinimalSizedPhoto() {
            PhotoSizes ps = null;
            foreach (PhotoSizes s in Sizes) {
                switch (s.Type) {
                    case "m": ps = s; break;
                    case "s": ps = s; break;
                }
            }
            return ps;
        }

        private PhotoSizes GetSizedPhotoForThumbnail() {
            PhotoSizes ps = null;
            foreach (PhotoSizes s in CollectionsMarshal.AsSpan(Sizes)) {
                if (ps != null && s.Width > 360) break;
                ps = s;
            }
            return ps;
        }
    }
}