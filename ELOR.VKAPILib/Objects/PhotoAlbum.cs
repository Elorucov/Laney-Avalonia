using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    public class PhotoAlbum : Album {
        [JsonProperty("thumb_id")]
        public int ThumbId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("created")]
        public long CreatedUnix { get; set; }

        [JsonIgnore]
        public DateTime Created { get { return DateTimeOffset.FromUnixTimeSeconds(CreatedUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("updated")]
        public long UpdatedUnix { get; set; }

        [JsonIgnore]
        public DateTime Updated { get { return DateTimeOffset.FromUnixTimeSeconds(UpdatedUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("thumb_src")]
        public string ThumbSrc { get; set; }

        [JsonIgnore]
        public Uri Thumb { get { return new Uri(ThumbSrc); } }

        [JsonProperty("sizes")]
        public List<PhotoSizes> Sizes { get; set; }
    }
}
