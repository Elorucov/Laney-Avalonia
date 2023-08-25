using ELOR.VKAPILib.Objects.Auth;

namespace ELOR.VKAPILib.Methods {
    public class AuthMethods : MethodsSectionBase {
        internal AuthMethods(VKAPI api) : base(api) { }

        public async Task<OauthResponse> GetOauthTokenAsync(int appId, int scope, string hash) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "app_id", appId.ToString() },
                { "scope", scope.ToString() },
                { "hash", hash },
            };
            return await API.CallMethodAsync<OauthResponse>("auth.getOauthToken", parameters);
        }
    }
}