using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    public class DocumentSaveResult {
        [JsonProperty("type")]
        public AttachmentType Type { get; set; }

        [JsonProperty("graffiti")]
        public Graffiti Graffiti { get; set; }

        [JsonProperty("audio_message")]
        public AudioMessage AudioMessage { get; set; }

        [JsonProperty("doc")]
        public Document Document { get; set; }
    }
}
