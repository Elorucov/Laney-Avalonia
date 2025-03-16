using ELOR.VKAPILib.Objects.Auth;
using System.Text.Json;

namespace ELOR.VKAPILib {
    public class DirectAuth {
        public static async Task<VKAPI> GetVKAPIWithAnonymTokenAsync(int clientId, string clientSecret, string userAgent, Func<Uri, Dictionary<string, string>, Dictionary<string, string>, Task<HttpResponseMessage>> webRequestCallback = null) {
            Dictionary<string, string> p = new Dictionary<string, string> {
                { "client_id", clientId.ToString() },
                { "client_secret", clientSecret }
            };

            //var resp = await API.InternalRequestAsync("https://oauth.vk.com/get_anonym_token", p);

            //byte[] rarr = await resp.Content.ReadAsByteArrayAsync();
            //string response = Encoding.UTF8.GetString(rarr);
            //resp.Dispose();

            //AnonymToken at = JsonConvert.DeserializeObject<AnonymToken>(response);
            //return at;

            VKAPI api = new VKAPI(null, "en", userAgent);
            api.WebRequestCallback = webRequestCallback;

            string response = await api.SendRequestAsync(new Uri("https://oauth.vk.com/get_anonym_token"), p);
            AnonymToken atr = (AnonymToken)JsonSerializer.Deserialize(response, typeof(AnonymToken), BuildInJsonContext.Default);
            api.AccessToken = atr.Token;
            return api;
        }

        public static async Task<DirectAuthResponse> AuthAsync(VKAPI api, int clientId, string clientSecret, int scope, string username, string password, string code = null, string captchaSid = null, string captchaKey = null) {
            Dictionary<string, string> p = new Dictionary<string, string> {
                { "lang", api.Language },
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
            if (!String.IsNullOrEmpty(captchaKey)) p.Add("captcha_key", captchaKey);

            string response = await api.SendRequestAsync(new Uri("https://oauth.vk.com/token"), p);
            DirectAuthResponse das = (DirectAuthResponse)JsonSerializer.Deserialize(response, typeof(DirectAuthResponse), BuildInJsonContext.Default);
            return das;
        }
    }
}