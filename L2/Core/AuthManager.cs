using ELOR.Laney.Core.Localization;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects.Auth;
using Serilog;
//using OAuthWebView;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class AuthManager {
        public const int APP_ID = 6614620;
        const string CLIENT_SECRET = "KdOeDEb0swoCpKLRTfKb";
        public const int SCOPE = 995414;
        static Uri authUri = new Uri($"https://oauth.vk.com/authorize?client_id={APP_ID}&redirect_uri=https://oauth.vk.com/blank.html&scope={SCOPE}&response_type=token&revoke=1&v={VKAPI.Version}");
        //static Uri finalUri = new Uri("https://oauth.vk.com/blank.html");
        //static Uri finalUriOauth = new Uri("https://oauth.vk.com/auth_redirect");

        //public static async Task<Tuple<long, string>> AuthWithOAuthAsync(bool oauthWorkaround = false) {
        //    long userId = 0;
        //    string accessToken = String.Empty;

        //    OAuthWindow window = new OAuthWindow(authUri, oauthWorkaround ? finalUriOauth : finalUri, Localizer.Instance["sign_in"], 784, 541); // 768 + 16; 502 + 39;   Доп. 16 и 39 px надо будет прописать в либе oauth.
        //    window.LocalDataPath = App.LocalDataPath;
        //    Uri url = await window.StartAuthenticationAsync();
        //    if (url == null) return new Tuple<long, string>(userId, accessToken);

        //    if (url.Fragment.Length <= 1) return new Tuple<long, string>(userId, accessToken);
        //    var queries = url.Fragment.Substring(1).ParseQuery();
        //    if (!oauthWorkaround && queries.ContainsKey("access_token") && queries.ContainsKey("user_id")) {
        //        userId = Int64.Parse(queries["user_id"]);
        //        accessToken = queries["access_token"];
        //    } else if (oauthWorkaround && queries.ContainsKey("authorize_url")) {
        //        Uri finalUri = new Uri(WebUtility.UrlDecode(queries["authorize_url"]));
        //        var finalQueries = finalUri.Fragment.Substring(1).ParseQuery();
        //        if (finalQueries.ContainsKey("access_token") && finalQueries.ContainsKey("user_id")) {
        //            userId = Int32.Parse(finalQueries["user_id"]);
        //            accessToken = finalQueries["access_token"];
        //        }
        //    }

        //    return new Tuple<long, string>(userId, accessToken);
        //}

        public static async Task<string> GetOauthHashAsync() {
            Dictionary<string, string> headers = new Dictionary<string, string> {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/115.0.1901.203" }
            };

            try {
                var resp = await LNet.GetAsync(authUri, headers: headers);
                var q = resp.RequestMessage.RequestUri.Query.Substring(1).ParseQuery();
                if (q.ContainsKey("return_auth_hash")) {
                    Log.Information($"GetOauthHashAsync: successfully fetch a return_auth_hash: {q["return_auth_hash"]}.");
                    return q["return_auth_hash"];
                }
                Log.Error($"GetOauthHashAsync: return_auth_hash is not found in url!");
                return null;
            } catch (Exception ex) {
                Log.Information($"GetOauthHashAsync: failed to fetch a return_auth_hash! 0x{ex.HResult.ToString("x8")}: {ex.Message.Trim()}");
                return null;
            }
        }

        public static async Task<Tuple<long, string>> AuthViaExternalBrowserAsync(CancellationTokenSource cts, string hash) {
            long userId = 0;
            string accessToken = String.Empty;

            Launcher.LaunchUrl($"https://id.vk.com/auth?app_id=6614620&state=&response_type=token&redirect_uri=http%3A%2F%2Flocalhost%3A52639&redirect_uri_hash=7cffb58e0529406e09&code_challenge=&code_challenge_method=&return_auth_hash={hash}&scope={SCOPE}&force_hash=");
            string response = await LServer.StartAndReturnQueryFromClient(cts.Token);
            if (response.Length <= 1) return new Tuple<long, string>(userId, accessToken);
            var queries = response.Substring(1).ParseQuery();
            if (queries.ContainsKey("access_token") && queries.ContainsKey("user_id")) {
                userId = Int64.Parse(queries["user_id"]);
                accessToken = queries["access_token"];
            }

            return new Tuple<long, string>(userId, accessToken);
        }

        static VKAPI tempAPI;
        public static async Task<DirectAuthResponse> AuthViaLoginAndPasswordAsync(string login, string password, string code = null, string captchaSid = null, string captchaKey = null) {
            if (tempAPI == null) {
                tempAPI = new VKAPI(0, null, Localizer.Instance["lang"], App.UserAgent);
                tempAPI.WebRequestCallback = LNetExtensions.SendRequestToAPIViaLNetAsync;
            }
            var response = await DirectAuth.AuthAsync(tempAPI, APP_ID, CLIENT_SECRET, SCOPE, login, password, code, captchaSid, captchaKey);
            return response;
        }
    }
}
