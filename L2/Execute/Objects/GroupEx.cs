using ELOR.VKAPILib.Objects;
using Newtonsoft.Json;

namespace ELOR.Laney.Execute.Objects {
    public class GroupEx : Group {
        [JsonProperty("messages_count")]
        public int MessagesCount { get; private set; }

        [JsonProperty("notifications_disabled")]
        public bool NotificationsDisabled { get; private set; }

        [JsonProperty("messages_allowed")]
        public bool MessagesAllowed { get; private set; }
    }
}