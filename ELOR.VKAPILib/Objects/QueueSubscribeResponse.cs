using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class Queue {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }
    }

    public class QueueSubscribeResponse {
        [JsonPropertyName("base_url")]
        public string BaseUrl { get; set; }

        [JsonPropertyName("queues")]
        public List<Queue> Queues { get; set; }
    }
}