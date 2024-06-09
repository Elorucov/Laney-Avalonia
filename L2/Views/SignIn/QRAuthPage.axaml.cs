using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib;
using System;
using VKUI.Controls;

namespace ELOR.Laney.Views.SignIn {
    public partial class QRAuthPage : Page {
        public QRAuthPage() {
            InitializeComponent();
        }

        VKAPI api;

        private async void BackButton_Click(object sender, RoutedEventArgs e) {
            await NavigationRouter.BackAsync();
        }

        private async void Page_Loaded(object? sender, RoutedEventArgs e) {
            try {
                // Get VKAPI instance with anonym token
                api = await DirectAuth.GetVKAPIWithAnonymTokenAsync(AuthManager.APP_ID, AuthManager.CLIENT_SECRET, App.UserAgent, LNetExtensions.SendRequestToAPIViaLNetAsync);

                // Get auth code
                var codeResp = await api.Auth.GetAuthCodeAsync(Localizer.Instance["lang"], $"Laney {App.BuildInfo} on {App.Platform}", AuthManager.APP_ID);
                QrCodeControl.Data = codeResp.AuthUrl;

                Loading.IsVisible = false;
                QrCodeControl.IsVisible = true;
            } catch (Exception ex) {
                await ExceptionHelper.ShowErrorDialogAsync(TopLevel.GetTopLevel(this) as Window, ex, true);
                await NavigationRouter.BackAsync();
            }
        }
    }
}