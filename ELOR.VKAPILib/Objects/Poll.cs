using ELOR.VKAPILib.Attributes;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public enum PollBackgroundType {
        Unknown,

        [EnumMember(Value = "gradient")]
        Gradient,

        [EnumMember(Value = "tile")]
        Tile
    }

    public class PollAnswer {
        public PollAnswer() { }

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("votes")]
        public int Votes { get; set; }

        [JsonPropertyName("rate")]
        public double Rate { get; set; }
    }

    public class PollBackgroundGradientPosition {
        public PollBackgroundGradientPosition() { }

        [JsonPropertyName("position")]
        public double Position { get; set; }

        [JsonPropertyName("color")]
        public string ColorHEX { get; set; }
    }

    public class PollBackground {
        public PollBackground() { }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<PollBackgroundType>))]
        public PollBackgroundType Type { get; set; }

        [JsonPropertyName("angle")]
        public int Angle { get; set; }

        [JsonPropertyName("color")]
        public string ColorHEX { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("images")]
        public List<PhotoSizes> Images { get; set; }

        [JsonPropertyName("points")]
        public List<PollBackgroundGradientPosition> Points { get; set; }
    }

    public class PollPhoto {
        public PollPhoto() { }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("color")]
        public string ColorHEX { get; set; }

        [JsonPropertyName("images")]
        public List<PhotoSizes> Images { get; set; }
    }

    public class Poll : AttachmentBase {
        public Poll() { }

        [JsonIgnore]
        public override string ObjectType { get { return "poll"; } }

        [JsonPropertyName("created")]
        public int CreatedUnix { get; set; }

        [JsonIgnore]
        public DateTime Created { get { return DateTimeOffset.FromUnixTimeSeconds(CreatedUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("question")]
        public string Question { get; set; }

        [JsonPropertyName("votes")]
        public int Votes { get; set; }

        [JsonPropertyName("answers")]
        public List<PollAnswer> Answers { get; set; }

        [JsonPropertyName("answer_ids")]
        public List<ulong> AnswerIds { get; set; }

        [JsonPropertyName("anonymous")]
        public bool Anonymous { get; set; }

        [JsonPropertyName("multiple")]
        public bool Multiple { get; set; }

        [JsonPropertyName("disable_unvote")]
        public bool DisableUnvote { get; set; }

        [JsonPropertyName("end_date")]
        public int EndDateUnix { get; set; }

        [JsonIgnore]
        public DateTime EndDate { get { return DateTimeOffset.FromUnixTimeSeconds(EndDateUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("closed")]
        public bool Closed { get; set; }

        [JsonPropertyName("can_vote")]
        public bool CanVote { get; set; }

        [JsonPropertyName("can_share")]
        public bool CanShare { get; set; }

        [JsonPropertyName("author_id")]
        public long AuthorId { get; set; }

        [JsonPropertyName("background")]
        public PollBackground Background { get; set; }

        [JsonPropertyName("photo")]
        public PollPhoto Photo { get; set; }

        [JsonPropertyName("friends")]
        public List<User> Friends { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }
}