using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    public class ConversationAttachment {
        [JsonProperty("attachment")]
        public Attachment Attachment { get; set; }

        [JsonProperty("message_id")]
        public int MessageId { get; set; }

        [JsonProperty("from_id")]
        public int FromId { get; set; }
    }

    public class ConversationAttachmentsResponse : VKList<ConversationAttachment> {
        [JsonProperty("next_from")]
        public string NextFrom { get; set; }
    }
}
