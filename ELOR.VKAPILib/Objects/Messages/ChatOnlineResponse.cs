using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects.Messages {
    public class ChatOnlineResponse {
        public ChatOnlineResponse() { }

        [JsonPropertyName("online_count")]
        public int OnlineCount { get; set; }
    }
}
