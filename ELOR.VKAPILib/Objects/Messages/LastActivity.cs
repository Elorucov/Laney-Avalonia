using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELOR.VKAPILib.Objects.Messages {
    public class LastActivity {
        [JsonProperty("online")]
        public bool Online { get; internal set; }

        [JsonProperty("time")]
        public int TimeUnix { get; internal set; }

        [JsonIgnore]
        public DateTime Time { get { return DateTimeOffset.FromUnixTimeSeconds(TimeUnix).DateTime.ToLocalTime(); } }
    }
}
