using System;
using System.Text.Json.Serialization;

namespace ELOR.Laney.DataModels.VKQueue {
    public class OnlineEvent {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("online")]
        public bool Online { get; set; }

        [JsonPropertyName("platform")]
        public int Platform { get; set; }

        [JsonPropertyName("app_id")]
        public int AppId { get; set; }

        [JsonPropertyName("last_seen")]
        public int LastSeenUnix { get; set; }

        [JsonIgnore]
        public DateTime LastSeen { get { return DateTimeOffset.FromUnixTimeSeconds(LastSeenUnix).LocalDateTime; } }
    }
}