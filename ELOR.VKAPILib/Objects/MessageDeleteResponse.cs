using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    public class MessageDeleteResponse {
        [JsonProperty("peer_id")]
        public int PeerId { get; set; }

        [JsonProperty("conversation_message_id")]
        public int ConversationMessageId { get; set; }

        [JsonProperty("response")]
        public int Response { get; set; }
    }
}
