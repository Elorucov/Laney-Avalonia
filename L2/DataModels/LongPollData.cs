using System.Text.Json.Serialization;

namespace ELOR.Laney.DataModels {
    public enum LongPollState {
        Connecting, Updating, Working, Failed, NoInternet
    }

    public enum LongPollActivityType {
        Typing = 0, RecordingAudioMessage = 1, UploadingPhoto = 2, UploadingVideo = 3, UploadingFile = 4
    }

    public class LongPollActivityInfo {
        public LongPollActivityInfo() { }

        public LongPollActivityInfo(long id, LongPollActivityType status) {
            MemberId = id;
            Status = status;
        }

        [JsonPropertyName("member_id")]
        public long MemberId { get; set; }

        [JsonPropertyName("status")]
        public LongPollActivityType Status { get; set; }

        public override string ToString() {
            return $"{MemberId}={Status}";
        }
    }

    public class LongPollPushNotificationData {
        [JsonPropertyName("peer_id")]
        public long PeerId { get; set; }

        [JsonPropertyName("sound")]
        public int Sound { get; set; }

        [JsonPropertyName("disabled_until")]
        public int DisabledUntil { get; set; }
    }

    public class LongPollCallbackAction {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } // Type = "show_snackbar"

        [JsonPropertyName("link")]
        public string Link { get; set; } // Type = "open_link"

        [JsonPropertyName("app_id")]
        public int AppId { get; set; } // Type = "open_app"

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; } // Type = "open_app"

        [JsonPropertyName("hash")]
        public string Hash { get; set; } // Type = "open_app"
    }

    public class LongPollCallbackResponse {
        [JsonPropertyName("peer_id")]
        public long PeerId { get; set; }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; }

        [JsonPropertyName("event_id")]
        public string EventId { get; set; }

        [JsonPropertyName("action")]
        public LongPollCallbackAction Action { get; set; }
    }
}