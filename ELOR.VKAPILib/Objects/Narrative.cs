using Newtonsoft.Json;
using System.Collections.Generic;

namespace ELOR.VKAPILib.Objects {
    public class NarrativeCover {
        [JsonProperty("cropped_sizes")]
        public List<PhotoSizes> CroppedSizes { get; set; }
    }

    public class Narrative : AttachmentBase {
        [JsonIgnore]
        public override string ObjectType { get { return "narrative"; } }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("cover")]
        public NarrativeCover Cover { get; set; }
    }
}