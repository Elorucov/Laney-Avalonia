using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public class LastActivity {
        public LastActivity() {}
        
        [JsonPropertyName("online")]
        public bool Online { get; set; }

        [JsonPropertyName("time")]
        public int TimeUnix { get; set; }

        [JsonIgnore]
        public DateTime Time { get { return DateTimeOffset.FromUnixTimeSeconds(TimeUnix).DateTime.ToLocalTime(); } }
    }
}
