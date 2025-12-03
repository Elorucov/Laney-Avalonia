using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class LongPollServerInfo {
        public LongPollServerInfo() { }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("lp_server_unavailable")]
        public int LongPollServerUnavailable { get; set; }

        [JsonPropertyName("server")]
        public string Server { get; set; }

        [JsonPropertyName("server_lp")] // only on messages.getDiff method
        public string ServerLongPoll { get; set; }

        [JsonPropertyName("server_sse")] // only on messages.getDiff method
        public string ServerSSE { get; set; }

        [JsonPropertyName("ts")]
        public long TS { get; set; }

        [JsonPropertyName("pts")] // only on messages.getLongPollServer/getLongPollHistory method
        public long PTS { get; set; }
    }

    public class LongPollFail {
        public LongPollFail() { }

        [JsonPropertyName("failed")]
        public int FailCode { get; set; }

        [JsonPropertyName("ts")]
        public string TS { get; set; }
    }

    public class LongPollResponse {
        public LongPollResponse() { }

        [JsonPropertyName("ts")]
        public string TS { get; set; }

        [JsonPropertyName("updates")]
        public List<object[]> Updates { get; set; }
    }
}