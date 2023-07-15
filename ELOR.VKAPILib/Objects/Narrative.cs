using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class NarrativeCover {
        public NarrativeCover() {}

        [JsonPropertyName("cropped_sizes")]
        public List<PhotoSizes> CroppedSizes { get; set; }
    }

    public class Narrative : AttachmentBase {
        public Narrative() {}

        [JsonIgnore]
        public override string ObjectType { get { return "narrative"; } }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("cover")]
        public NarrativeCover Cover { get; set; }
    }
}