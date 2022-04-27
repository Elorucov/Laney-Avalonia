using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects
{
    public class StickerImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class Sticker
    {
        [JsonProperty("animation_url")]
        public string AnimationUrl { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("sticker_id")]
        public int StickerId { get; set; }

        [JsonProperty("is_allowed")]
        public bool IsAllowed { get; set; }

        [JsonProperty("images_with_background")]
        public List<StickerImage> Images { get; set; }
    }

    public class StickerDictionary {
        [JsonProperty("words")]
        public List<string> Words { get; set; }

        [JsonProperty("stickers")]
        public List<Sticker> Stickers { get; set; }
    }
}
