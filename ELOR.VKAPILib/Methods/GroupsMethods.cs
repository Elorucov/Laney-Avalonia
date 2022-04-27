using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Groups;

namespace ELOR.VKAPILib.Methods {
    [Section("groups")]
    public class GroupsMethods : MethodsSectionBase {
        internal GroupsMethods(VKAPI api) : base(api) { }

        /// <summary>Returns a list of the communities to which a user belongs.</summary>
        /// <param name="userId">User ID.</param>
        /// <param name="extended">true — to return complete information about a user's communities.</param>
        /// <param name="fields">Group fields to return.</param>
        /// <param name="filter">Types of communities to return.</param>
        /// <param name="offset">Offset needed to return a specific subset of communities.</param>
        /// <param name="count">Number of communities to return.</param>
        [Method("get")]
        public async Task<VKList<Group>> GetAsync(int userId, List<string> fields = null, List<string> filter = null, int offset = 0, int count = 1000) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("user_id", userId.ToString());
            parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            if (!filter.IsNullOrEmpty()) parameters.Add("filter", filter.Combine());
            parameters.Add("offset", offset.ToString());
            parameters.Add("count", count.ToString());
            return await API.CallMethodAsync<VKList<Group>>(this, parameters);
        }

        /// <summary>Returns a list of users on a community blacklist.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="offset">Offset needed to return a specific subset of communities.</param>
        /// <param name="count">Number of communities to return.</param>
        /// <param name="fields">Group fields to return.</param>
        [Method("getBanned")]
        public async Task<VKList<BannedMembers>> GetBannedAsync(int groupId, int offset = 0, int count = 20, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("group_id", groupId.ToString());
            parameters.Add("offset", offset.ToString());
            parameters.Add("count", count.ToString());
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<VKList<BannedMembers>>(this, parameters);
        }

        /// <summary>Returns information about communities by their IDs.</summary>
        /// <param name="groupIds">Group IDs.</param>
        /// <param name="fields">Group fields to return.</param>
        [Method("getById")]
        public async Task<GroupsResponse> GetByIdAsync(List<int> groupIds, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("group_ids", groupIds.Combine());
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<GroupsResponse>(this, parameters);
        }

        /// <summary>Returns information about community by ID.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="fields">Group fields to return.</param>
        [Method("getById")]
        public async Task<GroupsResponse> GetByIdAsync(int groupId, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("group_id", groupId.ToString());
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return (await API.CallMethodAsync<GroupsResponse>(this, parameters));
        }
    }
}