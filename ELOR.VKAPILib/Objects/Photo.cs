using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects
{
    public class PhotoSizes
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return String.IsNullOrEmpty(Url) ? new Uri(Src) : new Uri(Url); } }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonIgnore]
        public Size Size { get { return new Size(Width, Height); } }

        public override string ToString() => $"{Type}:{Width}x{Height}";
    }

    public class Photo : AttachmentBase, IPreview {
        [JsonIgnore]
        public override string ObjectType { get { return "photo"; } }

        [JsonProperty("album_id")]
        public int AlbumId { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("sizes")]
        public List<PhotoSizes> Sizes { get; set; }

        [JsonIgnore]
        public PhotoSizes MaximalSizedPhoto { get { return GetMaximalSizedPhoto(); } }

        [JsonIgnore]
        public PhotoSizes MinimalSizedPhoto { get { return GetMinimalSizedPhoto(); } }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("photo_50")]
        public string Photo50Url { get; set; }

        [JsonIgnore]
        public Uri Photo50 { get { return new Uri(Photo50Url); } }

        [JsonProperty("photo_100")]
        public string Photo100Url { get; set; }

        [JsonIgnore]
        public Uri Photo100 { get { return new Uri(Photo100Url); } }

        [JsonProperty("photo_200")]
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
                        max = (long)(s.Width * s.Height);
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
