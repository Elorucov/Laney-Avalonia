using ELOR.VKAPILib.Objects.Auth;

namespace ELOR.VKAPILib.Methods {
    public class AuthMethods : MethodsSectionBase {
        internal AuthMethods(VKAPI api) : base(api) { }

        public async Task<GetAuthCodeResponse> GetAuthCodeAsync(string lang, string deviceName, int clientId) {
            var parameters = new Dictionary<string, string> {
                { "lang", lang },
                { "device_name", deviceName },
                { "client_id", clientId.ToString() },
                { "force_regenerate", "1" },
                { "auth_code_flow", "0" }
            };

            return await API.CallMethodAsync<GetAuthCodeResponse>("auth.getAuthCode", parameters);
        }

        public async Task<CheckAuthCodeResponse> CheckAuthCodeAsync(string lang, int clientId, string hash, bool webAuth) {
            var parameters = new Dictionary<string, string> {
                { "lang", lang },
                { "web_auth", webAuth ? "1" : "0" },
                { "client_id", clientId.ToString() },
                { "auth_hash", hash },
            };

            return await API.CallMethodAsync<CheckAuthCodeResponse>("auth.checkAuthCode", parameters);
        }

        public async Task<OauthResponse> GetOauthTokenAsync(int appId, int scope, string hash, string authUserHash) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "app_id", appId.ToString() },
                { "client_id", appId.ToString() },
                { "scope", scope.ToString() },
                { "hash", hash },
                { "auth_user_hash", authUserHash },
                { "is_seamless_auth", "0" },
            };
            return await API.CallMethodAsync<OauthResponse>("auth.getOauthToken", parameters);
        }
    }
}