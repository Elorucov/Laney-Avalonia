using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects
{
    public class Gift
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("thumb_256")]
        public string Thumb { get; set; }

        [JsonIgnore]
        public Uri ThumbUri { get { return new Uri(Thumb); } }
    }
}
