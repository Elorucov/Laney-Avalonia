using ELOR.VKAPILib.Objects;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ELOR.Laney.Execute.Objects {
    public class MessagesHistoryEx {
        public MessagesHistoryEx() {}

        [JsonPropertyName("conversation")]
        public Conversation Conversation { get; set; }

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }

        [JsonPropertyName("last_message")]
        public Message LastMessage { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("mentioned_profiles")]
        public List<User> MentionedProfiles { get; set; }

        [JsonPropertyName("mentioned_groups")]
        public List<Group> MentionedGroups { get; set; }

        [JsonPropertyName("online_info")]
        public UserOnlineInfoEx OnlineInfo { get; set; }
    }
}