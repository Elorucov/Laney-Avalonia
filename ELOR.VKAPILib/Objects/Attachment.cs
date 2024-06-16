using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace ELOR.VKAPILib.Objects {
    public enum AttachmentType {
        Unknown,

        [EnumMember(Value = "photo")]
        Photo,

        [EnumMember(Value = "album")]
        Album,

        [EnumMember(Value = "video")]
        Video,

        [EnumMember(Value = "audio")]
        Audio,

        [EnumMember(Value = "audio_message")]
        AudioMessage,

        [EnumMember(Value = "podcast")]
        Podcast,

        [EnumMember(Value = "doc")]
        Document,

        [EnumMember(Value = "graffiti")]
        Graffiti,

        [EnumMember(Value = "link")]
        Link,

        [EnumMember(Value = "poll")]
        Poll,

        [EnumMember(Value = "page")]
        Page,

        [EnumMember(Value = "market")]
        Market,

        [EnumMember(Value = "wall")]
        Wall,

        [EnumMember(Value = "wall_reply")]
        WallReply,

        [EnumMember(Value = "sticker")]
        Sticker,

        [EnumMember(Value = "story")]
        Story,

        [EnumMember(Value = "gift")]
        Gift,

        [EnumMember(Value = "call")]
        Call,

        [EnumMember(Value = "group_call_in_progress")]
        GroupCallInProgress,

        [EnumMember(Value = "event")]
        Event,

        [EnumMember(Value = "curator")]
        Curator,

        [EnumMember(Value = "narrative")]
        Narrative,

        [EnumMember(Value = "textpost_publish")]
        TextpostPublish,
    }

    public class AttachmentBase {
        public AttachmentBase() {}
            
        [JsonIgnore]
        public virtual string ObjectType { get; set; }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("access_key")]
        public string AccessKey { get; set; }

        public override string ToString() {
            string ak = String.IsNullOrEmpty(AccessKey) ? "" : $"_{AccessKey}";
            return $"{ObjectType}{OwnerId}_{Id}{ak}";
        }
    }

    public class Attachment {
        public Attachment() {}

        [JsonPropertyName("type")]
        public string TypeString { get; set; }

        [JsonIgnore]
        public AttachmentType Type { get { return GetAttachmentEnum(TypeString); } }

        [JsonPropertyName("photo")]
        public Photo Photo { get; set; }

        [JsonPropertyName("video")]
        public Video Video { get; set; }

        [JsonPropertyName("audio")]
        public Audio Audio { get; set; }

        [JsonPropertyName("audio_message")]
        public AudioMessage AudioMessage { get; set; }

        [JsonPropertyName("podcast")]
        public Podcast Podcast { get; set; }

        [JsonPropertyName("doc")]
        public Document Document { get; set; }

        [JsonPropertyName("link")]
        public Link Link { get; set; }

        [JsonPropertyName("market")]
        public Market Market { get; set; }

        [JsonPropertyName("wall")]
        public WallPost Wall { get; set; }

        [JsonPropertyName("wall_reply")]
        public WallReply WallReply { get; set; }

        [JsonPropertyName("sticker")]
        public Sticker Sticker { get; set; }

        [JsonPropertyName("graffiti")]
        public Graffiti Graffiti { get; set; }

        [JsonPropertyName("story")]
        public Story Story { get; set; }

        [JsonPropertyName("gift")]
        public Gift Gift { get; set; }

        [JsonPropertyName("call")]
        public Call Call { get; set; }

        [JsonPropertyName("group_call_in_progress")]
        public GroupCallInProgress GroupCallInProgress { get; set; }

        [JsonPropertyName("poll")]
        public Poll Poll { get; set; }

        [JsonPropertyName("event")]
        public Event Event { get; set; }

        [JsonPropertyName("curator")]
        public Curator Curator { get; set; }

        [JsonPropertyName("narrative")]
        public Narrative Narrative { get; set; }

        [JsonPropertyName("textpost_publish")]
        public TextpostPublish TextpostPublish { get; set; }

        internal static AttachmentType GetAttachmentEnum(string type) {
            switch (type) {
                case "photo": return AttachmentType.Photo;
                case "album": return AttachmentType.Album;
                case "video": return AttachmentType.Video;
                case "audio": return AttachmentType.Audio;
                case "audio_message": return AttachmentType.AudioMessage;
                case "podcast": return AttachmentType.Podcast;
                case "doc": return AttachmentType.Document;
                case "graffiti": return AttachmentType.Graffiti;
                case "link": return AttachmentType.Link;
                case "poll": return AttachmentType.Poll;
                case "page": return AttachmentType.Page;
                case "market": return AttachmentType.Market;
                case "wall": return AttachmentType.Wall;
                case "wall_reply": return AttachmentType.WallReply;
                case "sticker": return AttachmentType.Sticker;
                case "story": return AttachmentType.Story;
                case "event": return AttachmentType.Event;
                case "gift": return AttachmentType.Gift;
                case "call": return AttachmentType.Call;
                case "group_call_in_progress": return AttachmentType.GroupCallInProgress;
                case "curator": return AttachmentType.Curator;
                case "narrative": return AttachmentType.Narrative;
                case "textpost_publish": return AttachmentType.TextpostPublish;
                default: return AttachmentType.Unknown;
            }
        }
    }
}