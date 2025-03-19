using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using System;
using System.Threading.Tasks;
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
                var codeResp = await api.Auth.GetAuthCodeAsync(Assets.i18n.Resources.lang, $"Laney {App.BuildInfo} on {App.Platform}", AuthManager.APP_ID);
                QrCodeControl.Data = codeResp.AuthUrl;

                Loading.IsVisible = false;
                QrCodeControl.IsVisible = true;

                Check(codeResp.AuthHash);
            } catch (Exception ex) {
                await ExceptionHelper.ShowErrorDialogAsync(TopLevel.GetTopLevel(this) as Window, ex, true);
                await NavigationRouter.BackAsync();
            }
        }

        private async void Check(string authHash) {
            await Task.Factory.StartNew(async () => {
                bool isWorking = true;
                while (isWorking) {
                    await Task.Delay(1500).ConfigureAwait(false);
                    try {
                        var response = await api.Auth.CheckAuthCodeAsync(Assets.i18n.Resources.lang, AuthManager.APP_ID, authHash);
                        if (response.Status >= 2) isWorking = false;
                        await Dispatcher.UIThread.InvokeAsync(async () => {
                            // PageDesc.Text = $"Status: {response.Status}";
                            switch (response.Status) {
                                case 1:
                                    Loading.IsVisible = true;
                                    QrCodeControl.IsVisible = false;
                                    PageDesc.Text = $"Now press the \"Confirm\" button on your device";
                                    break;
                                case 2:
                                    PageDesc.Text = $"Authorizing, please wait...";
                                    // TODO: navigate to PostDirectAuthPage and refucktor
                                    break;
                                case 3:
                                    await NavigationRouter.BackAsync();
                                    break;
                                default:
                                    PageDesc.Text = $"Status: {response.Status}";
                                    break;
                            }
                        });
                    } catch (Exception ex) {
                        isWorking = false;
                        await Dispatcher.UIThread.InvokeAsync(async () => {
                            await ExceptionHelper.ShowErrorDialogAsync(TopLevel.GetTopLevel(this) as Window, ex, true);
                            await NavigationRouter.BackAsync();
                        });
                        break;
                    }
                }
            });
        }
    }
}