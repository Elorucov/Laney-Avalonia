using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects {
    public class StickerImage {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class StickerRender {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("images")]
        public List<StickerImage> Images { get; set; }

        [JsonProperty("is_stub")]
        public bool IsStub { get; set; }

        [JsonProperty("is_rendering")]
        public bool IsRendering { get; set; }
    }

    public class StickerVmoji {
        [JsonProperty("character_id")]
        public string CharacterId { get; set; }
    }

    public class Sticker {
        [JsonProperty("animation_url")]
        public string AnimationUrl { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("sticker_id")]
        public int StickerId { get; set; }

        [JsonProperty("is_allowed")]
        public bool IsAllowed { get; set; }

        [JsonProperty("images")]
        public List<StickerImage> Images { get; set; }

        [JsonProperty("images_with_background")]
        public List<StickerImage> ImagesWithBackground { get; set; }

        [JsonProperty("render")]
        public StickerRender Render { get; set; }

        [JsonProperty("vmoji")]
        public StickerVmoji Vmoji { get; set; }

        [JsonIgnore]
        public bool IsPartial { get { return Images == null && ImagesWithBackground == null; } }
    }

    public class StickerDictionary {
        [JsonProperty("words")]
        public List<string> Words { get; set; }

        [JsonProperty("stickers")]
        public List<Sticker> Stickers { get; set; }
    }
}