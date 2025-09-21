using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects.Auth;
using System;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Views.SignIn {
    public partial class QRAuthPage : Page {
        bool _useOfficialClient;

        public QRAuthPage() {
            InitializeComponent();
            _useOfficialClient = App.HasCmdLineValue("vkm");
        }

        VKAPI api;

        private async void BackButton_Click(object sender, RoutedEventArgs e) {
            await NavigationRouter.BackAsync();
        }

        private async void Page_Loaded(object? sender, RoutedEventArgs e) {
            try {
                // Get VKAPI instance with anonym token
                if (_useOfficialClient) {
                    api = await DirectAuth.GetVKAPIWithAnonymTokenAsync(AuthManager.OFFICIAL_CLIENT_ID, AuthManager.OFFICIAL_CLIENT_SECRET, App.UserAgent, LNetExtensions.SendRequestToAPIViaLNetAsync);
                } else {
                    api = await DirectAuth.GetVKAPIWithAnonymTokenAsync(AuthManager.CLIENT_ID, AuthManager.CLIENT_SECRET, App.UserAgent, LNetExtensions.SendRequestToAPIViaLNetAsync);
                }

                // Get auth code
                GetAuthCodeResponse codeResp = null;

                if (_useOfficialClient) {
                    codeResp = await api.Auth.GetAuthCodeAsync(Assets.i18n.Resources.lang, $"Laney {App.BuildInfo} (VKM mode) on {App.Platform}", AuthManager.OFFICIAL_CLIENT_ID);
                } else {
                    codeResp = await api.Auth.GetAuthCodeAsync(Assets.i18n.Resources.lang, $"Laney {App.BuildInfo} on {App.Platform}", AuthManager.CLIENT_ID);
                }

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
                        var response = await api.Auth.CheckAuthCodeAsync(Assets.i18n.Resources.lang,
                            _useOfficialClient ? AuthManager.CLIENT_ID : AuthManager.OFFICIAL_CLIENT_ID,
                            authHash, !_useOfficialClient);

                        if (response.Status >= 2) isWorking = false;
                        await Dispatcher.UIThread.InvokeAsync(async () => {
                            switch (response.Status) {
                                case 1:
                                    Loading.IsVisible = true;
                                    QrCodeControl.IsVisible = false;
                                    PageTitle.Text = Assets.i18n.Resources.qr_signin_p2_title;
                                    PageDesc.Text = Assets.i18n.Resources.qr_signin_p2_desc;
                                    break;
                                case 2:
                                    if (_useOfficialClient) {
                                        await NavigationRouter.NavigateToAsync(new PostDirectAuthPage(response.UserId, response.AccessToken));
                                    } else {
                                        await NavigationRouter.NavigateToAsync(new PostDirectAuthPage(response.SuperAppToken));
                                    }
                                    break;
                                case 3:
                                    await NavigationRouter.BackAsync();
                                    break;
                                default:
                                    if (response.Status != 0) PageDesc.Text = $"Status: {response.Status}";
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