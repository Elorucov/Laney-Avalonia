using ELOR.VKAPILib.Objects;
using System.Text.Json.Serialization;

namespace ELOR.Laney.Execute.Objects {
    public class GroupEx : Group {
        public GroupEx() {}

        [JsonPropertyName("messages_count")]
        public int MessagesCount { get; set; }

        [JsonPropertyName("notifications_disabled")]
        public bool NotificationsDisabled { get; set; }

        [JsonPropertyName("messages_allowed")]
        public bool MessagesAllowed { get; set; }
    }
}