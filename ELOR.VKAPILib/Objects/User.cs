using ELOR.VKAPILib.Attributes;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {

    public enum DeactivationState {
        No,

        [EnumMember(Value = "deleted")]
        Deleted,

        [EnumMember(Value = "banned")]
        Banned
    }

    public enum Sex {
        Train = 0,
        Female = 1,
        Male = 2
    }

    public enum FriendStatus {
        None = 0,
        RequestSent = 1,
        InboundRequest = 2,
        IsFriend = 3
    }

    public enum UserOccupationType {
        [EnumMember(Value = "work")]
        Work,

        [EnumMember(Value = "school")]
        School,

        [EnumMember(Value = "university")]
        University,
    }

    public enum UserOnlineStatus {
        Unknown,

        [EnumMember(Value = "not_show")]
        NotShow,

        [EnumMember(Value = "recently")]
        Recently,

        [EnumMember(Value = "last_week")]
        LastWeek,

        [EnumMember(Value = "last_month")]
        LastMonth,

        [EnumMember(Value = "long_ago")]
        LongAgo,
    }

    public class UserLastSeen {
        public UserLastSeen() { }

        [JsonPropertyName("time")]
        public long TimeUnix { get; set; }

        [JsonIgnore]
        public DateTime Time { get { return DateTimeOffset.FromUnixTimeSeconds(TimeUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("platform")]
        public int Platform { get; set; }
    }

    public class UserOnlineInfo {
        public UserOnlineInfo() { }

        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [JsonPropertyName("is_online")]
        public bool IsOnline { get; set; }

        [JsonPropertyName("app_id")]
        public int AppId { get; set; }

        [JsonPropertyName("is_mobile")]
        public bool IsMobile { get; set; }

        [JsonPropertyName("last_seen")]
        public long LastSeenUnix { get; set; }

        [JsonIgnore]
        public DateTime LastSeen { get { return DateTimeOffset.FromUnixTimeSeconds(LastSeenUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<UserOnlineStatus>))]
        public UserOnlineStatus Status { get; set; }
    }

    public class UserCountry {
        public UserCountry() { }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

    public class UserCareer {
        public UserCareer() { }

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("position")]
        public string Position { get; set; }
    }

    public class UserOccupation {
        public UserOccupation() { }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<UserOccupationType>))]
        public UserOccupationType Type { get; set; }

        [JsonPropertyName("id")]
        public double Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class UserOwnerState {
        public UserOwnerState() { }

        [JsonPropertyName("unban_date")]
        public long UnbanDate { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class User {
        public User() { }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("first_name_gen")]
        public string FirstNameGen { get; set; }

        [JsonPropertyName("first_name_dat")]
        public string FirstNameDat { get; set; }

        [JsonPropertyName("first_name_acc")]
        public string FirstNameAcc { get; set; }

        [JsonPropertyName("first_name_ins")]
        public string FirstNameIns { get; set; }

        [JsonPropertyName("first_name_abl")]
        public string FirstNameAbl { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("last_name_gen")]
        public string LastNameGen { get; set; }

        [JsonPropertyName("last_name_dat")]
        public string LastNameDat { get; set; }

        [JsonPropertyName("last_name_acc")]
        public string LastNameAcc { get; set; }

        [JsonPropertyName("last_name_ins")]
        public string LastNameIns { get; set; }

        [JsonPropertyName("last_name_abl")]
        public string LastNameAbl { get; set; }

        [JsonIgnore]
        public string FullName { get { return $"{FirstName} {LastName}"; } }

        [JsonPropertyName("nickname")]
        public string NickName { get; set; }

        [JsonPropertyName("is_closed")]
        public bool IsClosed { get; set; }

        [JsonPropertyName("can_access_closed")]
        public bool CanAccessClosed { get; set; }

        [JsonPropertyName("deactivated")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<DeactivationState>))]
        public DeactivationState Deactivated { get; set; }

        [JsonPropertyName("friend_status")]
        public FriendStatus FriendStatus { get; set; }

        [JsonPropertyName("bdate")]
        public string BirthDate { get; set; }

        [JsonPropertyName("city")]
        public UserCountry City { get; set; }

        [JsonPropertyName("country")]
        public UserCountry Country { get; set; }

        [JsonPropertyName("career")]
        public List<UserCareer> Career { get; set; }

        [JsonPropertyName("occupation")]
        public UserOccupation Occupation { get; set; }

        [JsonPropertyName("blacklisted")]
        public int Blacklisted { get; set; }

        [JsonPropertyName("blacklisted_by_me")]
        public int BlacklistedByMe { get; set; }

        [JsonPropertyName("can_send_friend_request")]
        public int CanSendFriendRequest { get; set; }

        [JsonPropertyName("can_write_private_message")]
        public int CanWritePrivateMessage { get; set; }

        [JsonPropertyName("last_seen")]
        public UserLastSeen LastSeen { get; set; }

        [JsonPropertyName("screen_name")]
        public string ScreenName { get; set; }

        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        [JsonPropertyName("sex")]
        public Sex Sex { get; set; }

        [JsonPropertyName("photo_50")]
        public string Photo50 { get; set; }

        [JsonPropertyName("photo_100")]
        public string Photo100 { get; set; }

        [JsonPropertyName("photo_200")]
        public string Photo200 { get; set; }

        [JsonIgnore]
        public Uri Photo {
            get {
                if (Uri.IsWellFormedUriString(Photo200, UriKind.Absolute)) return new Uri(Photo200);
                if (Uri.IsWellFormedUriString(Photo100, UriKind.Absolute)) return new Uri(Photo100);
                if (Uri.IsWellFormedUriString(Photo50, UriKind.Absolute)) return new Uri(Photo50);
                return new Uri("https://vk.ru/images/camera_200.png");
            }
        }

        [JsonPropertyName("followers_count")]
        public long Followers { get; set; }

        [JsonPropertyName("online_info")]
        public UserOnlineInfo OnlineInfo { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonPropertyName("mobile_phone")]
        public string MobilePhone { get; set; }

        [JsonPropertyName("verified")]
        public int Verified { get; set; }

        [JsonPropertyName("owner_state")]
        public UserOwnerState OwnerState { get; set; }
    }
}