using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    public class PhotoSaveResult {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("pid")]
        public int PhotoId { get; set; }

        [JsonProperty("album_id")]
        public int AlbumId { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("access_key")]
        public string AccessKey { get; set; }

        [JsonProperty("sizes")]
        public List<PhotoSizes> Sizes { get; set; }
    }
}
