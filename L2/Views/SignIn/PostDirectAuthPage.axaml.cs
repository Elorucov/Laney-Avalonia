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
            Log.Information($"{nameof(PostDirectAuthPage)} initialized. (sat)");
            Loaded += (a, b) => FinishAuthProcess(superAppToken);
        }

        public PostDirectAuthPage(long userId, string accessToken) {
            InitializeComponent();
            Log.Information($"{nameof(PostDirectAuthPage)} initialized. (id/token)");
            Loaded += (a, b) => SaveCredentials(userId, accessToken);
        }

        private void SaveCredentials(long userId, string accessToken) {
            bool useOfficialClient = App.HasCmdLineValue("vkm");
            var window = TopLevel.GetTopLevel(this) as Window;

            Log.Information($"{nameof(PostDirectAuthPage)}: Access token received for user {userId}! VKM mode: {useOfficialClient}");

            Settings.SetBatch(new Dictionary<string, object> {
                        { Settings.VK_USER_ID, userId },
                        { Settings.VK_TOKEN, accessToken },
                        { Settings.IS_VKM_MODE, useOfficialClient }
                    });
            VKSession.StartUserSession(userId, accessToken);
            App.Current.DesktopLifetime.MainWindow = VKSession.Main.Window;
            window.Close();
        }

        private async void FinishAuthProcess(string superAppToken) {
            byte attempts = 0;
            Exception lastEx = null;

            do {
                if (attempts > 0) await Task.Delay(500).ConfigureAwait(false);
                attempts++;
                try {
                    Log.Information($"{nameof(PostDirectAuthPage)}: attempt {attempts}.");
                    var hash = await AuthManager.GetOauthHashAsync();
                    if (String.IsNullOrEmpty(hash)) continue;
                    Log.Information($"{nameof(PostDirectAuthPage)}: Oauth hash received!");

                    var response = await AuthManager.DoConnectCodeAuthAsync(superAppToken, AuthManager.CLIENT_ID, AuthManager.SCOPE, hash);
                    SaveCredentials(response.UserId, response.AccessToken);
                    break;
                } catch (Exception ex) {
                    Log.Error(ex, $"{nameof(PostDirectAuthPage)}: failed!");
                    lastEx = ex;
                }
            } while (attempts < 2);

            if (lastEx != null) {
                var window = TopLevel.GetTopLevel(this) as Window;
                var result = await ExceptionHelper.ShowErrorDialogAsync(window, lastEx, true);
                if (!result) await NavigationRouter.NavigateToAsync(new MainPage(), NavigationMode.Clear);
            }
        }
    }
}