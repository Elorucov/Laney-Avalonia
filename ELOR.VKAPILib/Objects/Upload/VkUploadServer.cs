using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects.Upload {
    public class VkUploadServer {
        [JsonProperty("upload_url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return !String.IsNullOrEmpty(Url) ? new Uri(Url) : null; } }
    }
}
