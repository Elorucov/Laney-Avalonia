using ELOR.VKAPILib.Objects;
using System.Runtime.Serialization;

namespace ELOR.VKAPILib.Methods {
    public enum FriendsOrder {
        [EnumMember(Value = "hints")]
        Hints,

        [EnumMember(Value = "random")]
        Random,

        [EnumMember(Value = "mobile")]
        Mobile,

        [EnumMember(Value = "name")]
        Name,
    }

    public class FriendsMethods : MethodsSectionBase {
        internal FriendsMethods(VKAPI api) : base(api) { }

        public async Task<int> AddAsync(long userId, string text = null, bool follow = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("user_id", userId.ToString());
            if (!String.IsNullOrEmpty(text)) parameters.Add("text", text);
            if (follow) parameters.Add("follow", "1");
            return await API.CallMethodAsync<int>("friends.add", parameters);
        }

        public async Task<UsersList> GetAsync(List<string> fields, long userId = 0, FriendsOrder order = FriendsOrder.Hints, int listId = 0, int count = 5000, int offset = 0, NameCase nameCase = NameCase.Nom) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userId > 0) parameters.Add("user_id", userId.ToString());
            parameters.Add("order", order.ToEnumMemberAttribute());
            if (listId > 0) parameters.Add("list_id", listId.ToString());
            parameters.Add("count", count > 0 ? count.ToString() : "5000");
            if (offset > 0) parameters.Add("offset", offset.ToString());
            parameters.Add("fields", fields.Combine());
            parameters.Add("name_case", nameCase.ToEnumMemberAttribute());
            return await API.CallMethodAsync<UsersList>("friends.get", parameters);
        }

        public async Task<UsersList> SearchAsync(long userId, string query, int count = 1000, int offset = 0, List<string> fields = null, NameCase nameCase = NameCase.Nom) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "user_id", userId.ToString() },
                { "q", query },
                { "count", count.ToString() }
            };
            if (offset > 0) parameters.Add("offset", offset.ToString());
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            parameters.Add("name_case", nameCase.ToEnumMemberAttribute());
            return await API.CallMethodAsync<UsersList>("friends.search", parameters);
        }
    }
}