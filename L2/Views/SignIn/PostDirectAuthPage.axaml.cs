using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
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

        public PostDirectAuthPage(string superAppToken) {
            InitializeComponent();
            Log.Information($"{nameof(PostDirectAuthPage)} initialized.");
            Loaded += (a, b) => FinishAuthProcess(superAppToken);
        }

        private async void FinishAuthProcess(string superAppToken) {
            byte attempts = 0;
            Exception lastEx = null;
            Window window = TopLevel.GetTopLevel(this) as Window;

            do {
                if (attempts > 0) await Task.Delay(500).ConfigureAwait(false);
                attempts++;
                try {
                    Log.Information($"{nameof(PostDirectAuthPage)}: attempt {attempts}.");
                    var hash = await AuthManager.GetOauthHashAsync();
                    if (String.IsNullOrEmpty(hash)) continue;
                    Log.Information($"{nameof(PostDirectAuthPage)}: Oauth hash received!");

                    var response = await AuthManager.DoConnectCodeAuthAsync(superAppToken, AuthManager.APP_ID, AuthManager.SCOPE, hash);
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
            } while (attempts < 2);

            if (lastEx != null) {
                var result = await ExceptionHelper.ShowErrorDialogAsync(window, lastEx, true);
                if (!result) await NavigationRouter.NavigateToAsync(new MainPage(), NavigationMode.Clear);
            }
        }
    }
}