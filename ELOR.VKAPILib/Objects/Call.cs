using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects
{
    public class CallParticipants {
        public CallParticipants() {}
            
        [JsonPropertyName("list")]
        public List<long> List { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class Call {
        public Call() {}
            
        [JsonPropertyName("initiator_id")]
        public long InitiatorId { get; set; }

        [JsonPropertyName("receiver_id")]
        public long ReceiverId { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("duration")]
        public int DurationSeconds { get; set; }

        [JsonIgnore]
        public TimeSpan Duration { get { return TimeSpan.FromSeconds(DurationSeconds); } }

        [JsonPropertyName("video")]
        public bool Video { get; set; }

        [JsonPropertyName("participants")]
        public CallParticipants Participants { get; set; }
    }

    public class GroupCallInProgress {
        [JsonPropertyName("initiator_id")]
        public long InitiatorId { get; set; }

        [JsonPropertyName("participants")]
        public CallParticipants Participants { get; set; }

        [JsonPropertyName("join_link")]
        public string JoinLink { get; set; }
    }
}