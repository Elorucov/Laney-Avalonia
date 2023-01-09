using Avalonia.Controls;
using Avalonia.Media;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class AuthManager {
        const int APP_ID = 6614620;
        const string INTERNAL_PROTOCOL = "l2auth";
        static Uri authUri = new Uri($"https://oauth.vk.com/authorize?client_id={APP_ID}&display=windows_mobile&redirect_uri=https://oauth.vk.com/blank.html&scope=4666462&response_type=token&revoke=1&v=5.136");
        //static PhotinoWindow webView = null;

        //[STAThread]
        //public static async Task<string> AuthViaOauthWebViewAsync() {
        //    var httpClient = new HttpClient();
        //    string response = await httpClient.GetStringAsync(authUri.ToString());

        //    response = response.Replace("</body>", $"<script src=\"{INTERNAL_PROTOCOL}://dynamic.js\"></script></body>");

        //    webView = new PhotinoWindow();
        //    webView.SetContextMenuEnabled(false)
        //        .SetWidth(480).SetHeight(540)
        //        .SetResizable(false).Center()
        //        .RegisterWebMessageReceivedHandler(WebMsgHandler)
        //        .RegisterCustomSchemeHandler(INTERNAL_PROTOCOL, (object sender, string scheme, string url, out string contentType) => {
        //            contentType = "text/javascript";
        //            return new MemoryStream(Encoding.UTF8.GetBytes(@"
        //                (() =>{
        //                    window.setTimeout(() => {
        //                        window.external.sendMessage('Test');
        //                    }, 2000);
        //                })();
        //            "));
        //        })
        //        .LoadRawString(response).WaitForClose();
        //    return "OK!";
        //}

        //private static void WebMsgHandler(object? sender, string e) {
        //    var window = (PhotinoWindow)sender;
        //}

        public static async Task<Tuple<int, string>> AuthWithTokenAsync(Window parentWindow, string errorText = null) {
            int userId = 0;
            string accessToken = String.Empty;

            string[] buttons = new string[] { "Continue", "Cancel" };
            VKUIDialog dlg = new VKUIDialog("Enter access token", "WebView пока что не готов. Нажмите \"Open auth page\", пройдите авторизацию в браузере, затем скопируйте значение access_token из адресной строки и вставьте в поле ввода ниже.", buttons, 1);
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

            short id = await dlg.ShowDialog<short>(parentWindow);
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