using ELOR.VKAPILib.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ELOR.Laney.Execute.Objects {
    public class MessagesHistoryEx {
        [JsonProperty("conversation")]
        public Conversation Conversation { get; set; }

        [JsonProperty("messages")]
        public List<Message> Messages { get; set; }

        [JsonProperty("last_message")]
        public Message LastMessage { get; set; }

        [JsonProperty("profiles")]
        public List<User> Profiles { get; set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }

        [JsonProperty("mentioned_profiles")]
        public List<User> MentionedProfiles { get; set; }

        [JsonProperty("mentioned_groups")]
        public List<Group> MentionedGroups { get; set; }

        [JsonProperty("online_info")]
        public UserOnlineInfo OnlineInfo { get; set; }
    }
}