using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class StoreProduct {
        public StoreProduct() {}

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("stickers")]
        public List<Sticker> Stickers { get; set; }

        [JsonPropertyName("previews")]
        public List<StickerImage> Previews { get; set; }
    }
}