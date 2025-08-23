using Avalonia.Controls;
using DeviceId;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects.Auth;
using Serilog;
//using OAuthWebView;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class AuthManager {
        public const int APP_ID = 6614620;
        public const string CLIENT_SECRET = "KdOeDEb0swoCpKLRTfKb";
        public const int SCOPE = 995414;
        static Uri authUri = new Uri($"https://oauth.vk.com/authorize?client_id={APP_ID}&redirect_uri=https://oauth.vk.com/blank.html&scope={SCOPE}&response_type=token&revoke=1");
        static Uri authUriLocalhost = new Uri($"https://oauth.vk.com/authorize?client_id={APP_ID}&redirect_uri=http://localhost:52639&scope=65536&response_type=token&revoke=1");
        //static Uri finalUri = new Uri("https://oauth.vk.com/blank.html");
        //static Uri finalUriOauth = new Uri("https://oauth.vk.com/auth_redirect");

        //public static async Task<Tuple<long, string>> AuthWithOAuthAsync(bool oauthWorkaround = false) {
        //    long userId = 0;
        //    string accessToken = String.Empty;

        //    OAuthWindow window = new OAuthWindow(authUri, oauthWorkaround ? finalUriOauth : finalUri, Assets.i18n.Resources.sign_in, 784, 541); // 768 + 16; 502 + 39;   Доп. 16 и 39 px надо будет прописать в либе oauth.
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
                using var resp = await LNet.GetAsync(authUri, headers: headers);
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

        private static async Task<string> GetVKIDAuthLinkAsync() {
            Dictionary<string, string> headers = new Dictionary<string, string> {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/115.0.1901.203" }
            };

            using var resp1 = await LNet.GetAsync(authUri, headers: headers);
            var vkIdUri = resp1.RequestMessage.RequestUri;
            Log.Information($"GetVKIDAuthLinkAsync: authUri is {resp1.RequestMessage.RequestUri.AbsoluteUri}");
            var q1 = vkIdUri.Query.Substring(1).ParseQuery();

            using var resp2 = await LNet.GetAsync(authUriLocalhost, headers: headers);
            Log.Information($"GetVKIDAuthLinkAsync: authUriLocalhost is {resp2.RequestMessage.RequestUri.AbsoluteUri}");
            var q2 = resp2.RequestMessage.RequestUri.Query.Substring(1).ParseQuery();

            if (q1.ContainsKey("return_auth_hash") && q2.ContainsKey("redirect_uri") && q2.ContainsKey("redirect_uri_hash")) {
                string newVkIdUri = vkIdUri.AbsoluteUri.Replace(q1["redirect_uri"], q2["redirect_uri"]).Replace(q1["redirect_uri_hash"], q2["redirect_uri_hash"]);
                return newVkIdUri;
            }
            Log.Error($"GetVKIDAuthLinkAsync: required parameters is not found in url!");
            return null;
        }

        public static async Task<Tuple<long, string>> AuthViaExternalBrowserAsync(Window window, CancellationTokenSource cts) {
            long userId = 0;
            string accessToken = String.Empty;
            string authUri = await GetVKIDAuthLinkAsync();
            if (String.IsNullOrEmpty(authUri)) throw new Exception("Auth URL for external browser is null!");

            await window.Launcher.LaunchUriAsync(new Uri(authUri));
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
                tempAPI = new VKAPI(null, Assets.i18n.Resources.lang, App.UserAgent);
                tempAPI.WebRequestCallback = LNetExtensions.SendRequestToAPIViaLNetAsync;
            }
            var response = await DirectAuth.AuthAsync(tempAPI, APP_ID, CLIENT_SECRET, SCOPE, login, password, code, captchaSid, captchaKey);
            return response;
        }

        public static async Task<OauthResponse> DoConnectCodeAuthAsync(string superAppToken, int appId, int oauthScope, string returnAuthHash) {
            var hr = await LNet.PostAsync(new Uri($"https://login.vk.com"), new Dictionary<string, string> {
                { "act", "connect_code_auth" },
                { "token", superAppToken },
                { "app_id", appId.ToString() },
                { "oauth_scope", oauthScope.ToString() },
                { "oauth_force_hash","1" },
                { "is_registration", "0" },
                { "oauth_response_type", "token" },
                { "is_oauth_migrated_flow", "1" },
                { "version", "1" },
                { "to", "aHR0cHM6Ly9vYXV0aC52ay5jb20vYmxhbmsuaHRtbA==" }
            }, new Dictionary<string, string> {
                { "Origin", $"https://id.vk.com" }
            });
            var response = await hr.Content.ReadAsStreamAsync();
            var root = await JsonNode.ParseAsync(response);

            string type = root["type"].GetValue<string>();
            if (type == "okay") {
                // var cookies = hr.Headers.Where(h => h.Key == "Set-Cookie").Select(h => h.Value).FirstOrDefault();
                return await DoConnectAuthAsync(appId, oauthScope, returnAuthHash);
            } else if (type == "error") {
                string errtext = root["error_text"].GetValue<string>();
                Log.Error($"DoConnectCodeAuthAsync: VK returns an error! {errtext}");
                throw new ApplicationException($"VK Authorization server (connect_code_auth) returns an error:\n{errtext}");
            } else {
                Log.Error($"DoConnectAuthAsync: VK returns a non-standart response! {root.ToJsonString()}");
                throw new ApplicationException($"VK Authorization server (connect_code_auth) returns a non-standart response");
            }
        }

        private static async Task<OauthResponse> DoConnectAuthAsync(int appId, int oauthScope, string returnAuthHash) {
            string deviceId = new DeviceIdBuilder()
                .AddMachineName()
                .AddOsVersion()
                .ToString();

            // string cookiestr = String.Join("\n", cookies);
            var hr = await LNet.PostAsync(new Uri($"https://login.vk.com"), new Dictionary<string, string> {
                { "act", "connect_internal" },
                { "app_id", appId.ToString() },
                { "uuid", "" },
                { "service_group","" },
                { "device_id", deviceId },
                { "oauth_version", "1" },
                { "version", "1" }
            }, new Dictionary<string, string> {
                { "Origin", $"https://id.vk.com" }
            });
            var response = await hr.Content.ReadAsStreamAsync();

            var root = await JsonNode.ParseAsync(response);
            string type = root["type"].GetValue<string>();
            if (type == "okay") {
                var data = root["data"].AsObject();
                if (data.ContainsKey("access_token") && data.ContainsKey("auth_user_hash")) {
                    string token = data["access_token"].GetValue<string>();
                    string authUserHash = data["auth_user_hash"].GetValue<string>();

                    if (tempAPI == null) {
                        tempAPI = new VKAPI(token, Assets.i18n.Resources.lang, App.UserAgent);
                        tempAPI.WebRequestCallback = LNetExtensions.SendRequestToAPIViaLNetAsync;
                    }

                    return await tempAPI.Auth.GetOauthTokenAsync(appId, oauthScope, returnAuthHash, authUserHash);
                } else {
                    Log.Error($"DoConnectAuthAsync: Cannot get access_token and auth_user_hash! Full response: {root.ToJsonString()}");
                    throw new ApplicationException("Cannot get access_token and auth_user_hash, because one of these fields is not returned");
                }
            } else if (type == "error") {
                string errtext = root["error_text"].GetValue<string>();
                Log.Error($"DoConnectAuthAsync: VK returns an error! Full response: {root.ToJsonString()}");
                throw new ApplicationException($"VK Authorization server (connect_internal) returns an error:\n{errtext}");
            } else {
                Log.Error($"DoConnectAuthAsync: VK returns a non-standart response! {root.ToJsonString()}");
                throw new ApplicationException($"VK Authorization server (connect_internal) returns a non-standart response");
            }
        }

    }
}
