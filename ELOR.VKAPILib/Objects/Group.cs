using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using ELOR.VKAPILib.Attributes;

namespace ELOR.VKAPILib.Objects {
    public enum GroupState {
        Open = 0,
        Closed = 1,
        Private = 2
    }

    public enum AdminLevel {
        Moderator = 1,
        Editor = 2,
        Administrator = 3
    }

    public enum GroupType {
        [EnumMember(Value = "group")]
        Group,

        [EnumMember(Value = "page")]
        Page,

        [EnumMember(Value = "event")]
        Event
    }

    public class GroupCoverImage {
        public GroupCoverImage() {}

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class GroupCover {
        public GroupCover() {}

        [JsonPropertyName("enabled")]
        public int Enabled { get; set; }

        [JsonPropertyName("images")]
        public List<GroupCoverImage> Images { get; set; }
    }

    public class Group {
        public Group() {}

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("screen_name")]
        public string ScreenName { get; set; }

        [JsonPropertyName("is_closed")]
        public GroupState State { get; set; }

        [JsonPropertyName("deactivated")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<DeactivationState>))]
        public DeactivationState Deactivated { get; set; }

        [JsonPropertyName("is_admin")]
        public int IsAdmin { get; set; }

        [JsonPropertyName("verified")]
        public int Verified { get; set; }

        // admin level

        [JsonPropertyName("is_member")]
        public int IsMember { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<GroupType>))]
        public GroupType Type { get; set; }

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
                return new Uri("https://vk.com/images/community_200.png");
            }
        }

        [JsonPropertyName("activity")]
        public string Activity { get; set; }

        [JsonPropertyName("can_message")]
        public int CanMessage { get; set; }

        [JsonPropertyName("city")]
        public UserCountry City { get; set; }

        [JsonPropertyName("country")]
        public UserCountry Country { get; set; }

        [JsonPropertyName("cover")]
        public GroupCover Cover { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("members_count")]
        public long Members { get; set; }

        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class GroupsResponse {
        public GroupsResponse() {}

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }
    }

    // Event object in messages attachments
    public class Event {
        public Event() {}

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("button_text")]
        public string ButtonText { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("friends")]
        public List<long> Friends { get; set; }
    }
}