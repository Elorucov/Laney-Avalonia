using ELOR.VKAPILib.Objects.Auth;
using System.Text.Json;

namespace ELOR.VKAPILib {
    public class DirectAuth {
        public static async Task<VKAPI> GetVKAPIWithAnonymTokenAsync(int clientId, string clientSecret, string userAgent, Func<Uri, Dictionary<string, string>, Dictionary<string, string>, Task<HttpResponseMessage>> webRequestCallback = null) {
            Dictionary<string, string> p = new Dictionary<string, string> {
                { "client_id", clientId.ToString() },
                { "client_secret", clientSecret }
            };

            VKAPI api = new VKAPI(null, "en", userAgent);
            api.WebRequestCallback = webRequestCallback;

            using var response = await api.SendRequestAsync(new Uri("https://oauth.vk.ru/get_anonym_token"), p);
            using var respStream = await response.ReadAsStreamAsync();

            AnonymToken atr = (AnonymToken)await JsonSerializer.DeserializeAsync(respStream, typeof(AnonymToken), BuildInJsonContext.Default);
            api.AccessToken = atr.Token;
            return api;
        }
    }
}