using ELOR.VKAPILib.Objects;
using Newtonsoft.Json;

namespace ELOR.Laney.Execute.Objects {
    public class UserOnlineInfoEx : UserOnlineInfo {
        [JsonProperty("app_name")]
        public string AppName { get; set; }
    }

    public class UserEx : User {
        [JsonProperty("messages_count")]
        public int MessagesCount { get; private set; }

        [JsonProperty("notifications_disabled")]
        public bool NotificationsDisabled { get; private set; }

        [JsonProperty("live_in")]
        public string LiveIn { get; private set; }

        [JsonProperty("current_career")]
        public UserCareer CurrentCareer { get; set; }

        [JsonProperty("current_education")]
        public string CurrentEducation { get; set; }

        [JsonProperty("online_info")]
        public new UserOnlineInfoEx OnlineInfo { get; set; }
    }
}
