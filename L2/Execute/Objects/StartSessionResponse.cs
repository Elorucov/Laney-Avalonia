using ELOR.VKAPILib.Objects;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ELOR.Laney.Execute.Objects {
    public class LongPollInfoForSession {
        public LongPollInfoForSession() { }

        [JsonPropertyName("session_id")]
        public long SessionId { get; set; }

        [JsonPropertyName("longpoll")]
        public LongPollServerInfo LongPoll { get; set; }
    }

    public class StartSessionResponse {
        public StartSessionResponse() { }

        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("reactions_assets")]
        public List<ReactionAssets> ReactionsAssets { get; set; }

        [JsonPropertyName("available_reactions")]
        public List<int> AvailableReactions { get; set; }

        [JsonPropertyName("longpolls")]
        public List<LongPollInfoForSession> LongPolls { get; set; }

        [JsonPropertyName("queue_config")]
        public QueueSubscribeResponse QueueConfig { get; set; }

        [JsonPropertyName("templates")]
        public List<MessageTemplatesList> Templates { get; set; }
    }
}