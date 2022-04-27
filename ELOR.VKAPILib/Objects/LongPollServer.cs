using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects
{
    public class LongPollServerInfo
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("ts")]
        public string TS { get; set; }

        [JsonProperty("pts")]
        public string PTS { get; set; }
    }

    public class LongPollFail
    {
        [JsonProperty("failed")]
        public int FailCode { get; set; }

        [JsonProperty("ts")]
        public string TS { get; set; }
    }

    public class LongPollResponse
    {
        [JsonProperty("ts")]
        public string TS { get; set; }

        [JsonProperty("updates")]
        public List<object[]> Updates { get; set; }

        [JsonIgnore]
        public string Raw { get; set; }
    }
}
