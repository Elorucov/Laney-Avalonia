using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELOR.VKAPILib.Objects.Messages {
    public class JoinChatResponse {
        [JsonProperty("chat_id")]
        public int ChatId { get; set; }
    }
}
