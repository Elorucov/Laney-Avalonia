using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ELOR.VKAPILib.Objects.Groups {
    public enum BanReason {
        Other = 0,
        Spam = 1,
        VerbalAbuse = 2,
        StrongLanguage = 3,
        IrrelevantMessages = 4
    }

    [DataContract]
    public enum MemberType {
        [EnumMember(Value = "profile")]
        Profile,

        [EnumMember(Value = "group")]
        Group
    }

    public class BanInfo {
        [JsonProperty("admin_id")]
        public int AdminId { get; set; }

        [JsonProperty("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("reason")]
        public BanReason Reason { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("end_date")]
        public int EndDateUnix { get; set; }

        [JsonIgnore]
        public DateTime EndDate { get { return DateTimeOffset.FromUnixTimeSeconds(EndDateUnix).DateTime.ToLocalTime(); } }
    }

    public class BannedMembers {
        [JsonProperty("type")]
        public MemberType Type { get; set; }

        [JsonProperty("profile")]
        public User Profile { get; set; }

        [JsonProperty("group")]
        public Group Group { get; set; }

        [JsonProperty("ban_info")]
        public BanInfo BanInfo { get; set; }
    }
}
