using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    [DataContract]
    public enum StoryType {
        [EnumMember(Value = "photo")]
        Photo,

        [EnumMember(Value = "video")]
        Video
    }

    public class StoryLink {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }
    }

    public class ClickableStickerAreaPoints {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }

    public class ClickableSticker {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("mention")]
        public string Mention { get; set; }

        [JsonProperty("hashtag")]
        public string Hashtag { get; set; }

        [JsonProperty("place_id")]
        public int PlaceId { get; set; }

        [JsonProperty("market_item")]
        public Market MarketItem { get; set; }

        [JsonProperty("poll")]
        public Poll Poll { get; set; }

        [JsonProperty("sticker_id")]
        public int StickerId { get; set; }

        [JsonProperty("sticker_pack_id")]
        public int StickerPackId { get; set; }

        [JsonProperty("link_object")]
        public Link LinkObject { get; set; }

        [JsonProperty("post_id")]
        public int PostId { get; set; }

        [JsonProperty("post_owner_id")]
        public int PostOwnerId { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("story_id")]
        public int StoryId { get; set; }

        [JsonProperty("clickable_area")]
        public List<ClickableStickerAreaPoints> ClickableArea { get; set; }
    }

    public class ClickableStickersInfo {
        [JsonProperty("original_height")]
        public int OriginalHeight { get; set; }

        [JsonProperty("original_width")]
        public int OriginalWidth { get; set; }

        [JsonProperty("clickable_stickers")]
        public List<ClickableSticker> ClickableStickers { get; set; }
    }

    public class Story : AttachmentBase {
        [JsonIgnore]
        public override string ObjectType { get { return "story"; } }

        [JsonProperty("can_see")]
        public int CanSee { get; set; }

        [JsonProperty("can_like")]
        public bool CanLike { get; set; }

        [JsonProperty("can_share")]
        public int CanShare { get; set; }

        [JsonProperty("is_resricted")]
        public bool IsRestricted { get; set; }

        [JsonProperty("is_expired")]
        public bool IsExpired { get; set; }

        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }

        [JsonProperty("seen")]
        public int Seen { get; set; }

        [JsonProperty("link")]
        public StoryLink Link { get; set; }

        [JsonProperty("type")]
        public StoryType Type { get; set; }

        [JsonProperty("photo")]
        public Photo Photo { get; set; }

        [JsonProperty("video")]
        public Video Video { get; set; }

        [JsonProperty("clickable_stickers")]
        public ClickableStickersInfo ClickableStickers { get; set; }
    }
}
