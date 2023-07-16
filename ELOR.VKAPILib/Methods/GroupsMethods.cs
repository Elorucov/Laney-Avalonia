using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Groups;

namespace ELOR.VKAPILib.Methods {
    public class GroupsMethods : MethodsSectionBase {
        internal GroupsMethods(VKAPI api) : base(api) { }

        // TODO: когда будет возможность самому добавлять группы, тогда и переделаем.


        ///// <summary>Returns a list of the communities to which a user belongs.</summary>
        ///// <param name="userId">User ID.</param>
        ///// <param name="extended">true — to return complete information about a user's communities.</param>
        ///// <param name="fields">Group fields to return.</param>
        ///// <param name="filter">Types of communities to return.</param>
        ///// <param name="offset">Offset needed to return a specific subset of communities.</param>
        ///// <param name="count">Number of communities to return.</param>
        //public async Task<VKList<Group>> GetAsync(long userId, List<string> fields = null, List<string> filter = null, int offset = 0, int count = 1000) {
        //    Dictionary<string, string> parameters = new Dictionary<string, string> {
        //        { "user_id", userId.ToString() },
        //        { "extended", "1" }
        //    };
        //    if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
        //    if (!filter.IsNullOrEmpty()) parameters.Add("filter", filter.Combine());
        //    parameters.Add("offset", offset.ToString());
        //    parameters.Add("count", count.ToString());
        //    return await API.CallMethodAsync<VKList<Group>>("groups.get", parameters);
        //}

        ///// <summary>Returns a list of users on a community blacklist.</summary>
        ///// <param name="groupId">Group ID.</param>
        ///// <param name="offset">Offset needed to return a specific subset of communities.</param>
        ///// <param name="count">Number of communities to return.</param>
        ///// <param name="fields">Group fields to return.</param>
        //public async Task<VKList<BannedMembers>> GetBannedAsync(long groupId, int offset = 0, int count = 20, List<string> fields = null) {
        //    Dictionary<string, string> parameters = new Dictionary<string, string> {
        //        { "group_id", groupId.ToString() },
        //        { "offset", offset.ToString() },
        //        { "count", count.ToString() }
        //    };
        //    if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
        //    return await API.CallMethodAsync<VKList<BannedMembers>>("groups.getBanned", parameters);
        //}
    }
}