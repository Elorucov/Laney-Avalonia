using Avalonia.Controls;
using ELOR.Laney.Core;
using Serilog;
using System.Collections.Generic;
using VKUI.Controls;

namespace ELOR.Laney.Views.SignIn {
    public partial class PostDirectAuthPage : Page {

        public PostDirectAuthPage() {
            InitializeComponent();
        }

        public PostDirectAuthPage(long userId, string accessToken) {
            InitializeComponent();
            Log.Information($"{nameof(PostDirectAuthPage)} initialized. (id/token)");
            Loaded += (a, b) => SaveCredentials(userId, accessToken);
        }

        private void SaveCredentials(long userId, string accessToken) {
            var window = TopLevel.GetTopLevel(this) as Window;

            Log.Information($"{nameof(PostDirectAuthPage)}: Access token received for user {userId}!");

            Settings.SetBatch(new Dictionary<string, object> {
                        { Settings.VK_USER_ID, userId },
                        { Settings.VK_TOKEN, accessToken }
                    });
            VKSession.StartUserSession(userId, accessToken);
            App.Current.DesktopLifetime.MainWindow = VKSession.Main.Window;
            window.Close();
        }
    }
}