using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class WallPostViews {
        public WallPostViews() {}

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class WallPost : AttachmentBase {
        public WallPost() {}

        [JsonIgnore]
        public override string ObjectType { get { return "wall"; } }

        [JsonPropertyName("from_id")]
        public long FromId { get; set; }

        [JsonPropertyName("to_id")]
        public long ToId { get; set; }

        [JsonIgnore]
        public long OwnerOrToId { get { return OwnerId != 0 ? OwnerId : ToId; } } // #лучшееапивинтернете

        [JsonPropertyName("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("reply_owner_id")]
        public long ReplyOwnerId { get; set; }

        [JsonPropertyName("reply_post_id")]
        public int ReplyPostId { get; set; }

        [JsonPropertyName("friends_only")]
        public int FriendsOnly { get; set; }

        // Comments, likes and reposts

        [JsonPropertyName("views")]
        public WallPostViews Views { get; set; }

        [JsonPropertyName("attachments")]
        public List<Attachment> Attachments { get; set; }

        //[JsonPropertyName("geo")]
        //public Geo Geo { get; set; }

        [JsonPropertyName("signer_id")]
        public long SignerId { get; set; }

        [JsonPropertyName("is_pinned")]
        public int IsPinned { get; set; }

        [JsonPropertyName("marked_as_ads")]
        public int MarkedAsAds { get; set; }
    }

    public class WallReply {
        public WallReply() {}

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("from_id")]
        public long FromId { get; set; }

        [JsonPropertyName("post_id")]
        public int PostId { get; set; }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("reply_to_user")]
        public long ReplyToUser { get; set; }

        [JsonPropertyName("reply_to_comment")]
        public int ReplyToComment { get; set; }
    }
}