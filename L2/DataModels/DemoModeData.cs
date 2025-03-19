using ELOR.VKAPILib.Objects;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ELOR.Laney.DataModels {
    public class DemoModeSession {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("conversations")]
        public List<ConversationItem> Conversations { get; set; }

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }

        [JsonPropertyName("times")]
        public Dictionary<string, int> Times { get; set; }

        [JsonPropertyName("activity_statuses")]
        public Dictionary<string, List<LongPollActivityInfo>> ActivityStatuses { get; set; }
    }

    public class DemoModeData {
        [JsonPropertyName("sessions")]
        public List<DemoModeSession> Sessions { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }
}