using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Views.SignIn {
    public partial class PostDirectAuthPage : Page {
        public PostDirectAuthPage() {
            InitializeComponent();
        }

        public PostDirectAuthPage(long userId, string temporaryAccessToken) {
            InitializeComponent();
            Log.Information($"{nameof(PostDirectAuthPage)} initialized.");
            Loaded += (a, b) => FinishAuthProcess(userId, temporaryAccessToken);
        }

        private async void FinishAuthProcess(long userId, string temporaryAccessToken) {
            byte attempts = 0;
            Exception lastEx = null;
            Window window = TopLevel.GetTopLevel(this) as Window;

            VKAPI tempAPI = new VKAPI(temporaryAccessToken, Assets.i18n.Resources.lang, App.UserAgent);
            tempAPI.WebRequestCallback = LNetExtensions.SendRequestToAPIViaLNetAsync;
            do {
                if (attempts > 0) await Task.Delay(500).ConfigureAwait(false);
                attempts++;
                try {
                    Log.Information($"{nameof(PostDirectAuthPage)}: attempt {attempts}.");
                    var hash = await AuthManager.GetOauthHashAsync();
                    if (String.IsNullOrEmpty(hash)) continue;
                    Log.Information($"{nameof(PostDirectAuthPage)}: Oauth hash received!");

                    var response = await tempAPI.Auth.GetOauthTokenAsync(AuthManager.APP_ID, AuthManager.SCOPE, hash);
                    Log.Information($"{nameof(PostDirectAuthPage)}: Access token received!");
                    Settings.SetBatch(new Dictionary<string, object> {
                        { Settings.VK_USER_ID, response.UserId },
                        { Settings.VK_TOKEN, response.AccessToken }
                    });
                    VKSession.StartUserSession(response.UserId, response.AccessToken);
                    App.Current.DesktopLifetime.MainWindow = VKSession.Main.Window;
                    window.Close();
                    break;
                } catch (Exception ex) {
                    Log.Error(ex, $"{nameof(PostDirectAuthPage)}: failed!");
                    lastEx = ex;
                }
            } while (attempts < 10);

            if (lastEx != null) {
                var result = await ExceptionHelper.ShowErrorDialogAsync(window, lastEx, true);
                if (!result) await NavigationRouter.NavigateToAsync(new MainPage(), NavigationMode.Clear);
            }
        }
    }
}