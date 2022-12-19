using ELOR.VKAPILib.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ELOR.Laney.DataModels {
    public class DemoModeSession {
        [JsonProperty("id")]
        public int Id { get; private set; }

        [JsonProperty("conversations")]
        public List<ConversationItem> Conversations { get; private set; }

        [JsonProperty("messages")]
        public List<Message> Messages { get; private set; }

        [JsonProperty("times")]
        public Dictionary<int, int> Times { get; private set; }
    }

    public class DemoModeData {
        [JsonProperty("sessions")]
        public List<DemoModeSession> Sessions { get; private set; }

        [JsonProperty("profiles")]
        public List<User> Profiles { get; private set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; private set; }
    }
}