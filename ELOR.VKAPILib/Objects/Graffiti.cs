using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects
{
    public class Graffiti : AttachmentBase {
        [JsonIgnore]
        public override string ObjectType { get { return "doc"; } }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return !String.IsNullOrEmpty(Src) ? new Uri(Src) : new Uri(Url); } } // VK API devs is suckers.
    }
}
