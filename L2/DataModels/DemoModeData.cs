using ELOR.VKAPILib.Objects;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ELOR.Laney.DataModels {
    public class DemoModeSession {
        [JsonPropertyName("id")]
        public long Id { get; private set; }

        [JsonPropertyName("conversations")]
        public List<ConversationItem> Conversations { get; private set; }

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; private set; }

        [JsonPropertyName("times")]
        public Dictionary<int, int> Times { get; private set; }

        [JsonPropertyName("activity_statuses")]
        public Dictionary<long, List<LongPollActivityInfo>> ActivityStatuses { get; private set; }
    }

    public class DemoModeData {
        [JsonPropertyName("sessions")]
        public List<DemoModeSession> Sessions { get; private set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; private set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; private set; }
    }
}