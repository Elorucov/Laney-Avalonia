using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class WallPostViews {
        [JsonProperty("count")]
        public int Count { get; set; }
    }

    [DataContract]
    public enum PostType {
        [EnumMember(Value = "post")]
        Post,

        [EnumMember(Value = "post_ads")]
        Ads,

        [EnumMember(Value = "copy")]
        Copy,

        [EnumMember(Value = "reply")]
        Reply,

        [EnumMember(Value = "postpone")]
        Postpone,

        [EnumMember(Value = "suggest")]
        Suggest,
    }

    public class WallPost : AttachmentBase {
        [JsonIgnore]
        public override string ObjectType { get { return "wall"; } }

        [JsonProperty("from_id")]
        public long FromId { get; set; }

        [JsonProperty("to_id")]
        public long ToId { get; set; }

        [JsonIgnore]
        public long OwnerOrToId { get { return OwnerId != 0 ? OwnerId : ToId; } } // #лучшееапивинтернете

        [JsonProperty("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("reply_owner_id")]
        public long ReplyOwnerId { get; set; }

        [JsonProperty("reply_post_id")]
        public int ReplyPostId { get; set; }

        [JsonProperty("friends_only")]
        public int FriendsOnly { get; set; }

        // Comments, likes and reposts

        [JsonProperty("views")]
        public WallPostViews Views { get; set; }

        [JsonProperty("attachments")]
        public List<Attachment> Attachments { get; set; }

        [JsonProperty("geo")]
        public Geo Geo { get; set; }

        [JsonProperty("signer_id")]
        public long SignerId { get; set; }

        [JsonProperty("is_pinned")]
        public int IsPinned { get; set; }

        [JsonProperty("marked_as_ads")]
        public int MarkedAsAds { get; set; }

        public WallPost(string lpatch = null) {
            if (lpatch != null) {
                string[] ids = lpatch.Split('_');
                FromId = Int32.Parse(ids[0]);
                Id = Int32.Parse(ids[1]);
            }
        }
    }

    public class WallReply {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("from_id")]
        public long FromId { get; set; }

        [JsonProperty("post_id")]
        public int PostId { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("reply_to_user")]
        public long ReplyToUser { get; set; }

        [JsonProperty("reply_to_comment")]
        public int ReplyToComment { get; set; }

    }
}