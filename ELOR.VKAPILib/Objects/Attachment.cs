using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    [DataContract]
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
        Curator
    }

    public class AttachmentBase {
        [JsonIgnore]
        public virtual string ObjectType { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("access_key")]
        public string AccessKey { get; set; }

        public override string ToString() {
            string ak = String.IsNullOrEmpty(AccessKey) ? "" : $"_{AccessKey}";
            return $"{ObjectType}{OwnerId}_{Id}{ak}";
        }
    }

    public class Attachment {
        [JsonProperty("type")]
        public string TypeString { get; set; }

        [JsonIgnore]
        public AttachmentType Type { get { return GetAttachmentEnum(); } }

        [JsonProperty("photo")]
        public Photo Photo { get; set; }

        [JsonProperty("video")]
        public Video Video { get; set; }

        [JsonProperty("audio")]
        public Audio Audio { get; set; }

        [JsonProperty("audio_message")]
        public AudioMessage AudioMessage { get; set; }

        [JsonProperty("podcast")]
        public Podcast Podcast { get; set; }

        [JsonProperty("doc")]
        public Document Document { get; set; }

        [JsonProperty("link")]
        public Link Link { get; set; }

        [JsonProperty("market")]
        public Market Market { get; set; }

        [JsonProperty("wall")]
        public WallPost Wall { get; set; }

        [JsonProperty("wall_reply")]
        public WallReply WallReply { get; set; }

        [JsonProperty("sticker")]
        public Sticker Sticker { get; set; }

        [JsonProperty("graffiti")]
        public Graffiti Graffiti { get; set; }

        [JsonProperty("story")]
        public Story Story { get; set; }

        [JsonProperty("gift")]
        public Gift Gift { get; set; }

        [JsonProperty("call")]
        public Call Call { get; set; }

        [JsonProperty("group_call_in_progress")]
        public GroupCallInProgress GroupCallInProgress { get; set; }

        [JsonProperty("poll")]
        public Poll Poll { get; set; }

        [JsonProperty("event")]
        public Event Event { get; set; }

        [JsonProperty("curator")]
        public Curator Curator { get; set; }

        private AttachmentType GetAttachmentEnum() {
            switch(TypeString) {
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
                default: return AttachmentType.Unknown;
            }
        }
    }
}