using DeviceId;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects.Auth;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class AuthManager {
        // Client credentials for 3rd party app (by default, Laney)
        public const int CLIENT_ID = 6614620;
        public const string CLIENT_SECRET = "KdOeDEb0swoCpKLRTfKb";

        // Client credentials for VK's own apps (by default, VK Messenger). Обратите внимание, что:
        // 1. в этих полях должны быть креды (ID и secret) ТОЛЬКО официальных приложений ВК (ибо для них доступны все методы API messages),
        // 2. использование кредов мобильных приложений (в частности, официальные мобильные приложения) может привести к блокировке аккаунта, если не поменять User-Agent...
        // ...а ещё для них необходимо реализовать обновление токенов, чего L2 не поддерживает из-за ненадобности.
        public const int OFFICIAL_CLIENT_ID = 51453752;
        public const string OFFICIAL_CLIENT_SECRET = "4UyuCUsdK8pVCNoeQuGi";

        public const int SCOPE = 995414;
        static Uri authUri = new Uri($"https://oauth.vk.com/authorize?client_id={CLIENT_ID}&redirect_uri=https://oauth.vk.com/blank.html&scope={SCOPE}&response_type=token&revoke=1");

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
                var err = root.AsObject();
                string errtext = root["error_code"].GetValue<string>();
                if (err.ContainsKey("error_info")) errtext = root["error_info"].GetValue<string>();
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

                    var tempAPI = new VKAPI(token, Assets.i18n.Resources.lang, App.UserAgent);
                    tempAPI.WebRequestCallback = LNetExtensions.SendRequestToAPIViaLNetAsync;
                    return await tempAPI.Auth.GetOauthTokenAsync(appId, oauthScope, returnAuthHash, authUserHash);
                } else {
                    Log.Error($"DoConnectAuthAsync: Cannot get access_token and auth_user_hash! Full response: {root.ToJsonString()}");
                    throw new ApplicationException("Cannot get access_token and auth_user_hash, because one of these fields is not returned");
                }
            } else if (type == "error") {
                var err = root.AsObject();
                string errtext = root["error_code"].GetValue<string>();
                if (err.ContainsKey("error_info")) errtext = root["error_info"].GetValue<string>();
                Log.Error($"DoConnectAuthAsync: VK returns an error! Full response: {root.ToJsonString()}");
                throw new ApplicationException($"VK Authorization server (connect_internal) returns an error:\n{errtext}");
            } else {
                Log.Error($"DoConnectAuthAsync: VK returns a non-standart response! {root.ToJsonString()}");
                throw new ApplicationException($"VK Authorization server (connect_internal) returns a non-standart response");
            }
        }

    }
}
