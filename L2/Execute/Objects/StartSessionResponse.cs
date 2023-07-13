using ELOR.VKAPILib.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ELOR.Laney.Execute.Objects {
    public class LongPollInfoForSession {
        [JsonProperty("session_id")]
        public long SessionId { get; set; }

        [JsonProperty("longpoll")]
        public LongPollServerInfo LongPoll { get; set; }
    }

    public class StartSessionResponse {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }

        [JsonProperty("longpolls")]
        public List<LongPollInfoForSession> LongPolls { get; set; }
    }
}