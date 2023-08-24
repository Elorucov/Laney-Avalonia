using ELOR.VKAPILib.Objects.Auth;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib {
    public class DirectAuth {
        public static async Task<DirectAuthResponse> AuthAsync(VKAPI api, string lang, int clientId, string clientSecret, int scope, string username, string password, string code = null, string captchaSid = null, string captchaKey = null) {
            Dictionary<string, string> p = new Dictionary<string, string> {
                { "lang", lang },
                { "grant_type", "password" },
                { "client_id", clientId.ToString() },
                { "client_secret", clientSecret },
                { "scope", scope.ToString() },
                { "username", username },
                { "password", password },
                { "2fa_supported", "1" },
                { "v", "5.131" }, // в новых версиях офклиентам возвращается silent_token.
            };
            if (!String.IsNullOrEmpty(code)) p.Add("code", code);
            if (!String.IsNullOrEmpty(captchaSid)) p.Add("captcha_sid", captchaSid);
            if (!String.IsNullOrEmpty(captchaSid)) p.Add("captcha_key", captchaKey);

            string response = await api.SendRequestAsync("https://oauth.vk.com/token", p);

            DirectAuthResponse das = JsonSerializer.Deserialize(response, typeof(DirectAuthResponse), BuildInJsonContext.Default) as DirectAuthResponse;
            return das;
        }
    }
}