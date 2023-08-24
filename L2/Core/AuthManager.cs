using Avalonia.Controls;
using Avalonia.Media;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Extensions;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects;
using Serilog;
//using OAuthWebView;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class AuthManager {
        const int APP_ID = 6614620;
        static Uri authUri = new Uri($"https://oauth.vk.com/authorize?client_id={APP_ID}&redirect_uri=https://oauth.vk.com/blank.html&scope=995414&response_type=token&revoke=1&v={VKAPI.Version}");
        static Uri finalUri = new Uri("https://oauth.vk.com/blank.html");
        static Uri finalUriOauth = new Uri("https://oauth.vk.com/auth_redirect");

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

            Launcher.LaunchUrl($"https://id.vk.com/auth?app_id=6614620&state=&response_type=token&redirect_uri=http%3A%2F%2Flocalhost%3A52639&redirect_uri_hash=7cffb58e0529406e09&code_challenge=&code_challenge_method=&return_auth_hash={hash}&scope=995414&force_hash=");
            string response = await LServer.StartAndReturnQueryFromClient(cts.Token);
            if (response.Length <= 1) return new Tuple<long, string>(userId, accessToken);
            var queries = response.Substring(1).ParseQuery();
            if (queries.ContainsKey("access_token") && queries.ContainsKey("user_id")) {
                userId = Int64.Parse(queries["user_id"]);
                accessToken = queries["access_token"];
            }

            return new Tuple<long, string>(userId, accessToken);
        }

        public static async Task<Tuple<long, string>> AuthWithTokenAsync(Window parentWindow, string errorText = null) {
            long userId = 0;
            string accessToken = String.Empty;

            string[] buttons = new string[] { "Continue", "Cancel" };
            VKUIDialog dlg = new VKUIDialog("Enter access token", "Нажмите \"Open auth page\", пройдите авторизацию в браузере, затем скопируйте значение access_token из адресной строки и вставьте в поле ввода ниже.", buttons, 1);
            TextBox tokenBox = new TextBox { Watermark = "access_token" };

            Button link = new Button { Content = "Open auth page", Margin = new Avalonia.Thickness(0, 0, 0, 8) };
            link.Classes.Add("Tertiary");
            link.Click += (a, b) => Launcher.LaunchUrl(authUri.ToString());

            StackPanel panel = new StackPanel();
            panel.Children.Add(link);
            panel.Children.Add(tokenBox);
            if (!String.IsNullOrEmpty(errorText)) panel.Children.Add(new TextBlock {
                Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                TextWrapping = TextWrapping.Wrap,
                Text = errorText,
            });
            dlg.DialogContent = panel;

            int id = await dlg.ShowDialog<int>(parentWindow);
            if (id == 1) {
                if (String.IsNullOrEmpty(tokenBox.Text)) return await AuthWithTokenAsync(parentWindow, "Enter token!");
                try {
                    VKAPI api = new VKAPI(0, tokenBox.Text, "ru", App.UserAgent);
                    api.WebRequestCallback = LNetExtensions.SendRequestToAPIViaLNetAsync;
                    var app = await api.Apps.GetAsync();
                    int appId = app.Items[0].Id;
                    if (appId != APP_ID) return await AuthWithTokenAsync(parentWindow, $"Wrong token! Required token from app{APP_ID}, but this is from app{appId}");
                    var user = await api.Users.GetAsync();
                    userId = user.Id;
                    accessToken = tokenBox.Text;
                } catch (APIException aex) {
                    return await AuthWithTokenAsync(parentWindow, $"VK API Error {aex.Code}: {aex.Message}");
                } catch (Exception ex) {
                    return await AuthWithTokenAsync(parentWindow, $"Exception ${ex.HResult.ToString("x8")}: {ex.Message}");
                }
            }

            return new Tuple<long, string>(userId, accessToken);
        }
    }
}
