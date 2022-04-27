using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    [DataContract]
    public enum GroupState {
        Open = 0,
        Closed = 1,
        Private = 2
    }

    [DataContract]
    public enum AdminLevel {
        Moderator = 1,
        Editor = 2,
        Administrator = 3
    }

    [DataContract]
    public enum GroupType {
        [EnumMember(Value = "group")]
        Group,

        [EnumMember(Value = "page")]
        Page,

        [EnumMember(Value = "event")]
        Event
    }

    public class GroupCoverImage {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class GroupCover {
        [JsonProperty("enabled")]
        public int Enabled { get; set; }

        [JsonProperty("images")]
        public List<GroupCoverImage> Images { get; set; }
    }

    public class Group {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty("is_closed")]
        public GroupState State { get; set; }

        [JsonProperty("deactivated")]
        public DeactivationState Deactivated { get; set; }

        [JsonProperty("is_admin")]
        public int IsAdmin { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        // admin level

        [JsonProperty("is_member")]
        public int IsMember { get; set; }

        [JsonProperty("type")]
        public GroupType Type { get; set; }

        [JsonProperty("photo_50")]
        public string Photo50 { get; set; }

        [JsonProperty("photo_100")]
        public string Photo100 { get; set; }

        [JsonProperty("photo_200")]
        public string Photo200 { get; set; }

        [JsonIgnore]
        public Uri Photo {
            get {
                Uri p100 = !String.IsNullOrEmpty(Photo100) && Uri.IsWellFormedUriString(Photo100, UriKind.Absolute) ? new Uri(Photo100) : new Uri("https://vk.com/images/community_100.png");
                Uri p200 = !String.IsNullOrEmpty(Photo200) && Uri.IsWellFormedUriString(Photo200, UriKind.Absolute) ? new Uri(Photo200) : new Uri("https://vk.com/images/community_200.png");
                return String.IsNullOrEmpty(Photo200) ? p100 : p200;
            }
        }

        [JsonProperty("activity")]
        public string Activity { get; set; }

        [JsonProperty("can_message")]
        public bool CanMessage { get; set; }

        [JsonProperty("city")]
        public UserCountry City { get; set; }

        [JsonProperty("country")]
        public UserCountry Country { get; set; }

        [JsonProperty("cover")]
        public GroupCover Cover { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("members_count")]
        public long Members { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class GroupsResponse {
        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }

        [JsonProperty("profiles")]
        public List<User> Profiles { get; set; }
    }

    // Event object in messages attachments
    public class Event {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("friends")]
        public List<int> Friends { get; set; }
    }
}
