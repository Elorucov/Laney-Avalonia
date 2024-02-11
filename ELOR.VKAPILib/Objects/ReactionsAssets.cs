using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class ReactionAssetLinks {
        [JsonPropertyName("big_animation")]
        public string BigAnimation { get; set; }

        [JsonPropertyName("small_animation")]
        public string SmallAnimation { get; set; }

        [JsonPropertyName("static")]
        public string Static { get; set; }
    }

    public class ReactionAssets {
        [JsonPropertyName("reaction_id")]
        public int ReactionId { get; set; }

        [JsonPropertyName("links")]
        public ReactionAssetLinks Links { get; set; }
    }
}