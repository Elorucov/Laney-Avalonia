using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects
{
    public class CallParticipants {
        [JsonProperty("list")]
        public List<int> List { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class Call {
        [JsonProperty("initiator_id")]
        public int InitiatorId { get; set; }

        [JsonProperty("receiver_id")]
        public int ReceiverId { get; set; }

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
        public int InitiatorId { get; set; }

        [JsonProperty("participants")]
        public CallParticipants Participants { get; set; }

        [JsonProperty("join_link")]
        public string JoinLink { get; set; }
    }
}
