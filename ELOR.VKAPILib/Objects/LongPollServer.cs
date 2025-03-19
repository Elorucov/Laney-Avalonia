using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class LongPollServerInfo {
        public LongPollServerInfo() { }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("server")]
        public string Server { get; set; }

        [JsonPropertyName("ts")]
        public int TS { get; set; }

        [JsonPropertyName("pts")]
        public int PTS { get; set; }
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