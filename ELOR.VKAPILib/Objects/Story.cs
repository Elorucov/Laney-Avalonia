using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using ELOR.VKAPILib.Attributes;

namespace ELOR.VKAPILib.Objects {

    public enum StoryType {
        [EnumMember(Value = "photo")]
        Photo,

        [EnumMember(Value = "video")]
        Video
    }

    public class StoryLink {
        public StoryLink() {}

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }
    }

    public class ClickableStickerAreaPoints {
        public ClickableStickerAreaPoints() {}

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class ClickableSticker {
        public ClickableSticker() {}

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("mention")]
        public string Mention { get; set; }

        [JsonPropertyName("hashtag")]
        public string Hashtag { get; set; }

        [JsonPropertyName("place_id")]
        public int PlaceId { get; set; }

        [JsonPropertyName("market_item")]
        public Market MarketItem { get; set; }

        [JsonPropertyName("poll")]
        public Poll Poll { get; set; }

        [JsonPropertyName("sticker_id")]
        public int StickerId { get; set; }

        [JsonPropertyName("sticker_pack_id")]
        public int StickerPackId { get; set; }

        [JsonPropertyName("link_object")]
        public Link LinkObject { get; set; }

        [JsonPropertyName("post_id")]
        public int PostId { get; set; }

        [JsonPropertyName("post_owner_id")]
        public long PostOwnerId { get; set; }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; }

        [JsonPropertyName("story_id")]
        public int StoryId { get; set; }

        [JsonPropertyName("clickable_area")]
        public List<ClickableStickerAreaPoints> ClickableArea { get; set; }
    }

    public class ClickableStickersInfo {
        public ClickableStickersInfo() {}

        [JsonPropertyName("original_height")]
        public int OriginalHeight { get; set; }

        [JsonPropertyName("original_width")]
        public int OriginalWidth { get; set; }

        [JsonPropertyName("clickable_stickers")]
        public List<ClickableSticker> ClickableStickers { get; set; }
    }

    public class Story : AttachmentBase {
        public Story() {}

        [JsonIgnore]
        public override string ObjectType { get { return "story"; } }

        [JsonPropertyName("can_see")]
        public int CanSee { get; set; }

        //[JsonPropertyName("can_like")]
        //public bool CanLike { get; set; }

        [JsonPropertyName("can_share")]
        public int CanShare { get; set; }

        [JsonPropertyName("is_resricted")]
        public bool IsRestricted { get; set; }

        [JsonPropertyName("is_expired")]
        public bool IsExpired { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("views")]
        public int Views { get; set; }

        [JsonPropertyName("seen")]
        public int Seen { get; set; }

        [JsonPropertyName("link")]
        public StoryLink Link { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<StoryType>))]
        public StoryType Type { get; set; }

        [JsonPropertyName("photo")]
        public Photo Photo { get; set; }

        [JsonPropertyName("video")]
        public Video Video { get; set; }

        [JsonPropertyName("clickable_stickers")]
        public ClickableStickersInfo ClickableStickers { get; set; }
    }
}