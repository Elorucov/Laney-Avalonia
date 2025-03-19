using ELOR.VKAPILib.Attributes;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Groups {
    public enum BanReason {
        Other = 0,
        Spam = 1,
        VerbalAbuse = 2,
        StrongLanguage = 3,
        IrrelevantMessages = 4
    }

    public enum MemberType {
        [EnumMember(Value = "profile")]
        Profile,

        [EnumMember(Value = "group")]
        Group
    }

    public class BanInfo {
        [JsonPropertyName("admin_id")]
        public long AdminId { get; set; }

        [JsonPropertyName("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("reason")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<BanReason>))]
        public BanReason Reason { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("end_date")]
        public int EndDateUnix { get; set; }

        [JsonIgnore]
        public DateTime EndDate { get { return DateTimeOffset.FromUnixTimeSeconds(EndDateUnix).DateTime.ToLocalTime(); } }
    }

    public class BannedMembers {
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<MemberType>))]
        public MemberType Type { get; set; }

        [JsonPropertyName("profile")]
        public User Profile { get; set; }

        [JsonPropertyName("group")]
        public Group Group { get; set; }

        [JsonPropertyName("ban_info")]
        public BanInfo BanInfo { get; set; }
    }
}