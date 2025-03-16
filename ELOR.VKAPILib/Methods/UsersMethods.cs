using ELOR.VKAPILib.Objects;
using System.Runtime.Serialization;

namespace ELOR.VKAPILib.Methods {


    public enum NameCase {
        [EnumMember(Value = "nom")]
        Nom,

        [EnumMember(Value = "gen")]
        Gen,

        [EnumMember(Value = "dat")]
        Dat,

        [EnumMember(Value = "acc")]
        Acc,

        [EnumMember(Value = "ins")]
        Ins,

        [EnumMember(Value = "abl")]
        Abl
    }

    public class UsersMethods : MethodsSectionBase {
        internal UsersMethods(VKAPI api) : base(api) { }

        /// <summary>Returns detailed information on users.</summary>
        /// <param name="userIds">User IDs.</param>
        /// <param name="fields">Profile fields to return.</param>
        /// <param name="nameCase">Case for declension of user name and surname.</param>
        public async Task<List<User>> GetAsync(List<long> ids, List<string> fields = null, NameCase nameCase = NameCase.Nom) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (!ids.IsNullOrEmpty()) parameters.Add("user_ids", ids.Combine());
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            parameters.Add("name_case", nameCase.ToEnumMemberAttribute());
            return await API.CallMethodAsync<List<User>>("users.get", parameters);
        }

        /// <summary>Returns detailed information on user.</summary>
        /// <param name="userId">User ID.</param>
        /// <param name="fields">Profile fields to return.</param>
        /// <param name="nameCase">Case for declension of user name and surname.</param>
        public async Task<User> GetAsync(long id = 0, List<string> fields = null, NameCase nameCase = NameCase.Nom) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (id != 0) parameters.Add("user_ids", id.ToString());
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            parameters.Add("name_case", nameCase.ToEnumMemberAttribute());
            return (await API.CallMethodAsync<List<User>>("users.get", parameters)).First();
        }
    }
}