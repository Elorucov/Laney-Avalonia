using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects.Upload {
    public class PhotoUploadServer : VkUploadServer {
        [JsonProperty("album_id")]
        public int AlbumId { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }
    }
}
