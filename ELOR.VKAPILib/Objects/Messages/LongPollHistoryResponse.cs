using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELOR.VKAPILib.Objects.Messages {
    public class LongPollHistoryResponse {
        [JsonProperty("messages")]
        public VKList<Message> Messages { get; internal set; }

        [JsonProperty("profiles")]
        public List<User> Profiles { get; internal set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; internal set; }

        [JsonProperty("new_pts")]
        public int NewPTS { get; internal set; }

        [JsonProperty("more")]
        public bool More { get; internal set; }

        [JsonProperty("credentials")]
        public LongPollServerInfo Credentials { get; internal set; }
    }
}
