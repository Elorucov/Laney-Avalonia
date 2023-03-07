using Avalonia.Controls;
using Avalonia.Media;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects;
using OAuthWebView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class AuthManager {
        const int APP_ID = 6614620;
        static Uri authUri = new Uri($"https://oauth.vk.com/authorize?client_id={APP_ID}&display=mobile&redirect_uri=https://oauth.vk.com/blank.html&scope=995414&response_type=token&revoke=1&v={VKAPI.Version}");
        static Uri finalUri = new Uri("https://oauth.vk.com/auth_redirect"); // почему не blank.html? Потому что у OauthWebView не детектит редирект туда.

        public static async Task<Tuple<int, string>> AuthWithOAuthAsync() {
            int userId = 0;
            string accessToken = String.Empty;

            OAuthWindow window = new OAuthWindow(authUri, finalUri, "OAuth", 640, 560);
            window.LocalDataPath = App.LocalDataPath;
            Uri url = await window.StartAuthenticationAsync();
            if (url == null) return new Tuple<int, string>(userId, accessToken);

            var queries = url.Query.Substring(1).ParseQuery();
            if (queries.ContainsKey("authorize_url")) {
                Uri finalUri = new Uri(WebUtility.UrlDecode(queries["authorize_url"]));
                var finalQueries = finalUri.Fragment.Substring(1).ParseQuery();
                if (finalQueries.ContainsKey("access_token") && finalQueries.ContainsKey("user_id")) {
                    userId = Int32.Parse(finalQueries["user_id"]);
                    accessToken = finalQueries["access_token"];
                }
            }

            return new Tuple<int, string>(userId, accessToken);
        }

        public static async Task<Tuple<int, string>> AuthWithTokenAsync(Window parentWindow, string errorText = null) {
            int userId = 0;
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

            return new Tuple<int, string>(userId, accessToken);
        }
    }
}
