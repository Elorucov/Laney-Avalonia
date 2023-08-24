using ELOR.VKAPILib.Objects;

namespace ELOR.VKAPILib.Methods {
    public class AuthMethods : MethodsSectionBase {
        internal AuthMethods(VKAPI api) : base(api) { }

        //public async Task<AppsList> ValidateLoginAsync(string accessToken, string login) {
        //    Dictionary<string, string> parameters = new Dictionary<string, string> {
        //        { "login", login },
        //        { "access_token", accessToken }
        //    };
        //    return await API.CallMethodAsync<AppsList>("auth.validateLogin", parameters);
        //}
    }
}