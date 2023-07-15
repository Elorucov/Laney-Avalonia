using System.Text.Json.Serialization;

namespace ELOR.Laney.DataModels {
    public enum LongPollState {
        Connecting, Updating, Working, Failed
    }

    public enum LongPollActivityType {
        Typing, RecordingAudioMessage, UploadingPhoto, UploadingVideo, UploadingFile
    }

    public class LongPollActivityInfo {
        public LongPollActivityInfo() { }

        public LongPollActivityInfo(long id, LongPollActivityType status) {
            MemberId = id;
            Status = status;
        }

        [JsonPropertyName("member_id")]
        public long MemberId { get; private set; }

        [JsonPropertyName("status")]
        public LongPollActivityType Status { get; private set; }

        public override string ToString() {
            return $"{MemberId}={Status}";
        }
    }

    public class LongPollPushNotificationData {
        [JsonPropertyName("peer_id")]
        public long PeerId { get; private set; }

        [JsonPropertyName("sound")]
        public int Sound { get; private set; }

        [JsonPropertyName("disabled_until")]
        public int DisabledUntil { get; private set; }
    }

    public class LongPollCallbackAction {
        [JsonPropertyName("type")]
        public string Type { get; private set; }

        [JsonPropertyName("text")]
        public string Text { get; private set; } // Type = "show_snackbar"

        [JsonPropertyName("link")]
        public string Link { get; private set; } // Type = "open_link"

        [JsonPropertyName("app_id")]
        public int AppId { get; private set; } // Type = "open_app"

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; private set; } // Type = "open_app"

        [JsonPropertyName("hash")]
        public string Hash { get; private set; } // Type = "open_app"
    }

    public class LongPollCallbackResponse {
        [JsonPropertyName("peer_id")]
        public long PeerId { get; private set; }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; private set; }

        [JsonPropertyName("event_id")]
        public string EventId { get; private set; }

        [JsonPropertyName("action")]
        public LongPollCallbackAction Action { get; private set; }
    }
}