using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELOR.VKAPILib.Objects.Messages {
    public class SetChatPhotoResponse {
        [JsonProperty("message_id")]
        public int MessageId { get; internal set; }

        [JsonProperty("chat")]
        public Chat Chat { get; internal set; }
    }
}
