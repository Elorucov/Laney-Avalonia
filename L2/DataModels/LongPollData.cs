using Newtonsoft.Json;

namespace ELOR.Laney.DataModels {
    public enum LongPollState {
        Connecting, Updating, Working, Failed
    }

    public enum LongPollActivityType {
        Typing, RecordingAudioMessage, UploadingPhoto, UploadingVideo, UploadingFile
    }

    public class LongPollActivityInfo {
        public LongPollActivityInfo() { }

        public LongPollActivityInfo(int id, LongPollActivityType status) {
            MemberId = id;
            Status = status;
        }

        [JsonProperty("member_id")]
        public int MemberId { get; private set; }

        [JsonProperty("status")]
        public LongPollActivityType Status { get; private set; }

        public override string ToString() {
            return $"{MemberId}={Status}";
        }
    }

    public class LongPollPushNotificationData {
        [JsonProperty("peer_id")]
        public int PeerId { get; private set; }

        [JsonProperty("sound")]
        public int Sound { get; private set; }

        [JsonProperty("disabled_until")]
        public int DisabledUntil { get; private set; }
    }

    public class LongPollCallbackAction {
        [JsonProperty("type")]
        public string Type { get; private set; }

        [JsonProperty("text")]
        public string Text { get; private set; } // Type = "show_snackbar"

        [JsonProperty("link")]
        public string Link { get; private set; } // Type = "open_link"

        [JsonProperty("app_id")]
        public int AppId { get; private set; } // Type = "open_app"

        [JsonProperty("owner_id")]
        public int OwnerId { get; private set; } // Type = "open_app"

        [JsonProperty("hash")]
        public string Hash { get; private set; } // Type = "open_app"
    }

    public class LongPollCallbackResponse {
        [JsonProperty("peer_id")]
        public int PeerId { get; private set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; private set; }

        [JsonProperty("event_id")]
        public string EventId { get; private set; }

        [JsonProperty("action")]
        public LongPollCallbackAction Action { get; private set; }
    }
}