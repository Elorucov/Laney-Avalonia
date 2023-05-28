using ELOR.VKAPILib.Attributes;
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

    [Section("friends")]
    public class FriendsMethods : MethodsSectionBase {
        internal FriendsMethods(VKAPI api) : base(api) { }

        [Method("add")]
        public async Task<int> AddAsync(int userId, string text = null, bool follow = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("user_id", userId.ToString());
            if (!String.IsNullOrEmpty(text)) parameters.Add("text", text);
            if (follow) parameters.Add("follow", "1");
            return await API.CallMethodAsync<int>(this, parameters);
        }

        [Method("get")]
        public async Task<VKList<User>> GetAsync(List<string> fields, int userId = 0, FriendsOrder order = FriendsOrder.Hints, int listId = 0, int count = 5000, int offset = 0, NameCase nameCase = NameCase.Nom) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userId > 0) parameters.Add("user_id", userId.ToString());
            parameters.Add("order", order.ToEnumMemberAttribute());
            if (listId > 0) parameters.Add("list_id", listId.ToString());
            parameters.Add("count", count > 0 ? count.ToString() : "5000");
            if (offset > 0) parameters.Add("offset", offset.ToString());
            parameters.Add("fields", fields.Combine());
            parameters.Add("name_case", nameCase.ToEnumMemberAttribute());
            return await API.CallMethodAsync<VKList<User>>(this, parameters);
        }

        [Method("search")]
        public async Task<VKList<User>> SearchAsync(int userId, string query, int count = 1000, int offset = 0, List<string> fields = null, NameCase nameCase = NameCase.Nom) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("user_id", userId.ToString());
            parameters.Add("q", query);
            parameters.Add("count", count.ToString());
            if (offset > 0) parameters.Add("offset", offset.ToString());
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            parameters.Add("name_case", nameCase.ToEnumMemberAttribute());
            return await API.CallMethodAsync<VKList<User>>(this, parameters);
        }
    }
}