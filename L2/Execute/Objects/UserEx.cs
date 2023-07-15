using ELOR.VKAPILib.Objects;
using System.Text.Json.Serialization;

namespace ELOR.Laney.Execute.Objects {
    public class UserOnlineInfoEx : UserOnlineInfo {
        public UserOnlineInfoEx() {}

        [JsonPropertyName("app_name")]
        public string AppName { get; set; }
    }

    public class UserEx : User {
        public UserEx() {}

        [JsonPropertyName("messages_count")]
        public int MessagesCount { get; set; }

        [JsonPropertyName("notifications_disabled")]
        public bool NotificationsDisabled { get; set; }

        [JsonPropertyName("live_in")]
        public string LiveIn { get; set; }

        [JsonPropertyName("current_career")]
        public UserCareer CurrentCareer { get; set; }

        [JsonPropertyName("current_education")]
        public string CurrentEducation { get; set; }

        [JsonPropertyName("online_info")]
        public new UserOnlineInfoEx OnlineInfo { get; set; }
    }
}
