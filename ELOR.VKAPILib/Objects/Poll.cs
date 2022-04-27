using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects
{
    [DataContract]
    public enum PollBackgroundType {
        Unknown,

        [EnumMember(Value = "gradient")]
        Gradient,

        [EnumMember(Value = "tile")]
        Tile
    }

    public class PollAnswer {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("votes")]
        public int Votes { get; set; }

        [JsonProperty("rate")]
        public double Rate { get; set; }
    }

    public class PollBackgroundGradientPosition {
        [JsonProperty("position")]
        public double Position { get; set; }

        [JsonProperty("color")]
        public string ColorHEX { get; set; }
    }

    public class PollBackground {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public PollBackgroundType Type { get; set; }

        [JsonProperty("angle")]
        public int Angle { get; set; }

        [JsonProperty("color")]
        public string ColorHEX { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("images")]
        public List<PhotoSizes> Images { get; set; }

        [JsonProperty("points")]
        public List<PollBackgroundGradientPosition> Points { get; set; }
    }

    public class PollPhoto {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("color")]
        public string ColorHEX { get; set; }

        [JsonProperty("images")]
        public List<PhotoSizes> Images { get; set; }
    }

    public class Poll : AttachmentBase {
        [JsonIgnore]
        public override string ObjectType { get { return "poll"; } }

        [JsonProperty("created")]
        public int CreatedUnix { get; set; }

        [JsonIgnore]
        public DateTime Created { get { return DateTimeOffset.FromUnixTimeSeconds(CreatedUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("votes")]
        public int Votes { get; set; }

        [JsonProperty("answers")]
        public List<PollAnswer> Answers { get; set; }

        [JsonProperty("answer_ids")]
        public List<int> AnswerIds { get; set; }

        [JsonProperty("anonymous")]
        public bool Anonymous { get; set; }

        [JsonProperty("multiple")]
        public bool Multiple { get; set; }

        [JsonProperty("disable_unvote")]
        public bool DisableUnvote { get; set; }

        [JsonProperty("end_date")]
        public int EndDateUnix { get; set; }

        [JsonIgnore]
        public DateTime EndDate { get { return DateTimeOffset.FromUnixTimeSeconds(EndDateUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("closed")]
        public bool Closed { get; set; }

        [JsonProperty("can_vote")]
        public bool CanVote { get; set; }

        [JsonProperty("can_share")]
        public bool CanShare { get; set; }

        [JsonProperty("author_id")]
        public int AuthorId { get; set; }

        [JsonProperty("background")]
        public PollBackground Background { get; set; }

        [JsonProperty("photo")]
        public PollPhoto Photo { get; set; }

        [JsonProperty("friends")]
        public List<User> Friends { get; set; }

        [JsonProperty("profiles")]
        public List<User> Profiles { get; set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }
    }
}