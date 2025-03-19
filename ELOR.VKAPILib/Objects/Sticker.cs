using ELOR.VKAPILib.Attributes;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class StickerImage {
        public StickerImage() { }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class StickerRender {
        public StickerRender() { }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("images")]
        public List<StickerImage> Images { get; set; }

        [JsonPropertyName("is_stub")]
        public bool IsStub { get; set; }

        [JsonPropertyName("is_rendering")]
        public bool IsRendering { get; set; }
    }

    public class StickerVmoji {
        public StickerVmoji() { }

        [JsonPropertyName("character_id")]
        public string CharacterId { get; set; }
    }

    public class Sticker {
        public Sticker() { }

        [JsonPropertyName("animation_url")]
        public string AnimationUrl { get; set; }

        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [JsonPropertyName("sticker_id")]
        public int StickerId { get; set; }

        [JsonPropertyName("is_allowed")]
        public bool IsAllowed { get; set; }

        [JsonPropertyName("images")]
        public List<StickerImage> Images { get; set; }

        [JsonPropertyName("images_with_background")]
        public List<StickerImage> ImagesWithBackground { get; set; }

        [JsonPropertyName("render")]
        public StickerRender Render { get; set; }

        [JsonPropertyName("vmoji")]
        public StickerVmoji Vmoji { get; set; }

        [JsonIgnore]
        public bool IsPartial { get { return Images == null && ImagesWithBackground == null; } }
    }

    public class StickerDictionary {
        [JsonPropertyName("words")]
        public List<string> Words { get; set; }

        [JsonPropertyName("user_stickers")]
        public List<Sticker> UserStickers { get; set; }

        [JsonPropertyName("promoted_stickers")]
        public List<Sticker> PromotedStickers { get; set; }
    }

    public class StickersKeywordsResponse {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("chunks_count")]
        public int ChunksCount { get; set; }

        [JsonPropertyName("chunks_hash")]
        public string ChunksHash { get; set; }

        [JsonPropertyName("dictionary")]
        public List<StickerDictionary> Dictionary { get; set; }
    }

    [DataContract]
    public enum UGCStickerStatus {
        [EnumMember(Value = "created")]
        Created,

        [EnumMember(Value = "passed")]
        Passed,

        [EnumMember(Value = "in_review")]
        InReview,

        [EnumMember(Value = "banned")]
        Banned,

        [EnumMember(Value = "rejected")]
        Rejected,
    }

    public class UGCSticker {
        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; }

        [JsonPropertyName("pack_id")]
        public long PackId { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("images")]
        public List<StickerImage> Images { get; set; }

        [JsonPropertyName("restrictions")]
        public List<string> Restrictions { get; set; }

        [JsonPropertyName("active_restriction")]
        public string ActiveRestriction { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<UGCStickerStatus>))]
        public UGCStickerStatus Status { get; set; }

        [JsonPropertyName("status_description")]
        public string StatusDescription { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("is_claimed")]
        public bool IsClaimed { get; set; }
    }
}