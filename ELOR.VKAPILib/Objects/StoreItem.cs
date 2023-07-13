using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects {
    public class StoreProduct {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("stickers")]
        public List<Sticker> Stickers { get; set; }

        [JsonProperty("previews")]
        public List<StickerImage> Previews { get; set; }
    }
}