using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects
{
    public class CallParticipants {
        [JsonProperty("list")]
        public List<long> List { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class Call {
        [JsonProperty("initiator_id")]
        public long InitiatorId { get; set; }

        [JsonProperty("receiver_id")]
        public long ReceiverId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("duration")]
        public int DurationSeconds { get; set; }

        [JsonIgnore]
        public TimeSpan Duration { get { return TimeSpan.FromSeconds(DurationSeconds); } }

        [JsonProperty("video")]
        public bool Video { get; set; }

        [JsonProperty("participants")]
        public CallParticipants Participants { get; set; }
    }

    public class GroupCallInProgress {
        [JsonProperty("initiator_id")]
        public long InitiatorId { get; set; }

        [JsonProperty("participants")]
        public CallParticipants Participants { get; set; }

        [JsonProperty("join_link")]
        public string JoinLink { get; set; }
    }
}